using MCOP.Core.Services.Image;
using Serilog;
using System.Net.Http.Headers;

namespace MCOP.Core.Common.Booru
{
    public sealed record Tag
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Type { get; set; } = default!;
    }

    public sealed record BooruPost
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


        public async Task DownloadAsJpgAsync(HttpClient httpclient, string? authToken = null, int quality = 100)
        {
            try
            {
                var folderName = string.Join("_", Artist.Split(Path.GetInvalidFileNameChars())).Replace(".", string.Empty);
                Directory.CreateDirectory($"Images/Nsfw/{folderName}/");
                string path = $"Images/Nsfw/{folderName}/{MD5}.jpg";

                if (!File.Exists(path))
                {
                    HttpRequestMessage request = CreateDownloadRequest(authToken);

                    HttpResponseMessage response = new HttpResponseMessage();

                    try
                    {
                        response = await httpclient.SendAsync(request);
                    }
                    catch (Exception)
                    {
                        response.StatusCode = System.Net.HttpStatusCode.BadGateway;
                    }

                    // If failed retry 1 times
                    if (!response.IsSuccessStatusCode)
                    {
                        request = CreateDownloadRequest(authToken);
                        response = await httpclient.SendAsync(request);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var exception = new Exception($"Failed to download image. Status: {response.StatusCode}. MD5: {MD5}. {response.Content}");
                        Log.Error(exception, exception.Message);
                        throw exception;
                    }

                    byte[] bytes = await httpclient.GetByteArrayAsync(ImageUrl);

                    await SkiaSharpService.SaveAsJpgAsync(bytes, path, 100);
                }

                await SaveCompressedAsync(path);
            }
            catch (Exception)
            {
                throw;
            }

            return;

            HttpRequestMessage CreateDownloadRequest(string? authToken)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ImageUrl);
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                }

                return request;
            }
        }

        public void DeleteUnwantedFile()
        {
            if (LocalFilePathCompressed is not null)
            {
                File.Delete(LocalFilePathCompressed);
            }

            var containsUnwantedToSaveTags = Tags.Any(p => 
                p.Name == "3d" 
                || p.Name == "censored" 
                || p.Name.Contains("goblin_male", StringComparison.InvariantCultureIgnoreCase)
                || p.Name.Contains("pregnant", StringComparison.InvariantCultureIgnoreCase));

            if (LocalFilePath is not null && containsUnwantedToSaveTags)
            {
                string? directoryPath = Path.GetDirectoryName(LocalFilePath);
                File.Delete(LocalFilePath);

                if (directoryPath is not null && IsDirectoryEmpty(directoryPath))
                {
                    Directory.Delete(directoryPath);
                }

                LocalFilePath = null;
            }
        }

        public void DeleteFile()
        {
            if (LocalFilePathCompressed is not null)
            {
                File.Delete(LocalFilePathCompressed);
            }

            if (LocalFilePathCompressed is not null)
            {
                File.Delete(LocalFilePathCompressed);
            }

            if (LocalFilePath is not null)
            {
                string? directoryPath = Path.GetDirectoryName(LocalFilePath);
                File.Delete(LocalFilePath);

                if (directoryPath is not null && IsDirectoryEmpty(directoryPath))
                {
                    Directory.Delete(directoryPath);
                }

                LocalFilePath = null;
            }
        }

        private async Task SaveCompressedAsync(string pathFrom)
        {
            try
            {
                string pathTemp = $"Images/Nsfw/{MD5}-temp.jpg";
                int sizeKB;

                if (File.Exists(pathTemp))
                {
                    sizeKB = GetFileSizeInKb(pathTemp);
                    if (sizeKB < DiscordLimits.AttachmentSizeLimit)
                    {
                        LocalFilePathCompressed = pathTemp;
                        LocalFilePath = pathFrom;
                        AlreadySendDate = File.GetCreationTime(pathFrom);
                        return;
                    }
                }

                await SkiaSharpService.SaveAsJpgAsync(pathFrom, pathTemp, 95);
                sizeKB = GetFileSizeInKb(pathTemp);
                decimal resizeRatio = 2;

                while (sizeKB >= DiscordLimits.AttachmentSizeLimit)
                {
                    File.Delete(pathTemp);
                    await SkiaSharpService.SaveAsJpgAsync(pathFrom, pathTemp, 100, resizeRatio);
                    sizeKB = GetFileSizeInKb(pathTemp);
                    resizeRatio++;
                }

                LocalFilePath = pathFrom;
                LocalFilePathCompressed = pathTemp;
                AlreadySendDate = File.GetCreationTime(pathFrom);

                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static int GetFileSizeInKb(string pathTemp)
        {
            FileInfo fileInfo = new FileInfo(pathTemp);
            int sizeKB = (int)fileInfo.Length / 1024;
            return sizeKB;
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

    }
}
