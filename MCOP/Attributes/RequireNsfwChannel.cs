using DSharpPlus.Commands.ContextChecks;

namespace MCOP.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public class RequireNsfwChannelAttribute : ContextCheckAttribute;
}
