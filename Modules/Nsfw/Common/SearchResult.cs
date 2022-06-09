using System.Collections.Concurrent;

namespace MCOP.Modules.Nsfw.Common
{
    public class SearchResult
    {
        private ConcurrentBag<BooruPost> posts = new();
        private string? _next;
        private string? _prev;

        public int Count()
        {
            return posts.Count;
        }

        public void Sort()
        {
            posts = new ConcurrentBag<BooruPost>(posts.OrderBy(x => x.ParentId ?? x.ID).ThenBy(x => x.ID));

        }
        public List<BooruPost> ToList()
        {
            return posts.ToList();
        }

        public List<BooruPost> ToSortedList()
        {
            Sort();
            return posts.ToList();
        }

        public void AddPost(BooruPost post)
        {
            posts.Add(post);
        }

        public string? GetNext()
        {
            return _next;
        }

        public string? GetPrev()
        {
            return _prev;
        }

        public void SetNext(string? next)
        {
            _next = next;
        }

        public void SetPrev(string? prev)
        {
            _prev = prev;
        }
    }
}
