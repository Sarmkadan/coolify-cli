#nullable enable

using CoolifyCli.Extensions;
using FluentAssertions;
using Xunit;

using CoolifyCollectionExtensions = CoolifyCli.Extensions.CollectionExtensions;

namespace CoolifyCli.Tests;

/// <summary>
/// Provides unit tests for the <see cref="CoolifyCli.Extensions.CollectionExtensions"/> class.
/// Tests various extension methods for collections including batching, filtering, and transformation operations.
/// </summary>
public class CollectionExtensionsTests
{
    // ---- IsNullOrEmpty -------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}?.IsNullOrEmpty()"/> returns true when the collection is null.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_WithNullCollection_ReturnsTrue()
    {
        IEnumerable<int>? collection = null;
        collection.IsNullOrEmpty().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}.IsNullOrEmpty()"/> returns true when the collection is empty.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_WithEmptyCollection_ReturnsTrue()
    {
        new List<string>().IsNullOrEmpty().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}.IsNullOrEmpty()"/> returns false when the collection contains elements.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_WithNonEmptyCollection_ReturnsFalse()
    {
        new[] { 1, 2, 3 }.IsNullOrEmpty().Should().BeFalse();
    }

    // ---- Batch ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}.Batch(int)"/> splits a collection into equal-sized batches when the collection size is an exact multiple of the batch size.
    /// </summary>
    [Fact]
    public void Batch_WithExactMultiple_ProducesEqualSizedBatches()
    {
        var items = Enumerable.Range(1, 6).ToList();

        var batches = items.Batch(2).ToList();

        batches.Should().HaveCount(3);
        batches.Should().AllSatisfy(b => b.Should().HaveCount(2));
    }

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}.Batch(int)"/> splits a collection into batches where the last batch contains any remaining items when the collection size is not an exact multiple of the batch size.
    /// </summary>
    [Fact]
    public void Batch_WithRemainder_LastBatchContainsLeftoverItems()
    {
        var items = Enumerable.Range(1, 5).ToList();

        var batches = items.Batch(2).ToList();

        batches.Should().HaveCount(3);
        batches.Last().Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}.Batch(int)"/> throws an <see cref="ArgumentException"/> when the batch size is zero.
    /// </summary>
    [Fact]
    public void Batch_WithZeroSize_ThrowsArgumentException()
    {
        var items = new[] { 1, 2, 3 };

        var act = () => items.Batch(0).ToList();

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*greater than '0'*");
    }

    /// <summary>
    /// Tests that <see cref="IEnumerable{T}.Batch(int)"/> returns an empty sequence when the source collection is empty.
    /// </summary>
    [Fact]
    public void Batch_WithEmptySource_ProducesNoBatches()
    {
        var batches = Array.Empty<int>().Batch(3).ToList();

        batches.Should().BeEmpty();
    }

    // ---- DistinctBy ----------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.DistinctBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey})"/> removes duplicates based on a key selector function.
    /// </summary>
    [Fact]
    public void DistinctBy_RemovesDuplicatesBasedOnKeySelector()
    {
        var items = new[] { "apple", "apricot", "banana", "blueberry" };

        // Call via the extension explicitly to avoid ambiguity with Linq's built-in DistinctBy
        var result = CoolifyCollectionExtensions.DistinctBy(items, s => s[0]).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain("apple").And.Contain("banana");
    }

    // ---- Split ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Split{T}(IEnumerable{T},Func{T,bool})"/> partitions items into two collections based on a predicate.
    /// </summary>
    [Fact]
    public void Split_PartitionsItemsByPredicate()
    {
        var numbers = Enumerable.Range(1, 6).ToList();

        var (evens, odds) = numbers.Split(n => n % 2 == 0);

        evens.Should().BeEquivalentTo(new[] { 2, 4, 6 });
        odds.Should().BeEquivalentTo(new[] { 1, 3, 5 });
    }

    // ---- Flatten -------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Flatten{T}(IEnumerable{IEnumerable{T}})"/> combines nested collections into a single flattened sequence.
    /// </summary>
    [Fact]
    public void Flatten_CombinesNestedCollectionsIntoSingleSequence()
    {
        var nested = new List<List<int>>
        {
            new() { 1, 2 },
            new() { 3 },
            new() { 4, 5, 6 }
        };

        var result = nested.Flatten().ToList();

        result.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6 });
    }

    // ---- MaxBy / MinBy -------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.MaxBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey})"/> returns the item with the largest key value according to the key selector function.
    /// </summary>
    [Fact]
    public void MaxBy_ReturnsItemWithLargestKeyValue()
    {
        var words = new[] { "cat", "elephant", "dog" };

        // Call via the extension explicitly to avoid ambiguity with Linq's built-in MaxBy
        var longest = CoolifyCollectionExtensions.MaxBy(words, w => w.Length);

        longest.Should().Be("elephant");
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.MinBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey})"/> returns the item with the smallest key value according to the key selector function.
    /// </summary>
    [Fact]
    public void MinBy_ReturnsItemWithSmallestKeyValue()
    {
        var words = new[] { "cat", "elephant", "dog" };

        // Call via the extension explicitly to avoid ambiguity with Linq's built-in MinBy
        var shortest = CoolifyCollectionExtensions.MinBy(words, w => w.Length);

        shortest.Should().Be("cat");
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.MaxBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey})"/> returns null when the source collection is empty.
    /// </summary>
    [Fact]
    public void MaxBy_WithEmptyCollection_ReturnsDefault()
    {
        var result = CoolifyCollectionExtensions.MaxBy(Array.Empty<string>(), s => s.Length);

        result.Should().BeNull();
    }

    // ---- WhereNotNull --------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.WhereNotNull{T}(IEnumerable{T?})"/> filters out null references from the collection.
    /// </summary>
    [Fact]
    public void WhereNotNull_FiltersOutNullReferences()
    {
        var items = new string?[] { "a", null, "b", null, "c" };

        var result = items.WhereNotNull().ToList();

        result.Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    // ---- Merge ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Merge{TKey,TValue}(IDictionary{TKey,TValue},IDictionary{TKey,TValue})"/> merges two dictionaries, with values from the second dictionary overwriting values from the first dictionary for keys that exist in both.
    /// </summary>
    [Fact]
    public void Merge_SecondDictionaryValuesOverwriteFirst()
    {
        var first = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var second = new Dictionary<string, int> { { "b", 99 }, { "c", 3 } };

        var merged = first.Merge(second);

        merged["a"].Should().Be(1);
        merged["b"].Should().Be(99);
        merged["c"].Should().Be(3);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Merge{TKey,TValue}(IDictionary{TKey,TValue},IDictionary{TKey,TValue})"/> does not modify the original dictionaries when merging.
    /// </summary>
    [Fact]
    public void Merge_DoesNotModifyOriginalDictionaries()
    {
        var first = new Dictionary<string, int> { { "a", 1 } };
        var second = new Dictionary<string, int> { { "a", 2 } };

        first.Merge(second);

        first["a"].Should().Be(1);
    }

    // ---- ToQueryString -------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ToQueryString(IDictionary{string,string})"/> with multiple entries produces an ampersand-separated query string with key-value pairs.
    /// </summary>
    [Fact]
    public void ToQueryString_WithMultipleEntries_ProducesAmpersandSeparatedPairs()
    {
        var dict = new Dictionary<string, string>
        {
            { "env", "prod" },
            { "region", "us-east-1" }
        };

        var qs = dict.ToQueryString();

        qs.Should().Contain("env=prod");
        qs.Should().Contain("region=us-east-1");
        qs.Should().Contain("&");
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ToQueryString(IDictionary{string,string})"/> with an empty dictionary returns an empty string.
    /// </summary>
    [Fact]
    public void ToQueryString_WithEmptyDictionary_ReturnsEmptyString()
    {
        var dict = new Dictionary<string, string>();

        dict.ToQueryString().Should().BeEmpty();
    }

    // ---- GroupConsecutive ----------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.GroupConsecutive{T}(IEnumerable{T},Func{T,T,bool})"/> groups adjacent items that satisfy a consecutive condition into separate collections.
    /// </summary>
    [Fact]
    public void GroupConsecutive_GroupsAdjacentItemsMeetingCondition()
    {
        var numbers = new[] { 1, 2, 3, 10, 11, 20 };

        var groups = numbers.GroupConsecutive((a, b) => b - a <= 1).ToList();

        groups.Should().HaveCount(3);
        groups[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        groups[1].Should().BeEquivalentTo(new[] { 10, 11 });
        groups[2].Should().BeEquivalentTo(new[] { 20 });
    }
}
