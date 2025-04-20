using DSharpPlus.Entities;
using MCOP.Core.Exceptions;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

namespace MCOP.Core.Common.Booru
{
    public sealed class Sankaku
    {
        private static readonly string _authUrl = "/auth/token";
        private static readonly string _searchUrl = "/posts/keyset?lang=en&default_threshold=0&hide_posts_in_books=never";


        private string? _accessToken;
        private string? _refreshToken;
        private string? _login;
        private string? _password;

        public HttpClient HttpClient { get; init; }
        public Sankaku(HttpClient httpClient)
        {
            HttpClient = httpClient;
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
            var response = await HttpClient.GetAsync(_authUrl);

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

                HttpResponseMessage response = await HttpClient.PostAsync(_authUrl, content);
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
                throw new McopException("Sankaku. Ахтунг, вообще ничего, сайт умер?");
            }

            JObject json = JObject.Parse(strJson);
            JToken? meta = json.SelectToken("meta");
            JToken? posts = json.SelectToken("data");

            SearchResult searchResult = new SearchResult();

            if (meta is not null)
            {
                searchResult.SetPrev((string?)meta["prev"]);
                searchResult.SetNext((string?)meta["next"]);
            }

            if (posts is null || !posts.Any())
            {
                throw new McopException("Sankaku. Ничего не найдено");
            }

            Parallel.ForEach(posts.Children(), (post) =>
            {
                try
                {
                    List<Tag> tags = new List<Tag>();
                    foreach (var tag in post["tags"] ?? throw new McopException("Sankaku. Tags не найден, нужен фикс"))
                    {
                        tags.Add(new Tag
                        {
                            Id = tag["id"]?.Value<string>() ?? throw new McopException("Sankaku. Tag Id не найден, нужен фикс"),
                            Name = tag["tagName"]?.Value<string>() ?? throw new McopException("Sankaku. Tag Name не найден, нужен фикс"),
                            Type = tag["type"]?.Value<int>() ?? throw new McopException("Sankaku. Tag Type не найден, нужен фикс")
                        });
                    }

                    string id = post["id"]?.Value<string>() ?? throw new McopException("Sankaku. Post Id не найден, нужен фикс");
                    string artist = tags.FirstOrDefault(x => x.Type == 1)?.Name ?? "Автор не найден";
                    string filetype = post["file_type"]?.Value<string>() ?? throw new McopException("Sankaku. File Type не найден, нужен фикс");
                    filetype = filetype[(filetype.LastIndexOf('/') + 1)..];

                    searchResult.AddPost(new BooruPost
                    {
                        FileType = filetype,
                        ID = id,
                        MD5 = post["md5"]?.Value<string>() ?? throw new McopException("Sankaku. MD5 не найден, нужен фикс"),
                        PreviewUrl = post["preview_url"]?.Value<string>() ?? throw new McopException("Sankaku. Preview URL не найден, нужен фикс"),
                        ImageUrl = post["file_url"]?.Value<string>() ?? throw new McopException("Sankaku. File URL не найден, нужен фикс"),
                        PostUrl = $"https://beta.sankakucomplex.com/post/show/{id}",
                        Artist = artist,
                        ParentId = (string?)post["parent_id"],
                        Tags = tags
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e, posts.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            });

            return Task.FromResult(searchResult);
        }

        private async Task<SearchResult> SearchBaseAsync(string url)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                Log.Information(HttpClient.BaseAddress + url);

                var response = await HttpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    await RefreshTokenAsync();
                    HttpRequestMessage requestAfterRefresh = new HttpRequestMessage(HttpMethod.Get, url);
                    requestAfterRefresh.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                    response = await HttpClient.SendAsync(requestAfterRefresh);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    string error = $"Sankaku. Запрос не удался несколько раз, {response.StatusCode} \n {responseContent}";
                    throw new McopException(error);
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
            await RefreshTokenAsync();

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
            await RefreshTokenAsync();

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

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> GetSuggestionsAsync(string tag)
        {
            try
            {
                var url = $"/tags/autosuggestCreating?lang=en&tag={tag}&show_meta=1&target=post";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await HttpClient.SendAsync(request);

                Log.Information(HttpClient.BaseAddress + url);

                string strJson = await response.Content.ReadAsStringAsync();

                if (strJson == "[]")
                {
                    return [];
                }

                List<DiscordAutoCompleteChoice> choices = new List<DiscordAutoCompleteChoice>();

                JArray jArray = JArray.Parse(strJson);
                foreach (var item in jArray)
                {
                    string name = item["name"]?.Value<string>() ?? throw new Exception("Name token not found");
                    int count = item["count"]?.Value<int>() ?? throw new Exception("Name token not found");
                    choices.Add(new DiscordAutoCompleteChoice($"{name} ({count} шт.)", name));
                }

                return choices;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
