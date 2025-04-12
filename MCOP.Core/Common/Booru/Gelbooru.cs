using MCOP.Core.Exceptions;
using Newtonsoft.Json.Linq;
using Serilog;

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
                throw new McopException("Gelbooru. Ничего не найдено");
            }

            JObject json = JObject.Parse(strJson);
            Log.Debug("Gelbooru Json" + Environment.NewLine + json.ToString(Newtonsoft.Json.Formatting.Indented));
            var jsonPosts = json.SelectToken("post");

            SearchResult searchResult = new SearchResult();

            if (jsonPosts is null)
            {
                throw new McopException("Gelbooru. Ничего не найдено");
            }

            Parallel.ForEach(jsonPosts.Children(), (post) =>
            {
                try
                {
                    var id = post["id"]?.Value<string>() ?? throw new McopException("Id token not found");
                    var filetype = post["image"]?.Value<string>() ?? throw new McopException("Image token not found");
                    filetype = filetype[(filetype.LastIndexOf('.') + 1)..];

                    searchResult.AddPost(new BooruPost
                    {
                        FileType = filetype,
                        ID = id,
                        MD5 = post["md5"]?.Value<string>() ?? throw new McopException("Md5 token not found"),
                        ImageUrl = post["file_url"]?.Value<string>() ?? throw new McopException("File url token not found"),
                        PreviewUrl = post["preview_url"]?.Value<string>() ?? throw new McopException("Preview url token not found"),
                        PostUrl = $"https://gelbooru.com/index.php?page=post&s=view&id={id}",
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
                    string error = "Gelbooru. Запрос не удался";
                    Log.Warning(error);
                    throw new McopException(error);
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
