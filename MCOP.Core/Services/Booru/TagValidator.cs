using MCOP.Core.Common.Booru;
using MCOP.Core.Exceptions;

namespace MCOP.Core.Services.Booru
{
    public static class TagValidator
    {
        private static readonly HashSet<string> RestrictedTags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "loli",
            "contentious_content",
            "blood",
            "futanari"
        };

        public static void ValidateTags(BooruPost post)
        {
            if (post?.Tags == null) return;

            var foundRestrictedTags = post.Tags
                .Where(tag => tag?.Name != null)
                .Select(tag => tag.Name)
                .Where(tagName => RestrictedTags.Contains(tagName))
                .ToList();

            if (foundRestrictedTags.Count != 0)
            {
                var restrictedTagsList = string.Join(", ", foundRestrictedTags);
                throw new McopException($"Restricted tag(s) '{restrictedTagsList}' found in post with MD5: {post.MD5}");
            }
        }
    }
}
