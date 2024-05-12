using DSharpPlus.Entities;
using System.Text.RegularExpressions;

namespace MCOP.Common.Converters;

public sealed class ActivityTypeConverter : BaseArgumentConverter<DiscordActivityType>
{
    private static readonly Regex _listeningRegex;
    private static readonly Regex _playingRegex;
    private static readonly Regex _streamingRegex;
    private static readonly Regex _watchingRegex;
    private static readonly Regex _competingRegex;


    static ActivityTypeConverter()
    {
        _listeningRegex = new Regex(@"^l+(i+s+t+e+n+([sz]+|i+n+g+)?)?(to)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _playingRegex = new Regex(@"^p+(l+a+y+([sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _streamingRegex = new Regex(@"^s+(t+r+e+a+m+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _watchingRegex = new Regex(@"^w+(a+t+c+h+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _competingRegex = new Regex(@"^c+o+m+p+e+t+i+n+g(\s+i+n+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }


    public override bool TryConvert(string value, out DiscordActivityType result)
    {
        result = DiscordActivityType.Playing;
        bool parses = true;

        if (_playingRegex.IsMatch(value))
            result = DiscordActivityType.Playing;
        else if (_watchingRegex.IsMatch(value))
            result = DiscordActivityType.Watching;
        else if (_listeningRegex.IsMatch(value))
            result = DiscordActivityType.ListeningTo;
        else if (_streamingRegex.IsMatch(value))
            result = DiscordActivityType.Streaming;
        else if (_competingRegex.IsMatch(value))
            result = DiscordActivityType.Competing;
        else
            parses = false;

        return parses;
    }
}
