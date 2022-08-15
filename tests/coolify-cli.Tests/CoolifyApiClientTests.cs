#nullable enable

using CoolifyCli.Caching;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Unit tests for <see cref="MemoryCacheProvider"/> that verify caching behavior including set/get operations,
/// expiration, concurrency, and cache management functionality.
/// </summary>
public class MemoryCacheProviderTests : IDisposable
{
	internal readonly MemoryCacheProvider _cache;

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryCacheProviderTests"/> class.
	/// </summary>
	public MemoryCacheProviderTests()
	{
		// Use a very long cleanup interval so tests control cleanup manually
		_cache = new MemoryCacheProvider(cleanupInterval: TimeSpan.FromHours(1));
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose() => _cache.Dispose();

	/// <summary>
	/// Tests that values can be stored in the cache and retrieved successfully.
	/// </summary>
	[Fact]
	public void Set_AndGet_ReturnsStoredValue()
	{
		_cache.Set("key1", "hello");

		var result = _cache.Get<string>("key1");

		result.Should().Be("hello");
	}

	/// <summary>
	/// Tests that retrieving a non-existent key returns the default value (null).
	/// </summary>
	[Fact]
	public void Get_WhenKeyAbsent_ReturnsDefault()
	{
		var result = _cache.Get<string>("missing-key");

		result.Should().BeNull();
	}

	/// <summary>
	/// Tests that TryGet returns true and the correct value when the key exists.
	/// </summary>
	[Fact]
	public void TryGet_WhenKeyPresent_ReturnsTrueAndValue()
	{
		_cache.Set("app", new ApplicationDeployment { Name = "test-app" });

		var found = _cache.TryGet<ApplicationDeployment>("app", out var value);

		found.Should().BeTrue();
		value!.Name.Should().Be("test-app");
	}

	/// <summary>
	/// Tests that TryGet returns false when the key does not exist.
	/// </summary>
	[Fact]
	public void TryGet_WhenKeyAbsent_ReturnsFalse()
	{
		var found = _cache.TryGet<string>("ghost", out var value);

		found.Should().BeFalse();
		value.Should().BeNull();
	}

	/// <summary>
	/// Tests that Exists returns true when a key is present in the cache.
	/// </summary>
	[Fact]
	public void Exists_WhenKeyPresent_ReturnsTrue()
	{
		_cache.Set("present", 42);

		_cache.Exists("present").Should().BeTrue();
	}

	/// <summary>
	/// Tests that Exists returns false when a key is not present in the cache.
	/// </summary>
	[Fact]
	public void Exists_WhenKeyAbsent_ReturnsFalse()
	{
		_cache.Exists("absent").Should().BeFalse();
	}

	/// <summary>
	/// Tests that Remove deletes an entry from the cache.
	/// </summary>
	[Fact]
	public void Remove_DeletesEntryFromCache()
	{
		_cache.Set("to-remove", "value");
		_cache.Remove("to-remove");

		_cache.Exists("to-remove").Should().BeFalse();
	}

	/// <summary>
	/// Tests that Clear removes all entries from the cache.
	/// </summary>
	[Fact]
	public void Clear_RemovesAllEntries()
	{
		_cache.Set("k1", 1);
		_cache.Set("k2", 2);
		_cache.Set("k3", 3);

		_cache.Clear();

		_cache.Count.Should().Be(0);
	}

	/// <summary>
	/// Tests that Count reflects the actual number of stored entries.
	/// </summary>
	[Fact]
	public void Count_ReflectsNumberOfStoredEntries()
	{
		_cache.Set("a", 1);
		_cache.Set("b", 2);

		_cache.Count.Should().Be(2);
	}

	/// <summary>
	/// Tests that entries with expired TTL return null when retrieved.
	/// </summary>
	[Fact]
	public void Set_WithExpiredTtl_ReturnsNullOnGet()
	{
		_cache.Set("expiring", "soon", expiration: TimeSpan.FromMilliseconds(1));
		Thread.Sleep(10);

		var result = _cache.Get<string>("expiring");

		result.Should().BeNull();
	}

	/// <summary>
	/// Tests that Exists returns false after an entry has expired.
	/// </summary>
	[Fact]
	public void Exists_AfterEntryExpires_ReturnsFalse()
	{
		_cache.Set("temp", "data", expiration: TimeSpan.FromMilliseconds(1));
		Thread.Sleep(10);

		_cache.Exists("temp").Should().BeFalse();
	}

	/// <summary>
	/// Tests that GetOrAdd invokes the factory function when the key is absent,
	/// caches the result, and returns it. Subsequent calls should not invoke the factory.
	/// </summary>
	[Fact]
	public void GetOrAdd_WhenKeyAbsent_InvokesFactoryAndCachesResult()
	{
		var factoryCalled = 0;

		var result = _cache.GetOrAdd("new-key", () =>
		{
			factoryCalled++;
			return "factory-value";
		});

		result.Should().Be("factory-value");
		factoryCalled.Should().Be(1);

		// Second call should not invoke factory
		_cache.GetOrAdd("new-key", () =>
		{
			factoryCalled++;
			return "should-not-be-returned";
		});

		factoryCalled.Should().Be(1);
	}

	/// <summary>
	/// Tests that GetOrAddAsync invokes the async factory function when the key is absent,
	/// caches the result, and returns it.
	/// </summary>
	/// <returns>The cached value.</returns>
	[Fact]
	public async Task GetOrAddAsync_WhenKeyAbsent_InvokesAsyncFactoryAndCachesResult()
	{
		var result = await _cache.GetOrAddAsync("async-key", async () =>
		{
			await Task.Delay(1);
			return "async-value";
		});

		result.Should().Be("async-value");
		_cache.Exists("async-key").Should().BeTrue();
	}

	/// <summary>
	/// Tests that GetAllKeys returns all stored keys in the cache.
	/// </summary>
	[Fact]
	public void GetAllKeys_ReturnsAllStoredKeys()
	{
		_cache.Set("x", 1);
		_cache.Set("y", 2);

		var keys = _cache.GetAllKeys().ToList();

		keys.Should().Contain("x").And.Contain("y");
	}

	/// <summary>
	/// Tests that SetExpiration updates the expiration time of an existing entry
	/// and the entry is removed after the new expiration period.
	/// </summary>
	[Fact]
	public void SetExpiration_OnExistingEntry_RespectsNewExpiration()
	{
		_cache.Set("timed", "value");
		_cache.SetExpiration("timed", TimeSpan.FromMilliseconds(1));
		Thread.Sleep(10);

		_cache.Exists("timed").Should().BeFalse();
	}

	/// <summary>
	/// Tests that Set overwrites an existing entry with the new value.
	/// </summary>
	[Fact]
	public void Set_OverwritesExistingEntry()
	{
		_cache.Set("key", "old");
		_cache.Set("key", "new");

		_cache.Get<string>("key").Should().Be("new");
	}
}