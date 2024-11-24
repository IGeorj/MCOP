using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace MCOP.Common.Converters;

public abstract class BaseArgumentConverter<T> : ITextArgumentConverter<T>
{
    public bool RequiresText => throw new NotImplementedException();

    public string ReadableName => throw new NotImplementedException();

    ConverterInputType ITextArgumentConverter.RequiresText => ConverterInputType.Always;

    public abstract bool TryConvert(string value, out T? result);
    public Task<Optional<T>> ConvertAsync(ConverterContext context) => ConvertAsync(context.Argument?.ToString() ?? "");
    public Task<Optional<T>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => ConvertAsync(context.Argument?.RawValue ?? "");
    public Task<Optional<T>> ConvertAsync(string value)
        => this.TryConvert(value, out T? result) && result is { } ? Task.FromResult(new Optional<T>(result)) : Task.FromResult(new Optional<T>());
}
