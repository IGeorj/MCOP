using MCOP.Services;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

namespace MCOP.Modules.Nsfw.Common
{
    public sealed class Sankaku
    {
        private static readonly string _authUrl = "/auth/token";
        private static readonly string _searchUrl = "/posts/keyset?lang=en&default_threshold=1&hide_posts_in_books=never";


        private string? _accessToken;
        private string? _refreshToken;
        private string? _login;
        private string? _password;
        
        private readonly HttpClient _httpClient;
        public Sankaku(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AuthorizeAsync(string login, string password)
        {
            SetLogin(login);
            SetPassword(password);
            await RefreshTokenAsync();
        }

        public string? GetAcessToken()
        {
            return _accessToken;
        }

        public void SetLogin(string login)
        {
            _login = login;
        }

        public void SetPassword(string password)
        {
            _password = password;
        }

        public async Task<bool> IsAvailable()
        {
            var response = await _httpClient.GetAsync(_authUrl);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            Log.Warning("Sankaku is down...");
            return false;
        }

        private async Task RefreshTokenAsync()
        {
            if (!await IsAvailable())
            {
                return;
            }

            try
            {
                Log.Information("Get Sankaku tokens...");

                JObject jsonAuth = new()
                {
                    { "login", _login },
                    { "password", _password }
                };

                HttpContent content = new StringContent(jsonAuth.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(_authUrl, content);
                string strJson = await response.Content.ReadAsStringAsync();

                JObject jsonUser = JObject.Parse(strJson);
                _accessToken = (string?)jsonUser["access_token"];
                _refreshToken = (string?)jsonUser["refresh_token"];

                if (_accessToken is null || _refreshToken is null)
                {
                    throw new Exception("Failed to parse token. Null reference exception");
                }

                Log.Information($"Success!");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to update token. Error: {error}", ex.Message);
            }
        }

        private Task<SearchResult> ParseJsonAsync(string strJson)
        {
            if (strJson == "[]")
            {
                // TODO: Tags prediction
                throw new Exception("Sankaku. Nothing found");
            }

            JObject json = JObject.Parse(strJson);
            JToken? meta = json.SelectToken("meta");
            JToken? posts = json.SelectToken("data");

#pragma warning disable CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.
            SearchResult searchResult = new SearchResult();
            searchResult.SetPrev((string?)meta["prev"]);
            searchResult.SetNext((string?)meta["next"]);

            Parallel.ForEach(posts.Children(), (post) =>
            {
                JToken artist = post["tags"].FirstOrDefault(t => (int)t["type"] == 1);
                string filetype = (string)post["file_type"];
                filetype = filetype[(filetype.LastIndexOf('/') + 1)..];

                searchResult.AddPost(new BooruPost
                {
                    FileType = filetype,
                    ID = (string)post["id"],
                    MD5 = (string)post["md5"],
                    PreviewUrl = (string)post["preview_url"],
                    ImageUrl = (string)post["file_url"],
                    PostUrl = $"https://beta.sankakucomplex.com/post/show/{(string)post["id"]}",
                    Artist = artist == null ? "Автор не найден" : (string)artist["name"],
                    ParentId = (string?)post["parent_id"]
                });
            });
#pragma warning restore CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.

            return Task.FromResult(searchResult);
        }

        private async Task<SearchResult> SearchBaseAsync(string url)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    await RefreshTokenAsync();
                    HttpRequestMessage requestAfterRefresh = new HttpRequestMessage(HttpMethod.Get, url);
                    requestAfterRefresh.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                    response = await _httpClient.SendAsync(requestAfterRefresh);
                }

                if (!response.IsSuccessStatusCode)
                {
                    string error = "Sankaku. Can't get posts";
                    Log.Warning(error);
                    throw new Exception(error);
                }

                string strJson = await response.Content.ReadAsStringAsync();

                SearchResult searchResult = await ParseJsonAsync(strJson);
                searchResult.Sort();

                return searchResult;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<SearchResult> SearchAsync(int limit = 40)
        {
            string url = $"{_searchUrl}&limit={limit}";

            try
            {
                return await SearchBaseAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchResult> SearchAsync(string tags, int limit = 40)
        {
            string url = $"{_searchUrl}&tags={tags}&limit={limit}";

            try
            {
                return await SearchBaseAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchResult> SearchAsync(string tags, string next, int limit = 40)
        {
            string url = $"{_searchUrl}&tags={tags}&limit={limit}&next={next}";

            try
            {
                return await SearchBaseAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchResult> GetRandomAsync(string tags, int limit = 40)
        {
            tags += " order:random";

            try
            {
                return await SearchAsync(tags, limit);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchResult> GetRandomAsync(string tags, string next, int limit = 40)
        {
            tags += " order:random";

            try
            {
                return await SearchAsync(tags, next, limit);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchResult> GetDailyTopAsync(string tags, int limit = 40, int days = 1)
        {
            DateTime date = DateTime.Today;
            tags = $"{tags} order:popularity date:{date.AddDays(-days):yyyy-MM-ddTHH:mm}";
            string url = $"{_searchUrl}&tags={tags}&limit={limit}";

            try
            {
                return await SearchBaseAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SearchResult> GetDailyTopAsync(string tags, string next, int limit = 40, int days = 1)
        {
            DateTime date = DateTime.Today;
            tags = $"{tags} order:popularity date:{date.AddDays(-days):yyyy-MM-ddTHH:mm}";
            string url = $"{_searchUrl}&tags={tags}&limit={limit}&next={next}";

            try
            {
                return await SearchBaseAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
