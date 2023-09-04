using System.Collections.Concurrent;

namespace MCOP.Core.Common.Booru
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
            posts = new ConcurrentBag<BooruPost>(posts.OrderBy(x => x.Artist).ThenBy(x => x.ParentId ?? x.ID).ThenBy(x => x.ID));

        }
        public List<BooruPost> ToBooruPosts()
        {
            return posts.ToList();
        }

        public List<BooruPost> ToSortedBooruPosts()
        {
            Sort();
            return posts.ToList();
        }

        public void AddPost(BooruPost post)
        {
            posts.Add(post);
        }

        public void DeleteUnwantedFiles()
        {
            foreach (var post in posts)
            {
                post.DeleteUnwantedFile();
            }
        }
        public void DeleteAllFiles()
        {
            foreach (var post in posts)
            {
                post.DeleteFile();
            }
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
