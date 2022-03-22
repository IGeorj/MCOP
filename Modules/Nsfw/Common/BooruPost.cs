using MCOP.Services;
using Serilog;
using System.Net.Http.Headers;

namespace MCOP.Modules.Nsfw.Common
{
    public record BooruPost
    {
        public string ID { get; init; } = default!;
        public string MD5 { get; init; } = default!;
        public string Artist { get; init; } = default!;
        public string FileType { get; init; } = default!;
        public string PreviewUrl { get; init; } = default!;
        public string ImageUrl { get; init; } = default!;
        public string PostUrl { get; init; } = default!;
        public string? ParentId { get; init; } = null;

        public async ValueTask<bool> DownloadAsJpgAsync(string path, string? authToken = null, int quality = 100)
        {
            if (!File.Exists(path))
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ImageUrl);
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                }

                HttpResponseMessage responce = await HttpService.SendAsync(request);

                // If failed retry 1 times
                if (!responce.IsSuccessStatusCode)
                {
                    responce = await HttpService.SendAsync(request);
                }

                if (!responce.IsSuccessStatusCode)
                {
                    Log.Error("Failed to download image: {Url}", ImageUrl);
                    return false;
                }

                byte[] bytes = await HttpService.GetByteArrayAsync(ImageUrl);

                return await ImageProcessorService.SaveAsJpgAsync(bytes, path, 100);
            }
            return true;
        }
    }
}
