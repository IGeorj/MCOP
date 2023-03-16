using MCOP.Common;
using MCOP.Services;
using Serilog;
using System.Net.Http.Headers;

namespace MCOP.Modules.Nsfw.Common
{
    public record Tag
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
    }

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
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public string? LocalFilePath { get; set; } = default!;
        public string? LocalFilePathCompressed { get; set; } = default!;
        public DateTime? AlreadySendDate { get; set; } = default!;

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

                var savedFull = await ImageProcessorService.SaveAsJpgAsync(bytes, path, 100);

                bool savedCompressed = await SaveCompressed(path);
                LocalFilePath = path;

                var containsRestrictedTags = Tags.Any(p => p.Name == "3d");
                if (containsRestrictedTags)
                {
                    File.Delete(LocalFilePath);
                    LocalFilePath = null;
                }
                return savedFull || savedCompressed;
            }

            return await SaveCompressed(path); ;
        }

        private async Task<bool> SaveCompressed(string pathFrom)
        {
            string pathTemp = $"Images/Nsfw/{MD5}-temp.jpg";
            var savedCompressed = await ImageProcessorService.SaveAsJpgAsync(pathFrom, pathTemp, 95);
            int sizeKB = GetFileSizeInKb(pathTemp);
            decimal resizeRatio = 2;
            while (sizeKB >= DiscordLimits.AttachmentSizeLimit)
            {
                File.Delete(pathTemp);
                savedCompressed = await ImageProcessorService.SaveAsJpgAsync(pathFrom, pathTemp, 100, resizeRatio);
                sizeKB = GetFileSizeInKb(pathTemp);
                resizeRatio++;
            }
            LocalFilePathCompressed = pathTemp;
            AlreadySendDate = File.GetCreationTime(pathFrom);
            return savedCompressed;
        }

        private static int GetFileSizeInKb(string pathTemp)
        {
            FileInfo fileInfo = new FileInfo(pathTemp);
            int sizeKB = (int)fileInfo.Length / 1024;
            return sizeKB;
        }
    }
}
