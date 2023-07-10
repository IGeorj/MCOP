using DSharpPlus.SlashCommands;

namespace MCOP.Attributes.SlashCommands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SlashRequireNsfwAttribute : SlashCheckBaseAttribute
    {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
            => Task.FromResult(ctx.Channel.Guild == null || ctx.Channel.IsNSFW);
    }
}
