#nullable enable

using CoolifyCli.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolifyCli.Tests;

public static class CollectionExtensionsTestsExtensions
{
    /// <summary>
    /// Creates a new list containing all items from the source collection.
    /// Useful for testing scenarios where you need to materialize a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <returns>A new list containing all items from the source collection.</returns>
    public static List<T> ToMaterializedList<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new List<T>(source);
    }

    /// <summary>
    /// Creates a new array containing all items from the source collection.
    /// Useful for testing scenarios where array operations are needed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <returns>A new array containing all items from the source collection.</returns>
    public static T[] ToMaterializedArray<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.ToArray();
    }

    /// <summary>
    /// Returns the count of items in the collection, or 0 if the collection is null.
    /// Useful for defensive programming in test scenarios.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <returns>The count of items in the collection, or 0 if null.</returns>
    public static int SafeCount<T>(this IEnumerable<T>? source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Count();
    }

    /// <summary>
    /// Returns the first element of the collection, or a default value if the collection is null or empty.
    /// Useful for testing edge cases with empty collections.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="defaultValue">The default value to return if no element is found.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <returns>The first element or the default value.</returns>
    public static T FirstOrDefaultWithDefault<T>(this IEnumerable<T> source, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.FirstOrDefault(candidate => true, defaultValue);
    }
}