using DSharpPlus.Entities;
using MCOP.Core.Common.Booru;
using MCOP.Core.Exceptions;
using MCOP.Core.Services.Shared;
using MCOP.Utils.Interfaces;
using Serilog;

namespace MCOP.Core.Services.Booru
{
    public sealed class GelbooruService : ISharedService
    {
        private readonly Gelbooru _gelbooru;
        private string _baseTags = string.Empty;

        public GelbooruService(Gelbooru gelbooru, ConfigurationService config)
        {
            _gelbooru = gelbooru;
            _baseTags = config.CurrentConfiguration.GelbooruRestrictegTags ?? string.Empty;
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
                SearchResult searchResult;
                if (string.IsNullOrEmpty(next))
                {
                    searchResult = await _gelbooru.GetRandomAsync(tags, limit);
                }
                else
                {
                    searchResult = await _gelbooru.GetRandomAsync(tags, next, limit);
                }

                return searchResult;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
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
                    Log.Warning($"Gelbooru. Failed to send Discord Message {post.MD5}, {post.LocalFilePath} from SearchResult");
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
                    searchResult = await _gelbooru.GetRandomAsync(tags, limit);
                }
                else
                {
                    searchResult = await _gelbooru.GetRandomAsync(tags, next, limit);
                }

                List<DiscordMessage> messages = await SendSearchResultAsync(channel, searchResult);

                return (messages, searchResult.GetNext());
            }
            catch (Exception e)
            {
                Log.Error(e, "Gelbooru. SendRandomImages error");
                await channel.SendMessageAsync("Gelbooru. SendRandomImages error");
                return (new List<DiscordMessage>(), null);
            }
        }

    }
}
