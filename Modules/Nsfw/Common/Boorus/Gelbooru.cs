using MCOP.Services;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.Nsfw.Common
{
    public sealed class Gelbooru
    {
        private static readonly string _baseUrl = "https://gelbooru.com";
        private static readonly string _searchUrl = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1";

        public static async Task<bool> IsAvailable()
        {
            var response = await HttpService.GetAsync(_baseUrl);

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
            var jsonPosts = json.SelectToken("post");

#pragma warning disable CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.
            SearchResult searchResult = new SearchResult();

            Parallel.ForEach(jsonPosts.Children(), (post) =>
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
            });
#pragma warning restore CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.

            return Task.FromResult(searchResult);
        }

        private async Task<SearchResult> SearchBaseAsync(string url)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await HttpService.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string error = "Gelbooru. Can't get posts";
                    Log.Warning(error);
                    throw new Exception(error);
                }

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
