using DSharpPlus.Entities;
using System.Text.RegularExpressions;

namespace MCOP.Extensions;

public static class DiscordMessageExtensions
{
    public static bool ContainsImageLink(this DiscordMessage message)
    {
        if (string.IsNullOrEmpty(message.Content))
            return false;

        string pattern = @"https?://\S+\.(jpg|jpeg|png|gif|bmp|webp)";

        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        return regex.IsMatch(message.Content);
    }
}
