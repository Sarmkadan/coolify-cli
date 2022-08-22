#nullable enable

using System.Collections;

namespace CoolifyCli.Extensions;

/// <summary>
/// Validation helpers for collections to ensure collection state is valid.
/// Provides methods to validate collection instances and throw exceptions for invalid states.
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates a collection and returns any problems found.
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> Validate(this IEnumerable? collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and returns any problems found.
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> Validate(this ICollection? collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and returns any problems found.
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> Validate(this IReadOnlyCollection<string>? collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a dictionary and returns any problems found.
    /// </summary>
    /// <param name="dictionary">The dictionary to validate</param>
    /// <returns>List of human-readable problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dictionary is null</exception>
    public static IReadOnlyList<string> Validate(this IDictionary? dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a dictionary and returns any problems found.
    /// </summary>
    /// <param name="dictionary">The dictionary to validate</param>
    /// <returns>List of human-readable problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dictionary is null</exception>
    public static IReadOnlyList<string> Validate<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a collection is valid.
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this IEnumerable? collection)
    {
        return collection.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a collection is valid.
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ICollection? collection)
    {
        return collection.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a collection is valid.
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this IReadOnlyCollection<string>? collection)
    {
        return collection.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a dictionary is valid.
    /// </summary>
    /// <param name="dictionary">The dictionary to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this IDictionary? dictionary)
    {
        return dictionary.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a dictionary is valid.
    /// </summary>
    /// <param name="dictionary">The dictionary to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
    {
        return dictionary.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures a collection is valid, throwing an exception if not.
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, listing all problems</exception>
    public static void EnsureValid(this IEnumerable? collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = collection.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a collection is valid, throwing an exception if not.
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, listing all problems</exception>
    public static void EnsureValid(this ICollection? collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = collection.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a collection is valid, throwing an exception if not.
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, listing all problems</exception>
    public static void EnsureValid(this IReadOnlyCollection<string>? collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = collection.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a dictionary is valid, throwing an exception if not.
    /// </summary>
    /// <param name="dictionary">The dictionary to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if dictionary is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, listing all problems</exception>
    public static void EnsureValid(this IDictionary? dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var problems = dictionary.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Dictionary validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a dictionary is valid, throwing an exception if not.
    /// </summary>
    /// <param name="dictionary">The dictionary to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if dictionary is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, listing all problems</exception>
    public static void EnsureValid<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var problems = dictionary.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Dictionary validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}