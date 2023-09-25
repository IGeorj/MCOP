using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Core.Common.Booru
{
    public sealed class E621
    {
        private static readonly string _searchUrl = "/posts.json?";

        private readonly string _password;
        private readonly HttpClient _httpClient;

        public E621(string password, HttpClient httpClient)
        {
            _password = password;
            _httpClient = httpClient;
        }

        public async Task<bool> IsAvailable()
        {
            var response = await _httpClient.GetAsync(_searchUrl);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            Log.Warning("Gelbooru is down...");
            return false;
        }


        private Task<SearchResult> ParseJsonAsync(string strJson)
        {
            if (strJson == "[]")
            {
                // TODO: Tags prediction
                throw new Exception("E621. Nothing found");
            }

            JObject json = JObject.Parse(strJson);
            Log.Debug("E621 Json" + Environment.NewLine + json.ToString(Newtonsoft.Json.Formatting.Indented));
            JToken? posts = json.SelectToken("posts");

            SearchResult searchResult = new SearchResult();

            if (posts is null)
            {
                throw new Exception("E621. Nothing found");
            }

            Parallel.ForEach(posts.Children(), (post) =>
            {
                try
                {
                    var fileToken = post["file"] ?? throw new Exception("File token not found");
                    var url = fileToken["url"]?.Value<string>() ?? throw new Exception("Url token not found");
                    var filetype = url[(url.LastIndexOf('.') + 1)..];
                    var tagsToken = post["tags"] ?? throw new Exception("Tags token not found");
                    var artistToken = tagsToken["artist"] ?? throw new Exception("Artist token not found");
                    var id = post["id"]?.Value<string>() ?? throw new Exception("Id token not found");

                    searchResult.AddPost(new BooruPost
                    {
                        FileType = filetype,
                        ID = id,
                        MD5 = fileToken["md5"]?.Value<string>() ?? throw new Exception("Md5 token not found"),
                        ImageUrl = url,
                        PreviewUrl = post["preview"]?.SelectToken("url")?.Value<string>() ?? throw new Exception("Preview url token not found"),
                        PostUrl = $"https://e621.net/posts{id}",
                        Artist = artistToken?.Children().FirstOrDefault()?.Value<string>() ?? "Автор не найден",
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
                url = url + $"&login=georj&api_key={_password}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);


                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string error = "E621. Can't get posts";
                    Log.Warning(error);
                    throw new Exception(error);
                }

                Log.Information(url);

                string strJson = await response.Content.ReadAsStringAsync();

                SearchResult searchResult = await ParseJsonAsync(strJson);

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
            string url = $"{_searchUrl}&tags={tags}&limit={limit}&page={next}";

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
    }
}
