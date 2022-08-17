using DSharpPlus.Entities;
using MCOP.Modules.Nsfw.Common;
using MCOP.Modules.Nsfw.Services.Boorus;
using MCOP.Services;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.Nsfw.Services
{
    public sealed class GelbooruService : IBotService
    {
        private readonly Gelbooru _gelbooru;
        private string _baseTags = string.Empty;

        public GelbooruService(BotConfigService config)
        {
            _gelbooru = new Gelbooru();
            _baseTags = config.CurrentConfiguration.GelbooruRestrictegTags ?? string.Empty;
        }

        private async Task<DiscordMessage> SendPostAsync(DiscordChannel channel, BooruPost post)
        {
            var embed = new DiscordEmbedBuilder()
            .WithTitle(post.Artist)
            .WithImageUrl(post.ImageUrl)
            .WithUrl(post.PostUrl);

            return await channel.SendMessageAsync(embed.Build());
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
                    DiscordMessage msg = await SendPostAsync(channel, post);

                    messages.Add(msg);
                }
                catch (Exception)
                {
                    Log.Warning($"Gelbooru. Failed to send images");
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
                await channel.SendMessageAsync(e.Message);
                return (new List<DiscordMessage>(), null);
            }
        }

    }
}
