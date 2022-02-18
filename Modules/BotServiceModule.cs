using MCOP.Services;
using DSharpPlus.CommandsNext;

namespace MCOP.Modules
{
    public abstract class BotServiceModule<TService> : BotModule where TService : IBotService
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TService Service { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public override Task BeforeExecutionAsync(CommandContext ctx)
            => base.BeforeExecutionAsync(ctx);
    }
}
