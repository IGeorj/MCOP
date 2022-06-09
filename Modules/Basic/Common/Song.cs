using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MCOP.Modules.Basic.Common
{
    public class Song
    {
        string Name { get; set; }
        string Url { get; set; }
        TimeSpan Duration { get; set; }

        public Song(string url)
        {
            Url = url;
        }

        public async Task<Stream> ConvertToStreamAsync()
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(Url);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            Directory.CreateDirectory("Music");
            string path = $"Music/{video.Id}.{streamInfo.Container}";
            await youtube.Videos.Streams.DownloadAsync(streamInfo, path);

            return ConvertAudioToPcm(path);
        }

        private Stream ConvertAudioToPcm(string filePath)
        {
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            return ffmpeg.StandardOutput.BaseStream;
        }
    }
}
