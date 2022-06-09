using System.Collections;
using System.Collections.Concurrent;

namespace MCOP.Modules.Basic.Common
{
    public class Playlist : IEnumerable
    {
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public ConcurrentDictionary<Guid, Song> Songs;

        public Playlist()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Songs = new ConcurrentDictionary<Guid, Song>();
        }

        public async Task<Stream?> GetCurrentStreamAsync()
        {
            if (Songs.Any())
            {
                return await Songs.First().Value.ConvertToStreamAsync();
            }

            return null;
        }

        public void Add(Song song)
        {
            Songs.TryAdd(new Guid(), song);
        }

        public void Skip()
        {
            if (Songs.Any())
            {
                Songs.Remove(Songs.First().Key, out _);
            }
        }

        public int Count()
        {
            return Songs.Count;
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Songs).GetEnumerator();
        }
    }
}