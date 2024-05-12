using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using Serilog;
using System.Reflection;

namespace MCOP.Extensions;

internal static class CNextExtensions
{
    public static void RegisterConverters(this TextCommandProcessor commandProcessor, Assembly? assembly = null)
    {
        try
        {
            assembly ??= Assembly.GetExecutingAssembly();

            commandProcessor.AddConverters(assembly);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to register converters:");
        }
    }

    public static void RegisterConverters(this SlashCommandProcessor commandProcessor, Assembly? assembly = null)
    {
        try
        {
            assembly ??= Assembly.GetExecutingAssembly();

            commandProcessor.AddConverters(assembly);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to register converters:");
        }
    }
}
