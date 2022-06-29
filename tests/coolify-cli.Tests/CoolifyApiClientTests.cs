#nullable enable

using CoolifyCli.Caching;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class MemoryCacheProviderTests : IDisposable
{
    private readonly MemoryCacheProvider _cache;

    public MemoryCacheProviderTests()
    {
        // Use a very long cleanup interval so tests control cleanup manually
        _cache = new MemoryCacheProvider(cleanupInterval: TimeSpan.FromHours(1));
    }

    public void Dispose() => _cache.Dispose();

    [Fact]
    public void Set_AndGet_ReturnsStoredValue()
    {
        _cache.Set("key1", "hello");

        var result = _cache.Get<string>("key1");

        result.Should().Be("hello");
    }

    [Fact]
    public void Get_WhenKeyAbsent_ReturnsDefault()
    {
        var result = _cache.Get<string>("missing-key");

        result.Should().BeNull();
    }

    [Fact]
    public void TryGet_WhenKeyPresent_ReturnsTrueAndValue()
    {
        _cache.Set("app", new ApplicationDeployment { Name = "test-app" });

        var found = _cache.TryGet<ApplicationDeployment>("app", out var value);

        found.Should().BeTrue();
        value!.Name.Should().Be("test-app");
    }

    [Fact]
    public void TryGet_WhenKeyAbsent_ReturnsFalse()
    {
        var found = _cache.TryGet<string>("ghost", out var value);

        found.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Exists_WhenKeyPresent_ReturnsTrue()
    {
        _cache.Set("present", 42);

        _cache.Exists("present").Should().BeTrue();
    }

    [Fact]
    public void Exists_WhenKeyAbsent_ReturnsFalse()
    {
        _cache.Exists("absent").Should().BeFalse();
    }

    [Fact]
    public void Remove_DeletesEntryFromCache()
    {
        _cache.Set("to-remove", "value");
        _cache.Remove("to-remove");

        _cache.Exists("to-remove").Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        _cache.Set("k1", 1);
        _cache.Set("k2", 2);
        _cache.Set("k3", 3);

        _cache.Clear();

        _cache.Count.Should().Be(0);
    }

    [Fact]
    public void Count_ReflectsNumberOfStoredEntries()
    {
        _cache.Set("a", 1);
        _cache.Set("b", 2);

        _cache.Count.Should().Be(2);
    }

    [Fact]
    public void Set_WithExpiredTtl_ReturnsNullOnGet()
    {
        _cache.Set("expiring", "soon", expiration: TimeSpan.FromMilliseconds(1));
        Thread.Sleep(10);

        var result = _cache.Get<string>("expiring");

        result.Should().BeNull();
    }

    [Fact]
    public void Exists_AfterEntryExpires_ReturnsFalse()
    {
        _cache.Set("temp", "data", expiration: TimeSpan.FromMilliseconds(1));
        Thread.Sleep(10);

        _cache.Exists("temp").Should().BeFalse();
    }

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

    [Fact]
    public void GetAllKeys_ReturnsAllStoredKeys()
    {
        _cache.Set("x", 1);
        _cache.Set("y", 2);

        var keys = _cache.GetAllKeys().ToList();

        keys.Should().Contain("x").And.Contain("y");
    }

    [Fact]
    public void SetExpiration_OnExistingEntry_RespectsNewExpiration()
    {
        _cache.Set("timed", "value");
        _cache.SetExpiration("timed", TimeSpan.FromMilliseconds(1));
        Thread.Sleep(10);

        _cache.Exists("timed").Should().BeFalse();
    }

    [Fact]
    public void Set_OverwritesExistingEntry()
    {
        _cache.Set("key", "old");
        _cache.Set("key", "new");

        _cache.Get<string>("key").Should().Be("new");
    }
}