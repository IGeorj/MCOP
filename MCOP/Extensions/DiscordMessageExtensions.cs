using DSharpPlus.Entities;
using Serilog;
using System.Text.RegularExpressions;

namespace MCOP.Extensions;

public static class DiscordMessageExtensions
{
    public static bool IsContainsImageLink(this DiscordMessage message)
    {
        if (string.IsNullOrEmpty(message.Content))
            return false;

        string pattern = @"https?://\S+\.(jpg|jpeg|png|gif|bmp|webp)";

        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        return regex.IsMatch(message.Content);
    }

    public static async Task DeleteSilentAsync(this DiscordMessage message)
    {
        try
        {
            await message.DeleteAsync();
        }
        catch (Exception ex)
        {
            Log.Information("Failed to delete duel message: {messageId}, ex: {ex} ", message.Id, ex.GetType().Name);
        }
    }
}
