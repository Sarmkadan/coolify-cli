#nullable enable
namespace CoolifyCli.Extensions;

/// <summary>
/// Extension methods for collections (IEnumerable, List, Dictionary, etc.).
/// Provides utilities for filtering, transforming, and working with collection data.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Checks if a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    /// <summary>
    /// Batches a sequence into groups of specified size.
    /// Useful for pagination or chunking large datasets.
    /// </summary>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be greater than zero", nameof(batchSize));

        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Distinct items by a selector function.
    /// Removes duplicates based on a projected value.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();

        foreach (var element in source)
        {
            var key = keySelector(element);
            if (seenKeys.Add(key))
                yield return element;
        }
    }

    /// <summary>
    /// Splits a sequence based on a predicate.
    /// Returns tuples of elements that match and don't match the condition.
    /// </summary>
    public static (List<T> Matches, List<T> NonMatches) Split<T>(
        this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var matches = new List<T>();
        var nonMatches = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                matches.Add(item);
            else
                nonMatches.Add(item);
        }

        return (matches, nonMatches);
    }

    /// <summary>
    /// Flattens nested collections into a single sequence.
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Partitions a sequence into multiple groups of specified size.
    /// </summary>
    public static List<List<T>> Partition<T>(this IEnumerable<T> source, int partitionSize)
    {
        return source.Batch(partitionSize).ToList();
    }

    /// <summary>
    /// Takes elements while a condition is true, then returns all remaining elements.
    /// Useful for skipping initial matching elements.
    /// </summary>
    public static IEnumerable<T> SkipWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var skip = true;

        foreach (var item in source)
        {
            if (skip && predicate(item))
                continue;

            skip = false;
            yield return item;
        }
    }

    /// <summary>
    /// Returns elements in reverse order using the specified key selector for sorting.
    /// </summary>
    public static IEnumerable<T> OrderByDescending<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        return System.Linq.Enumerable.OrderByDescending(source, keySelector);
    }

    /// <summary>
    /// Gets the max item by a specified key selector.
    /// </summary>
    public static T? MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        var list = source.ToList();
        if (list.Count == 0) return default;
        return list.Aggregate((a, b) =>
            keySelector(a).CompareTo(keySelector(b)) >= 0 ? a : b);
    }

    /// <summary>
    /// Gets the min item by a specified key selector.
    /// </summary>
    public static T? MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        var list = source.ToList();
        if (list.Count == 0) return default;
        return list.Aggregate((a, b) =>
            keySelector(a).CompareTo(keySelector(b)) <= 0 ? a : b);
    }

    /// <summary>
    /// Groups consecutive elements by a predicate.
    /// </summary>
    public static IEnumerable<List<T>> GroupConsecutive<T>(this IEnumerable<T> source, Func<T, T, bool> condition)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        var group = new List<T> { enumerator.Current };

        while (enumerator.MoveNext())
        {
            if (condition(group.Last(), enumerator.Current))
                group.Add(enumerator.Current);
            else
            {
                yield return group;
                group = new List<T> { enumerator.Current };
            }
        }

        if (group.Count > 0)
            yield return group;
    }

    /// <summary>
    /// Removes null values from a collection.
    /// </summary>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        return source.Where(x => x is not null).Select(x => x!);
    }

    /// <summary>
    /// Safely gets an item at index, returning default value if out of range.
    /// </summary>
    public static T? GetAtIndexOrDefault<T>(this IEnumerable<T> source, int index, T? defaultValue = default)
    {
        var item = source.ElementAtOrDefault(index);
        return item is not null ? item : defaultValue;
    }

    /// <summary>
    /// Shuffles a collection randomly.
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var random = new Random();
        return source.OrderBy(_ => random.Next()).ToList();
    }

    /// <summary>
    /// Converts a dictionary to a query string format (key=valuekey=value).
    /// </summary>
    public static string ToQueryString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        if (dictionary.Count == 0)
            return string.Empty;

        var pairs = dictionary.Select(x => $"{Uri.EscapeDataString(x.Key.ToString()!)}={Uri.EscapeDataString(x.Value?.ToString() ?? "")}");
        return string.Join("&", pairs);
    }

    /// <summary>
    /// Converts a dictionary to a comma-separated string of key=value pairs.
    /// </summary>
    public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        if (dictionary.Count == 0)
            return string.Empty;

        var pairs = dictionary.Select(x => $"{x.Key}={x.Value}");
        return string.Join(", ", pairs);
    }

    /// <summary>
    /// Merges two dictionaries, with later values overwriting earlier ones.
    /// </summary>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second) where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(first);

        foreach (var kvp in second)
            result[kvp.Key] = kvp.Value;

        return result;
    }

    /// <summary>
    /// Helper for getting first matching item by two-way comparison.
    /// </summary>
    private static T? FirstOrDefault<T>(this IEnumerable<T> source,
        Func<T, T, int> comparer, T? defaultValue) where T : notnull
    {
        var items = source.ToList();
        if (items.Count == 0)
            return defaultValue;

        var max = items[0];
        foreach (var item in items.Skip(1))
        {
            if (comparer(item, max) > 0)
                max = item;
        }

        return max;
    }
}
