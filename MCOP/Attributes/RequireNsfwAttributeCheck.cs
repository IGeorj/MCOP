using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace MCOP.Attributes
{
    internal sealed class RequireNsfwChannelAttributeCheck : IContextCheck<RequireNsfwAttribute>
    {
        public ValueTask<string?> ExecuteCheckAsync(RequireNsfwAttribute attribute, CommandContext context) => ValueTask.FromResult
            (
                context.Channel.IsNSFW || (context.Guild is not null && context.Guild.IsNSFW)
                    ? null
                    : "This command must be executed in a NSFW channel."
            );
    }
}
