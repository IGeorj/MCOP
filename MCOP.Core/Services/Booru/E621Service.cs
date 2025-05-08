using DSharpPlus.Entities;
using MCOP.Core.Common.Booru;
using MCOP.Utils;
using MCOP.Utils.Interfaces;
using Serilog;

namespace MCOP.Core.Services.Booru
{
    public sealed class E621Service : ISharedService
    {
        private readonly E621 _e621;
        private string _baseTags = string.Empty;

        public E621Service(E621 e621, ConfigurationService config)
        {
            _e621 = e621;
            _baseTags = config.CurrentConfiguration.E621RestrictegTags ?? string.Empty;
        }


        private async Task<DiscordMessage> SendPostAsync(DiscordChannel channel, BooruPost post)
        {
            try
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle(post.Artist)
                    .WithImageUrl(post.ImageUrl)
                    .WithUrl(post.PostUrl);

                return await channel.SendMessageAsync(embed.Build());
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
                SearchResult searchResult = new();
                if (string.IsNullOrEmpty(next))
                {
                    searchResult = await _e621.GetRandomAsync(tags, limit);
                }
                else
                {
                    searchResult = await _e621.GetRandomAsync(tags, next, limit);
                }

                return searchResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DiscordMessage>> SendSearchResultAsync(DiscordChannel channel, SearchResult result)
        {
            var posts = result.ToSortedBooruPosts();
            List<DiscordMessage> messages = new();

            foreach (var post in posts)
            {
                try
                {
                    DiscordMessage msg = await SendPostAsync(channel, post);

                    messages.Add(msg);
                }
                catch (Exception)
                {
                    Log.Warning($"E621. Failed to send Discord Message {post.MD5}, {post.LocalFilePath} from SearchResult");
                }
            }

            return messages;
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
                    searchResult = await _e621.GetRandomAsync(tags, limit);
                }
                else
                {
                    searchResult = await _e621.GetRandomAsync(tags, next, limit);
                }

                List<DiscordMessage> messages = await SendSearchResultAsync(channel, searchResult);

                return (messages, searchResult.GetNext());
            }
            catch (Exception)
            {
                return (new List<DiscordMessage>(), null);
            }
        }

    }
}
