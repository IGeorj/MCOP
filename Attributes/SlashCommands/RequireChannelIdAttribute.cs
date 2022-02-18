using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Attributes.SlashCommands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SlashRequireChannelIdAttribute : SlashCheckBaseAttribute
    {
        public ulong[] Ids;

        public SlashRequireChannelIdAttribute(params ulong[] ids)
        {
            Ids = ids;
        }

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return Task.FromResult(Ids.Contains(ctx.Channel.Id));
        }
    }
}
