using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Modules.Basic.Common;
using MCOP.Services;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MCOP.Modules.Basic.Services
{
    public sealed class MusicService : IBotService
    {
        ConcurrentDictionary<ulong, Playlist> playlists = new();

        public async Task<Stream?> GetCurrentStreamAsync(ulong guildId)
        {
            var stream = await playlists[guildId].GetCurrentStreamAsync();
            playlists[guildId].Skip();
            return stream;
        }

        public CancellationToken GetCancellationToken(ulong guildId)
        {
            return playlists[guildId].CancellationTokenSource.Token;
        }

        public int GetPlaylistCount(ulong guildId)
        {
            return playlists[guildId].Count();
        }
        
        public void AddSong(ulong guildId, Song song)
        {
            playlists.TryAdd(guildId, new Playlist());
            playlists[guildId].Add(song);
        }

        public void SkipSong(ulong guildId)
        {
            playlists[guildId].Cancel();
        }
    }
}