using DSharpPlus.Entities;
using MCOP.Modules.Nsfw.Common;
using MCOP.Services;
using Serilog;

namespace MCOP.Modules.Nsfw.Services
{
    public sealed class E621Service : IBotService
    {
        private readonly string _baseTags = "rating:explicit score:>=80 -male -penis -censored -type:webm -feral -monochrome -vomit -male/male -gay -death -imminent_death -castration -nightmare_fuel -snuff -herm -death_by_penis -pooping -feces -urine -necrophilia -animal_genitalia";
        private readonly E621 _e621;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public E621Service()
        {
            _e621 = new E621(Program.Bot.Config.CurrentConfiguration.E621HashPassword);

        }

#pragma warning restore CS8602 // Dereference of a possibly null reference.

        private async Task<DiscordMessage> SendPostAsync(DiscordChannel channel, BooruPost post)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = post.Artist,
                ImageUrl = post.ImageUrl,
                Url = post.PostUrl,
            };

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

            SearchResult searchResult = new();

            try
            {
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
            var posts = result.ToSortedList();
            var authToken = Sankaku.GetAcessToken();
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
                    Log.Warning($"Sankaku. Failed to send images");
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
            catch (Exception e)
            {
                await channel.SendMessageAsync(e.Message);
                return (new List<DiscordMessage>(), null);
            }
        }

    }
}
