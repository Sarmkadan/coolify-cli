#nullable enable

using CoolifyCli.Extensions;
using FluentAssertions;
using Xunit;

using CoolifyCollectionExtensions = CoolifyCli.Extensions.CollectionExtensions;

namespace CoolifyCli.Tests;

public class CollectionExtensionsTests
{
    // ---- IsNullOrEmpty -------------------------------------------------------

    [Fact]
    public void IsNullOrEmpty_WithNullCollection_ReturnsTrue()
    {
        IEnumerable<int>? collection = null;
        collection.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithEmptyCollection_ReturnsTrue()
    {
        new List<string>().IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithNonEmptyCollection_ReturnsFalse()
    {
        new[] { 1, 2, 3 }.IsNullOrEmpty().Should().BeFalse();
    }

    // ---- Batch ---------------------------------------------------------------

    [Fact]
    public void Batch_WithExactMultiple_ProducesEqualSizedBatches()
    {
        var items = Enumerable.Range(1, 6).ToList();

        var batches = items.Batch(2).ToList();

        batches.Should().HaveCount(3);
        batches.Should().AllSatisfy(b => b.Should().HaveCount(2));
    }

    [Fact]
    public void Batch_WithRemainder_LastBatchContainsLeftoverItems()
    {
        var items = Enumerable.Range(1, 5).ToList();

        var batches = items.Batch(2).ToList();

        batches.Should().HaveCount(3);
        batches.Last().Should().HaveCount(1);
    }

    [Fact]
    public void Batch_WithZeroSize_ThrowsArgumentException()
    {
        var items = new[] { 1, 2, 3 };

        var act = () => items.Batch(0).ToList();

        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public void Batch_WithEmptySource_ProducesNoBatches()
    {
        var batches = Array.Empty<int>().Batch(3).ToList();

        batches.Should().BeEmpty();
    }

    // ---- DistinctBy ----------------------------------------------------------

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

    [Fact]
    public void Split_PartitionsItemsByPredicate()
    {
        var numbers = Enumerable.Range(1, 6).ToList();

        var (evens, odds) = numbers.Split(n => n % 2 == 0);

        evens.Should().BeEquivalentTo(new[] { 2, 4, 6 });
        odds.Should().BeEquivalentTo(new[] { 1, 3, 5 });
    }

    // ---- Flatten -------------------------------------------------------------

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

    [Fact]
    public void MaxBy_ReturnsItemWithLargestKeyValue()
    {
        var words = new[] { "cat", "elephant", "dog" };

        // Call via the extension explicitly to avoid ambiguity with Linq's built-in MaxBy
        var longest = CoolifyCollectionExtensions.MaxBy(words, w => w.Length);

        longest.Should().Be("elephant");
    }

    [Fact]
    public void MinBy_ReturnsItemWithSmallestKeyValue()
    {
        var words = new[] { "cat", "elephant", "dog" };

        // Call via the extension explicitly to avoid ambiguity with Linq's built-in MinBy
        var shortest = CoolifyCollectionExtensions.MinBy(words, w => w.Length);

        shortest.Should().Be("cat");
    }

    [Fact]
    public void MaxBy_WithEmptyCollection_ReturnsDefault()
    {
        var result = CoolifyCollectionExtensions.MaxBy(Array.Empty<string>(), s => s.Length);

        result.Should().BeNull();
    }

    // ---- WhereNotNull --------------------------------------------------------

    [Fact]
    public void WhereNotNull_FiltersOutNullReferences()
    {
        var items = new string?[] { "a", null, "b", null, "c" };

        var result = items.WhereNotNull().ToList();

        result.Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    // ---- Merge ---------------------------------------------------------------

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

    [Fact]
    public void Merge_DoesNotModifyOriginalDictionaries()
    {
        var first = new Dictionary<string, int> { { "a", 1 } };
        var second = new Dictionary<string, int> { { "a", 2 } };

        first.Merge(second);

        first["a"].Should().Be(1);
    }

    // ---- ToQueryString -------------------------------------------------------

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

    [Fact]
    public void ToQueryString_WithEmptyDictionary_ReturnsEmptyString()
    {
        var dict = new Dictionary<string, string>();

        dict.ToQueryString().Should().BeEmpty();
    }

    // ---- GroupConsecutive ----------------------------------------------------

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
