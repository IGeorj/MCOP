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
                throw new Exception("Sankaku. Nothing found");
            }

            JObject json = JObject.Parse(strJson);

#pragma warning disable CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.
            SearchResult searchResult = new SearchResult();

            Parallel.ForEach(json["posts"].Children(), (post) =>
            {
                var fileToken = post["file"];
                var url = fileToken["url"].Value<string>();
                var filetype = url[(url.LastIndexOf('.') + 1)..];
                var artist = post["tags"]["artist"];
                searchResult.AddPost(new BooruPost
                {
                    FileType = filetype,
                    ID = (string)fileToken["id"],
                    MD5 = (string)post["md5"],
                    ImageUrl = url,
                    PreviewUrl = (string)post["preview"]["url"],
                    PostUrl = $"https://e621.net/posts{(string)post["id"]}",
                    Artist = artist == null ? "Автор не найден" : (string)artist.FirstOrDefault(),
                });
            });
#pragma warning restore CS8604, CS8602, CS8601, CS8600 // Possible null reference argument.

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
