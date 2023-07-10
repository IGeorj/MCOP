﻿using MCOP.Common;
using MCOP.Core.Common;

namespace MCOP.Extensions;

internal static class EnumerableExtensions
{
    public static string JoinWith<T>(this IEnumerable<T> source, string separator = "\n")
        => string.Join(separator, source.Select(e => e?.ToString() ?? ""));

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        => source.Shuffle(new SafeRandom());

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, SafeRandom rng)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (rng is null)
            throw new ArgumentNullException(nameof(rng));

        return source.ShuffleIterator(rng);
    }


    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, SafeRandom rng)
    {
        var buffer = source.ToList();
        for (int i = 0; i < buffer.Count; i++) {
            int j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}
