using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace MCOP.Modules.Owner.Common;

public sealed class EvaluationEnvironment
{
    public CommandContext Context { get; }
    public IReadOnlyDictionary<ulong, DiscordMessage> FollowupMessages => Context.FollowupMessages;
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
