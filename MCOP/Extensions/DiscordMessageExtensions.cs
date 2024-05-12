namespace MCOP.Extensions;

public static class DiscordMessageExtensions
{
    //public static async ValueTask<List<byte[]>> GetImageHashesAsync(this DiscordMessage msg)
    //{
    //    List<byte[]> hashes = new();

    //    var links = msg.Content.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("http://") || s.StartsWith("www.") || s.StartsWith("https://"));
    //    var link = links.FirstOrDefault();

    //    if (link is not null && (link.Contains(".png") || link.Contains(".jpg") || link.Contains(".jpeg") || link.Contains(".webp")))
    //    {
    //        byte[] imgBytes = await HttpService.GetByteArrayAsync(link);
    //        using var bitmap = SkiaSharp.SKBitmap.Decode(imgBytes);
    //        hashes.Add(SkiaSharpService.GetBitmapHash(bitmap));
    //    }

    //    foreach (var item in msg.Attachments)
    //    {
    //        string type = item.MediaType;

    //        if (type.Contains("png") || type.Contains("jpeg") || type.Contains("webp"))
    //        {
    //            byte[] imgBytes = await HttpService.GetByteArrayAsync(item.Url);
    //            using var bitmap = SkiaSharp.SKBitmap.Decode(imgBytes);
    //            hashes.Add(SkiaSharpService.GetBitmapHash(bitmap));

    //        }
    //    }

    //    return hashes;
    //}
}
