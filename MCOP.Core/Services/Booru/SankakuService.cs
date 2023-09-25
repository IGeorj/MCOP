using DSharpPlus.Entities;
using MCOP.Core.Common.Booru;
using MCOP.Core.Exceptions;
using MCOP.Core.Services.Scoped;
using MCOP.Core.Services.Shared;
using MCOP.Utils.Interfaces;
using Serilog;

namespace MCOP.Core.Services.Booru
{
    public sealed class SankakuService : ISharedService
    {
        private readonly Sankaku _sankaku;
        private string _baseTags = string.Empty;

        public SankakuService(Sankaku sankaku, ConfigurationService config)
        {
            _sankaku = sankaku;
            _baseTags = config.CurrentConfiguration.SankakuRestrictegTags ?? string.Empty;
        }


        private async Task<DiscordMessage> SendPostAsync(DiscordChannel channel, BooruPost post, string path)
        {
            try
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle(post.Artist)
                    .WithUrl(post.PostUrl)
                    .WithImageUrl($"attachment://{post.MD5}-temp.jpg")
                    .WithTimestamp(post.AlreadySendDate);

                var characterTag = post.Tags.Where(t => t.Type == 4);
                if (characterTag is not null && characterTag.Any())
                {
                    embed.WithFooter("Персонажи: " + characterTag.Select(x => x.Name).Aggregate((i, j) => i + " " + j));
                }

                DiscordMessage message;
                using (FileStream fstream2 = File.OpenRead(path))
                {
                    message = await new DiscordMessageBuilder()
                        .AddFile($"{post.MD5}-temp.jpg", fstream2)
                        .WithEmbed(embed)
                        .SendAsync(channel);
                }

                return message;
            }
            catch (Exception)
            {
                throw;
            }
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

            try
            {
                SearchResult searchResult;
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
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<DiscordMessage> SendBooruPostAsync(DiscordChannel channel, BooruPost post)
        {
            var authToken = _sankaku.GetAcessToken();

            try
            {
                await post.DownloadAsJpgAsync(_sankaku.HttpClient, authToken);

                return await SendPostAsync(channel, post, post.LocalFilePathCompressed ?? "");
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<List<DiscordMessage>> SendDailyTopToChannelsAsync(List<DiscordChannel> channels, int limit = 80, int daysShift = 1)
        {
            try
            {
                Log.Information($"Sankaku {nameof(SendDailyTopToChannelsAsync)} started");

                SearchResult searchResult = await _sankaku.GetDailyTopAsync(_baseTags, limit, daysShift);
                List<DiscordMessage> messages = new List<DiscordMessage>();

                foreach (var post in searchResult.ToSortedBooruPosts())
                {
                    foreach (var channel in channels)
                    {
                        try
                        {
                            messages.Add(await SendBooruPostAsync(channel, post));
                        }
                        catch (Exception ex)
                        {
                            Log.Warning("Failed to send file: {MD5}, {Path}, {Path2}. Error:{ex}", post.MD5, post.LocalFilePath, post.LocalFilePathCompressed, ex.Message);
                        }
                    }
                }

                searchResult.DeleteUnwantedFiles();

                Log.Information($"Sankaku {nameof(SendDailyTopToChannelsAsync)} finished");

                return messages;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
    }
}
