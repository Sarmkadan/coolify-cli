groups[2].Should().BeEquivalentTo(new[] { 20 });
}

// ---- Partition ------------------------------------------------------------

/// <summary>
/// Tests that <see cref="CollectionExtensions.Partition{T}(IEnumerable{T},int)"/> splits a collection into equal-sized partitions when the collection size is an exact multiple of the partition size.
/// </summary>
[Fact]
public void Partition_WithExactMultiple_ProducesEqualSizedPartitions()
{
    var items = Enumerable.Range(1, 6).ToList();

    var partitions = items.Partition(2);

    partitions.Should().HaveCount(3);
    partitions.Should().AllSatisfy(p => p.Should().HaveCount(2));
    partitions[0].Should().BeEquivalentTo(new[] { 1, 2 });
    partitions[1].Should().BeEquivalentTo(new[] { 3, 4 });
    partitions[2].Should().BeEquivalentTo(new[] { 5, 6 });
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Partition{T}(IEnumerable{T},int)"/> splits a collection into partitions where the last partition contains any remaining items when the collection size is not an exact multiple of the partition size.
/// </summary>
[Fact]
public void Partition_WithRemainder_LastPartitionContainsLeftoverItems()
{
    var items = Enumerable.Range(1, 5).ToList();

    var partitions = items.Partition(2);

    partitions.Should().HaveCount(3);
    partitions[0].Should().BeEquivalentTo(new[] { 1, 2 });
    partitions[1].Should().BeEquivalentTo(new[] { 3, 4 });
    partitions[2].Should().BeEquivalentTo(new[] { 5 });
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Partition{T}(IEnumerable{T},int)"/> throws an <see cref="ArgumentException"/> when the partition size is zero.
/// </summary>
[Fact]
public void Partition_WithZeroSize_ThrowsArgumentException()
{
    var items = new[] { 1, 2, 3 };

    var act = () => items.Partition(0);

    act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*greater than '0'*");
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Partition{T}(IEnumerable{T},int)"/> returns an empty list when the source collection is empty.
/// </summary>
[Fact]
public void Partition_WithEmptySource_ReturnsEmptyList()
{
    var partitions = Array.Empty<int>().Partition(3);

    partitions.Should().BeEmpty();
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Partition{T}(IEnumerable{T},int)"/> works correctly with a single element.
/// </summary>
[Fact]
public void Partition_WithSingleElement_ReturnsSinglePartition()
{
    var items = new[] { 42 };

    var partitions = items.Partition(5);

    partitions.Should().HaveCount(1);
    partitions[0].Should().BeEquivalentTo(new[] { 42 });
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Partition{T}(IEnumerable{T},int)"/> works correctly with a large collection.
/// </summary>
[Fact]
public void Partition_WithLargeCollection_ProducesCorrectPartitions()
{
    var items = Enumerable.Range(1, 1000).ToList();

    var partitions = items.Partition(100);

    partitions.Should().HaveCount(10);
    partitions.Should().AllSatisfy(p => p.Should().HaveCount(100));
    partitions[0].Should().BeEquivalentTo(Enumerable.Range(1, 100).ToList());
    partitions[9].Should().BeEquivalentTo(Enumerable.Range(901, 100).ToList());
}

// ---- SkipWhile ---------------------------------------------------------------

/// <summary>
/// Tests that <see cref="CollectionExtensions.SkipWhile{T}(IEnumerable{T},Func{T,bool})"/> skips elements while the predicate is true and returns remaining elements.
/// </summary>
[Fact]
public void SkipWhile_SkipsInitialMatchingElements_ReturnsRemaining()
{
    var numbers = new[] { 1, 2, 3, 4, 5, 1, 2 };

    var result = numbers.SkipWhile(n => n < 4).ToList();

    result.Should().BeEquivalentTo(new[] { 4, 5, 1, 2 });
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.SkipWhile{T}(IEnumerable{T},Func{T,bool})"/> returns all elements when the first element doesn't match the predicate.
/// </summary>
[Fact]
public void SkipWhile_FirstElementDoesNotMatch_ReturnsAllElements()
{
    var numbers = new[] { 5, 4, 3, 2, 1 };

    var result = numbers.SkipWhile(n => n > 10).ToList();

    result.Should().BeEquivalentTo(new[] { 5, 4, 3, 2, 1 });
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.SkipWhile{T}(IEnumerable{T},Func{T,bool})"/> returns an empty sequence when all elements match the predicate.
/// </summary>
[Fact]
public void SkipWhile_AllElementsMatch_ReturnsEmpty()
{
    var numbers = new[] { 1, 2, 3 };

    var result = numbers.SkipWhile(n => n > 0).ToList();

    result.Should().BeEmpty();
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.SkipWhile{T}(IEnumerable{T},Func{T,bool})"/> works correctly with a single element that doesn't match the predicate.
/// </summary>
[Fact]
public void SkipWhile_SingleElementDoesNotMatch_ReturnsSingleElement()
{
    var numbers = new[] { 42 };

    var result = numbers.SkipWhile(n => n > 100).ToList();

    result.Should().BeEquivalentTo(new[] { 42 });
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.SkipWhile{T}(IEnumerable{T},Func{T,bool})"/> throws an <see cref="ArgumentNullException"/> when the source is null.
/// </summary>
[Fact]
public void SkipWhile_WithNullSource_ThrowsArgumentNullException()
{
    var act = () => ((IEnumerable<int>)null!).SkipWhile(n => n > 0).ToList();

    act.Should().Throw<ArgumentNullException>();
}

// ---- GetAtIndexOrDefault --------------------------------------------------

/// <summary>
/// Tests that <see cref="CollectionExtensions.GetAtIndexOrDefault{T}(IEnumerable{T},int,T?)"/> returns the element at the specified index when it exists.
/// </summary>
[Fact]
public void GetAtIndexOrDefault_WithValidIndex_ReturnsElement()
{
    var items = new[] { "a", "b", "c", "d" };

    var result = items.GetAtIndexOrDefault(2);

    result.Should().Be("c");
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.GetAtIndexOrDefault{T}(IEnumerable{T},int,T?)"/> returns the default value when the index is out of range.
/// </summary>
[Fact]
public void GetAtIndexOrDefault_WithOutOfRangeIndex_ReturnsDefault()
{
    var items = new[] { "a", "b", "c" };

    var result = items.GetAtIndexOrDefault(10);

    result.Should().BeNull();
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.GetAtIndexOrDefault{T}(IEnumerable{T},int,T?)"/> returns the specified default value when the index is out of range.
/// </summary>
[Fact]
public void GetAtIndexOrDefault_WithCustomDefaultValue_ReturnsCustomDefault()
{
    var items = new[] { 1, 2, 3 };

    var result = items.GetAtIndexOrDefault(100, -1);

    result.Should().Be(-1);
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.GetAtIndexOrDefault{T}(IEnumerable{T},int,T?)"/> works correctly with a single element.
/// </summary>
[Fact]
public void GetAtIndexOrDefault_WithSingleElement_ReturnsElementOrDefault()
{
    var items = new[] { "only" };

    var result = items.GetAtIndexOrDefault(0);
    result.Should().Be("only");

    var outOfRange = items.GetAtIndexOrDefault(5);
    outOfRange.Should().BeNull();
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.GetAtIndexOrDefault{T}(IEnumerable{T},int,T?)"/> throws an <see cref="ArgumentNullException"/> when the source is null.
/// </summary>
[Fact]
public void GetAtIndexOrDefault_WithNullSource_ThrowsArgumentNullException()
{
    var act = () => ((IEnumerable<string>)null!).GetAtIndexOrDefault(0);

    act.Should().Throw<ArgumentNullException>();
}

// ---- Shuffle ---------------------------------------------------------------

/// <summary>
/// Tests that <see cref="CollectionExtensions.Shuffle{T}(IEnumerable{T})"/> returns a new list containing all elements.
/// </summary>
[Fact]
public void Shuffle_ReturnsAllElements()
{
    var items = new[] { 1, 2, 3, 4, 5 };

    var shuffled = items.Shuffle().ToList();

    shuffled.Should().HaveCount(5);
    shuffled.Should().Contain(items);
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Shuffle{T}(IEnumerable{T})"/> returns a different order on subsequent calls for the same input.
/// </summary>
[Fact]
public void Shuffle_ProducesDifferentOrderings()
{
    var items = Enumerable.Range(1, 100).ToList();

    var first = items.Shuffle().ToList();
    var second = items.Shuffle().ToList();

    first.Should().NotEqual(second);
    first.Should().BeEquivalentTo(items);
    second.Should().BeEquivalentTo(items);
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Shuffle{T}(IEnumerable{T})"/> works correctly with a single element.
/// </summary>
[Fact]
public void Shuffle_WithSingleElement_ReturnsSameSingleElement()
{
    var items = new[] { 42 };

    var shuffled = items.Shuffle().ToList();

    shuffled.Should().HaveCount(1);
    shuffled[0].Should().Be(42);
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Shuffle{T}(IEnumerable{T})"/> throws an <see cref="ArgumentNullException"/> when the source is null.
/// </summary>
[Fact]
public void Shuffle_WithNullSource_ThrowsArgumentNullException()
{
    var act = () => ((IEnumerable<int>)null!).Shuffle().ToList();

    act.Should().Throw<ArgumentNullException>();
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.Shuffle{T}(IEnumerable{T})"/> works correctly with an empty collection.
/// </summary>
[Fact]
public void Shuffle_WithEmptyCollection_ReturnsEmpty()
{
    var items = Array.Empty<int>();

    var shuffled = items.Shuffle().ToList();

    shuffled.Should().BeEmpty();
}

// ---- ToKeyValueString ----------------------------------------------------

/// <summary>
/// Tests that <see cref="CollectionExtensions.ToKeyValueString{TKey,TValue}(Dictionary{TKey,TValue})/> with multiple entries produces a comma-separated string of key=value pairs.
/// </summary>
[Fact]
public void ToKeyValueString_WithMultipleEntries_ProducesCommaSeparatedPairs()
{
    var dict = new Dictionary<string, int>
    {
        { "key1", 100 },
        { "key2", 200 },
        { "key3", 300 }
    };

    var result = dict.ToKeyValueString();

    result.Should().Contain("key1=100");
    result.Should().Contain("key2=200");
    result.Should().Contain("key3=300");
    result.Should().Contain(", ");
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.ToKeyValueString{TKey,TValue}(Dictionary{TKey,TValue})/> with an empty dictionary returns an empty string.
/// </summary>
[Fact]
public void ToKeyValueString_WithEmptyDictionary_ReturnsEmptyString()
{
    var dict = new Dictionary<string, string>();

    var result = dict.ToKeyValueString();

    result.Should().BeEmpty();
}

/// <summary>
/// Tests that <see cref="CollectionExtensions.ToKeyValueString{TKey,TValue}(Dictionary{TKey,TValue})/> handles various value types correctly.
/// </summary>
[Fact]
public void ToKeyValueString_WithDifferentValueTypes_FormatsCorrectly()
{
    var dict = new Dictionary<string, object>
    {
        { "string", "value" },
        { "int", 42 },
        { "bool", true },
        { "null", null }
    };

    var result = dict.ToKeyValueString();

    result.Should().Contain("string=value");
    result.Should().Contain("int=42");
    result.Should().Contain("bool=True");
    result.Should().Contain("null=");
}