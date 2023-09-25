using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections.Concurrent;

namespace MCOP.Core.Common.Booru
{
    public sealed class Gelbooru
    {
        private static readonly string _searchUrl = "/index.php?page=dapi&s=post&q=index&json=1";

        private HttpClient _httpClient;

        public Gelbooru(HttpClient httpClient)
        {
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
                throw new Exception("Gelbooru. Nothing found");
            }

            JObject json = JObject.Parse(strJson);
            Log.Debug("Gelbooru Json" + Environment.NewLine + json.ToString(Newtonsoft.Json.Formatting.Indented));
            var jsonPosts = json.SelectToken("post");

            SearchResult searchResult = new SearchResult();

            if (jsonPosts is null)
            {
                throw new Exception("Gelbooru. Nothing found");
            }

            Parallel.ForEach(jsonPosts.Children(), (post) =>
            {
                var exceptions = new ConcurrentQueue<Exception>();
                try
                {
                    var filetype = post["image"].Value<string>();
                    filetype = filetype[(filetype.LastIndexOf('.') + 1)..];
                    searchResult.AddPost(new BooruPost
                    {
                        FileType = filetype,
                        ID = (string)post["id"],
                        MD5 = (string)post["md5"],
                        ImageUrl = (string)post["file_url"],
                        PreviewUrl = (string)post["preview_url"],
                        PostUrl = $"https://gelbooru.com/index.php?page=post&s=view&id={(string)post["id"]}",
                        Artist = "Автор не найден",
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e, jsonPosts.ToString(Newtonsoft.Json.Formatting.Indented));
                }

            });

            return Task.FromResult(searchResult);
        }

        private async Task<SearchResult> SearchBaseAsync(string url)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string error = "Gelbooru. Can't get posts";
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
            string url = $"{_searchUrl}&tags={tags}&limit={limit}&pid={next}";

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
            tags += " sort:random";

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
            tags += " sort:random";

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
