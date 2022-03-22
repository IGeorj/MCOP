using System.Reflection;
using MCOP.EventListeners.Attributes;

namespace MCOP.EventListeners;

internal static partial class Listeners
{
    // nsfw-помойка
    static readonly ulong nsfwAnimeChannelId = 586295440358506496;

    // lewd-bot
    static readonly ulong lewdBotChannelId = 857354195866615808;

    public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; } = Enumerable.Empty<ListenerMethod>();

    public static void FindAndRegister(Bot shard)
    {
        ListenerMethods =
            from t in Assembly.GetExecutingAssembly().GetTypes()
            from m in t.GetMethods()
            let a = m.GetCustomAttribute(typeof(AsyncEventListenerAttribute), inherit: true)
            where a is { }
            select new ListenerMethod(m, (AsyncEventListenerAttribute)a);

        foreach (ListenerMethod lm in ListenerMethods)
            lm.Attribute.Register(shard, lm.Method);
    }
}


internal sealed class ListenerMethod
{
    public MethodInfo Method { get; }
    public AsyncEventListenerAttribute Attribute { get; }

    public ListenerMethod(MethodInfo mi, AsyncEventListenerAttribute attr)
    {
        this.Method = mi;
        this.Attribute = attr;
    }
}
