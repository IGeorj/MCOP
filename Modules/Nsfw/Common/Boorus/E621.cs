using MCOP.Services;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.Nsfw.Common
{
    public sealed class E621
    {
        private static readonly string _baseUrl = "https://e621.net";
        private static readonly string _searchUrl = "https://e621.net/posts.json?";

        private readonly string _password;

        public E621(string password)
        {
            _password = password;
        }

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
                throw new Exception("Sankaku. Nothing found");
            }

            JObject json = JObject.Parse(strJson);

#pragma warning disable CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.
            SearchResult searchResult = new SearchResult();

            Parallel.ForEach(json.Children(), (post) =>
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", "Basic " + _password);


                var response = await HttpService.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string error = "E621. Can't get posts";
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
