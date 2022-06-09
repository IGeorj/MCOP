using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.VoiceNext;
using MCOP.Modules.Basic.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MCOP.Modules.Basic
{
    public sealed class MusicModule : ApplicationCommandModule
    {
        [SlashRequirePermissions(Permissions.Administrator)]
        [SlashCommand("play", "Play YouTube url")]
        public async Task Play(InteractionContext ctx,
            [Option("url", "name/url")] string text)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            try
            {
                var musicService = ctx.Services.GetRequiredService<MusicService>();

                var vnext = ctx.Client.GetVoiceNext();
                if (vnext is null)
                {
                    throw new("VNext is not enabled or configured.");
                }

                var vnc = vnext.GetConnection(ctx.Guild);

                if(vnc is not null)
                {
                    if (vnc.IsPlaying)
                    {
                        var youtube = new YoutubeClient();
                        var video = await youtube.Videos.GetAsync(text);
                        musicService.AddSong(ctx.Guild.Id, new Common.Song(text));
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added: {video.Title}"));
                        return;
                    }
                }

                if (vnc is null)
                {
                    var vstat = ctx.Member?.VoiceState;
                    if (vstat?.Channel is null)
                    {
                        throw new("You are not in a voice channel.");
                    }

                    vnc = await vnext.ConnectAsync(vstat?.Channel);
                }

                try
                {
                    var youtube = new YoutubeClient();
                    var video = await youtube.Videos.GetAsync(text);
                    musicService.AddSong(ctx.Guild.Id, new Common.Song(text));
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added: {video.Title}"));

                    var transmit = vnc.GetTransmitSink();
                    await vnc.SendSpeakingAsync(true);

                    while(musicService.GetPlaylistCount(ctx.Guild.Id) > 0)
                    {
                        var songStream = await musicService.GetCurrentStreamAsync(ctx.Guild.Id);
                        var token = musicService.GetCancellationToken(ctx.Guild.Id);

                        if (songStream is null)
                        {
                            continue;
                        }

                        int readed = 0;
                        do
                        {
                            byte[] buffer = new byte[1024];
                            readed = await songStream.ReadAsync(buffer, 0, 1024, token);
                            await transmit.WriteAsync(buffer, 0, readed, token);
                        } while (songStream.CanRead && readed > 0);
                        await songStream.DisposeAsync();
                    }

                    await vnc.WaitForPlaybackFinishAsync();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    await vnc.SendSpeakingAsync(false);
                    vnc.Disconnect();
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        [SlashRequirePermissions(Permissions.Administrator)]
        [SlashCommand("skip", "Skip current song")]
        public async Task Skip(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                var musicService = ctx.Services.GetRequiredService<MusicService>();
                musicService.SkipSong(ctx.Guild.Id);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Skipped"));
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
