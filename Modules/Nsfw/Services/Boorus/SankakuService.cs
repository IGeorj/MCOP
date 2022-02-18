using DSharpPlus.Entities;
using MCOP.Modules.Nsfw.Common;
using MCOP.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.Modules.Nsfw.Services
{
    public sealed class SankakuService : IBotService
    {
        private static readonly string _baseTags = "-male -penis -loli -censored -video female rating:e";
        private static readonly string[] _authors = new[] { "cyicheng", "fi-san", "gray_bear", "itsukarine", "ken_ill",
                "niliu_chahui", "ru_zhai", "z.taiga", "zhixue", "hong_bai", "sunday_se7en", "boppin", "etsunami_kumita", "shaggy_susu",
                "xiaodi"};
        private readonly Sankaku _sankaku;

#pragma warning disable CS8604 // Possible null reference argument.
        public SankakuService()
        {
            _sankaku = Sankaku.Create("georj", Program.Bot?.Config.CurrentConfiguration.SankakuPassword).GetAwaiter().GetResult();
        }

#pragma warning restore CS8604 // Possible null reference argument.


        private async Task<DiscordMessage> SendPostAsync(DiscordChannel channel, BooruPost post, string path)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = post.Artist,
                ImageUrl = $"attachment://{post.MD5}-temp.jpg",
                Url = post.PostUrl,
            };

            DiscordMessage message;
            using (FileStream fstream2 = File.OpenRead(path))
            {
                message = await new DiscordMessageBuilder()
                    .WithFiles(new Dictionary<string, Stream>() { { $"{post.MD5}-temp.jpg", fstream2 } })
                    .WithEmbed(embed)
                    .SendAsync(channel)
                    .ConfigureAwait(false);

            }
            File.Delete(path);

            return message;
        }


        public string GetBaseTags()
        {
            return _baseTags;
        }

        public async Task<SearchResult> GetRandomSearchResultAsync(int limit = 40, string tags = "", string next = "")
        {
            if (!string.IsNullOrEmpty(tags))
            {
                tags = $"{_baseTags} {tags}";
            }
            else
            {
                tags = _baseTags;
            }

            SearchResult searchResult = new();

            try
            {
                if (string.IsNullOrEmpty(next))
                {
                    searchResult = await _sankaku.GetRandomAsync(tags, limit);
                }
                else
                {
                    searchResult = await _sankaku.GetRandomAsync(tags, next, limit);
                }

                searchResult.Sort();
                return searchResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DiscordMessage>> SendSearchResultAsync(DiscordChannel channel, SearchResult result)
        {
            var posts = result.ToSortedList();
            List<DiscordMessage> messages = new();

            foreach (var post in posts)
            {
                try
                {
                    messages.Add(await SendBooruPostAsync(channel, post));
                }
                catch (Exception)
                {
                    Log.Warning($"Failed to send file {post.MD5}.jpg");
                }
            }

            return messages;
        }

        public async Task<DiscordMessage> SendBooruPostAsync(DiscordChannel channel, BooruPost post)
        {
            var authToken = Sankaku.GetAcessToken();

            Directory.CreateDirectory($"Images/Nsfw/{post.Artist}/");
            string path = $"Images/Nsfw/{post.Artist}/{post.MD5}.jpg";
            string pathTemp = $"Images/Nsfw/{post.MD5}-temp.jpg";

            try
            {
                var md5 = post.MD5;

                await post.DownloadAsJpgAsync(path, authToken);

                await ImageProcessorService.SaveAsJpgAsync(path, pathTemp, 95);

                return await SendPostAsync(channel, post, pathTemp);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(List<DiscordMessage>, string?)> SendRandomImagesAsync(
            DiscordChannel channel, int limit = 40, string tags = "", string next = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(tags))
                {
                    tags = $"{_baseTags} {tags}";
                }
                else
                {
                    tags = _baseTags;
                }

                SearchResult searchResult = new();

                if (string.IsNullOrEmpty(next))
                {
                    searchResult = await _sankaku.GetRandomAsync(tags, limit);
                }
                else
                {
                    searchResult = await _sankaku.GetRandomAsync(tags, next, limit);
                }

                List<DiscordMessage> messages = await SendSearchResultAsync(channel, searchResult);

                return (messages, searchResult.GetNext());
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(e.Message);
                return (new List<DiscordMessage>(), null);
            }
        }

        public async Task<(List<DiscordMessage>, string?)> SendDailyTopAsync(
            DiscordChannel channel, int limit = 40, string tags = "", string next = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(tags))
                {
                    tags = $"{_baseTags} {tags}";
                }
                else
                {
                    tags = _baseTags;
                }

                SearchResult searchResult = new();

                if (string.IsNullOrEmpty(next))
                {
                    searchResult = await _sankaku.GetDailyTopAsync(tags, limit);
                }
                else
                {
                    searchResult = await _sankaku.GetDailyTopAsync(tags, next, limit);
                }

                List<DiscordMessage> messages = await SendSearchResultAsync(channel, searchResult);

                return (messages, searchResult.GetNext());
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(e.Message);
                return (new List<DiscordMessage>(), null);
            }
        }
    }
}
