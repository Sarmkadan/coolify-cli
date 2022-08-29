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
    /// <param name="collection">The collection to check.</param>
    /// <returns><see langword="true"/> if the collection is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    /// <summary>
    /// Batches a sequence into groups of specified size.
    /// Useful for pagination or chunking large datasets.
    /// </summary>
    /// <param name="source">The source sequence to batch.</param>
    /// <param name="batchSize">The size of each batch. Must be greater than zero.</param>
    /// <returns>A sequence of batches, each containing up to <paramref name="batchSize"/> elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is less than or equal to zero.</exception>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

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
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key to compare for uniqueness.</param>
    /// <returns>A sequence that contains no duplicate values based on <paramref name="keySelector"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

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
    /// <param name="source">The source sequence to split.</param>
    /// <param name="predicate">The function to test each element.</param>
    /// <returns>A tuple containing two lists: the first with elements that match the predicate, and the second with elements that don't match.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static (List<T> Matches, List<T> NonMatches) Split<T>(
        this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

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
    /// <param name="source">The source sequence of sequences to flatten.</param>
    /// <returns>A single sequence containing all elements from all inner sequences.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Partitions a sequence into multiple groups of specified size.
    /// </summary>
    /// <param name="source">The source sequence to partition.</param>
    /// <param name="partitionSize">The size of each partition. Must be greater than zero.</param>
    /// <returns>A list of lists, each containing up to <paramref name="partitionSize"/> elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="partitionSize"/> is less than or equal to zero.</exception>
    public static List<List<T>> Partition<T>(this IEnumerable<T> source, int partitionSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(partitionSize, 0);

        return source.Batch(partitionSize).ToList();
    }

    /// <summary>
    /// Takes elements while a condition is true, then returns all remaining elements.
    /// Useful for skipping initial matching elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">The function to test each element.</param>
    /// <returns>A sequence that contains the remaining elements after the initial elements that satisfy the predicate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> SkipWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

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
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract a key for sorting.</param>
    /// <returns>A sequence of elements sorted in descending order by the specified key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> OrderByDescending<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return System.Linq.Enumerable.OrderByDescending(source, keySelector);
    }

    /// <summary>
    /// Gets the max item by a specified key selector.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract a key for comparison.</param>
    /// <returns>The first element with the maximum key value, or <see langword="default"/> if the sequence is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static T? MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return System.Linq.Enumerable.MaxBy(source, keySelector);
    }

    /// <summary>
    /// Gets the min item by a specified key selector.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract a key for comparison.</param>
    /// <returns>The first element with the minimum key value, or <see langword="default"/> if the sequence is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static T? MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return System.Linq.Enumerable.MinBy(source, keySelector);
    }

    /// <summary>
    /// Groups consecutive elements by a predicate.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="condition">A function that determines if two consecutive elements should be grouped together.</param>
    /// <returns>A sequence of groups, where each group contains consecutive elements that satisfy the condition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="condition"/> is <see langword="null"/>.</exception>
    public static IEnumerable<List<T>> GroupConsecutive<T>(this IEnumerable<T> source, Func<T, T, bool> condition)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(condition);

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
    /// <param name="source">The source sequence of nullable elements.</param>
    /// <returns>A sequence containing only non-null elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Where(x => x is not null).Select(x => x!);
    }

    /// <summary>
    /// Safely gets an item at index, returning default value if out of range.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <param name="defaultValue">The default value to return if index is out of range. Defaults to <see langword="default"/> of <typeparamref name="T"/>.</param>
    /// <returns>The element at the specified index, or <paramref name="defaultValue"/> if index is out of range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static T? GetAtIndexOrDefault<T>(this IEnumerable<T> source, int index, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var item = source.ElementAtOrDefault(index);
        return item is not null ? item : defaultValue;
    }

    /// <summary>
    /// Shuffles a collection randomly.
    /// </summary>
    /// <param name="source">The source sequence to shuffle.</param>
    /// <returns>A new list containing the elements in random order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var random = new Random();
        return source.OrderBy(_ => random.Next()).ToList();
    }

    /// <summary>
    /// Converts a dictionary to a query string format (key=valuekey=value).
    /// </summary>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>A query string representation of the dictionary, or an empty string if the dictionary is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
    public static string ToQueryString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.Count == 0)
            return string.Empty;

        var pairs = dictionary.Select(x => $"{Uri.EscapeDataString(x.Key.ToString()!)}={Uri.EscapeDataString(x.Value?.ToString() ?? "")}");
        return string.Join("&", pairs);
    }

    /// <summary>
    /// Converts a dictionary to a comma-separated string of key=value pairs.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>A comma-separated string of key=value pairs, or an empty string if the dictionary is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
    public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.Count == 0)
            return string.Empty;

        var pairs = dictionary.Select(x => $"{x.Key}={x.Value}");
        return string.Join(", ", pairs);
    }

    /// <summary>
    /// Merges two dictionaries, with later values overwriting earlier ones.
    /// </summary>
    /// <param name="first">The first dictionary.</param>
    /// <param name="second">The second dictionary whose values take precedence.</param>
    /// <returns>A new dictionary containing all key-value pairs from both dictionaries, with conflicts resolved in favor of <paramref name="second"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <see langword="null"/>.</exception>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var result = new Dictionary<TKey, TValue>(first);

        foreach (var kvp in second)
            result[kvp.Key] = kvp.Value;

        return result;
    }

    /// <summary>
    /// Helper for getting first matching item by two-way comparison.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="comparer">A comparison function that returns a positive value if the first argument is greater than the second.</param>
    /// <param name="defaultValue">The default value to return if the sequence is empty.</param>
    /// <returns>The maximum element according to the comparer, or <paramref name="defaultValue"/> if the sequence is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="comparer"/> is <see langword="null"/>.</exception>
    private static T? FirstOrDefault<T>(
        this IEnumerable<T> source,
        Func<T, T, int> comparer,
        T? defaultValue) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(comparer);

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