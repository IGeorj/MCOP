using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace MCOP.Modules.Owner.Common;

public sealed class EvaluationEnvironment
{
    public CommandContext Context { get; }
    public DiscordMessage Message => Context.Message;
    public DiscordChannel Channel => Context.Channel;
    public DiscordGuild Guild => Context.Guild;
    public DiscordUser User => Context.User;
    public DiscordMember? Member => Context.Member;
    public DiscordClient Client => Context.Client;


    public EvaluationEnvironment(CommandContext ctx)
    {
        this.Context = ctx;
    }
}
