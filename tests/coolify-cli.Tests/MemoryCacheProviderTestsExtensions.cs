#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Extension methods for <see cref="MemoryCacheProviderTests"/> providing additional test scenarios and helper methods
/// for verifying cache behavior under various conditions.
/// </summary>
public static class MemoryCacheProviderTestsExtensions
{
    /// <summary>
    /// Verifies that the cache correctly handles null values.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The cache key to use.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null or key is null/empty.</exception>
    public static void Set_AndGet_NullValue_ReturnsNull(this MemoryCacheProviderTests test, string key)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(key);

        test._cache.Set<string>(key, null);
        var result = test._cache.Get<string>(key);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the cache correctly handles value types.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Set_AndGet_ValueType_ReturnsStoredValue(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        test._cache.Set("int-key", 42);
        test._cache.Set("bool-key", true);
        test._cache.Set("date-key", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var intResult = test._cache.Get<int>("int-key");
        var boolResult = test._cache.Get<bool>("bool-key");
        var dateResult = test._cache.Get<DateTime>("date-key");

        intResult.Should().Be(42);
        boolResult.Should().BeTrue();
        dateResult.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that the cache correctly handles complex object types.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Set_AndGet_ComplexObject_ReturnsStoredObject(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var app = new ApplicationDeployment
        {
            Name = "test-application",
            Status = DeploymentStatus.Deployed,
            CreatedAt = DateTime.UtcNow
        };

        test._cache.Set("complex-app", app);
        var result = test._cache.Get<ApplicationDeployment>("complex-app");

        result.Should().NotBeNull();
        result!.Name.Should().Be("test-application");
        result.Status.Should().Be(DeploymentStatus.Deployed);
    }

    /// <summary>
    /// Verifies that TryGet works correctly with value types.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void TryGet_ValueType_WhenKeyPresent_ReturnsTrueAndValue(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        test._cache.Set("value-type-key", 123);
        var found = test._cache.TryGet<int>("value-type-key", out var value);

        found.Should().BeTrue();
        value.Should().Be(123);
    }

    /// <summary>
    /// Verifies that multiple operations can be performed concurrently without issues.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void ConcurrentOperations_DoNotCauseRaceConditions(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var tasks = new List<Task>();
        var results = new List<bool>();

        // Start multiple concurrent operations
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                var key = $"concurrent-{index}";
                test._cache.Set(key, index);
                var exists = test._cache.Exists(key);
                results.Add(exists);
                var retrievedValue = test._cache.Get<int>(key);
                results.Add(retrievedValue == index);
            }));
        }

        Task.WaitAll([.. tasks]);

        results.Should().AllBeEquivalentTo(true);
        test._cache.Count.Should().Be(10);
    }

    /// <summary>
    /// Verifies that GetOrAdd with expiration works correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void GetOrAdd_WithExpiration_RespectsExpiration(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var factoryCallCount = 0;

        // First call should invoke factory and cache
        var result1 = test._cache.GetOrAdd("expiring-key", () =>
        {
            factoryCallCount++;
            return "value";
        }, TimeSpan.FromMilliseconds(5));

        result1.Should().Be("value");
        factoryCallCount.Should().Be(1);
        test._cache.Exists("expiring-key").Should().BeTrue();

        // Wait for expiration
        Task.Delay(10).Wait();

        // Second call should invoke factory again since entry expired
        var result2 = test._cache.GetOrAdd("expiring-key", () =>
        {
            factoryCallCount++;
            return "new-value";
        }, TimeSpan.FromMilliseconds(5));

        result2.Should().Be("new-value");
        factoryCallCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GetAllKeys returns keys in a consistent order.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>An ordered list of keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static IReadOnlyList<string> GetAllKeys_ShouldReturnConsistentOrder(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        test._cache.Set("zebra", 1);
        test._cache.Set("apple", 2);
        test._cache.Set("mango", 3);

        var keys = test._cache.GetAllKeys().ToList();
        keys.Should().HaveCount(3);

        return keys.OrderBy(k => k, StringComparer.Ordinal).ToList().AsReadOnly();
    }

    /// <summary>
    /// Verifies that removing a non-existent key doesn't throw.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Remove_NonExistentKey_DoesNotThrow(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        Action act = () => test._cache.Remove("non-existent-key");
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that clearing an empty cache doesn't throw.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Clear_EmptyCache_DoesNotThrow(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        Action act = () => test._cache.Clear();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that Exists returns false for expired entries.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Exists_ExpiredEntry_ReturnsFalse(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        test._cache.Set("expiring-soon", "data", TimeSpan.FromMilliseconds(1));
        Task.Delay(10).Wait();

        test._cache.Exists("expiring-soon").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Set with zero expiration works correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Set_WithZeroExpiration_EntryExpiresImmediately(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        test._cache.Set("zero-ttl", "immediate", TimeSpan.Zero);
        test._cache.Exists("zero-ttl").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetOrAddAsync with expiration works correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static async Task GetOrAddAsync_WithExpiration_RespectsExpiration(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var factoryCallCount = 0;

        // First call should invoke factory and cache
        var result1 = await test._cache.GetOrAddAsync("async-expiring-key", async () =>
        {
            await Task.Delay(1);
            factoryCallCount++;
            return "async-value";
        }, TimeSpan.FromMilliseconds(5));

        result1.Should().Be("async-value");
        factoryCallCount.Should().Be(1);
        test._cache.Exists("async-expiring-key").Should().BeTrue();

        // Wait for expiration
        await Task.Delay(10);

        // Second call should invoke factory again since entry expired
        var result2 = await test._cache.GetOrAddAsync("async-expiring-key", async () =>
        {
            await Task.Delay(1);
            factoryCallCount++;
            return "new-async-value";
        }, TimeSpan.FromMilliseconds(5));

        result2.Should().Be("new-async-value");
        factoryCallCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that the cache handles large numbers of entries efficiently.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Set_ManyEntries_CountIsAccurate(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        for (int i = 0; i < 1000; i++)
        {
            test._cache.Set($"key-{i}", i);
        }

        test._cache.Count.Should().Be(1000);
        var keys = test._cache.GetAllKeys().ToList();
        keys.Should().HaveCount(1000);
    }

    /// <summary>
    /// Verifies that Get returns default for value types when key is absent.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Get_ValueTypeAbsent_ReturnsDefault(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var result = test._cache.Get<int>("missing-int-key");
        result.Should().Be(0);

        var dateResult = test._cache.Get<DateTime>("missing-date-key");
        dateResult.Should().Be(default);
    }

    /// <summary>
    /// Verifies that SetExpiration on non-existent key doesn't throw.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void SetExpiration_NonExistentKey_DoesNotThrow(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        Action act = () => test._cache.SetExpiration("ghost-key", TimeSpan.FromHours(1));
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that Clear removes all entries including those with different types.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void Clear_RemovesAllEntryTypes(this MemoryCacheProviderTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        test._cache.Set("string-key", "hello");
        test._cache.Set("int-key", 42);
        test._cache.Set("bool-key", true);
        test._cache.Set("object-key", new ApplicationDeployment { Name = "app" });

        test._cache.Clear();
        test._cache.Count.Should().Be(0);

        test._cache.Exists("string-key").Should().BeFalse();
        test._cache.Exists("int-key").Should().BeFalse();
        test._cache.Exists("bool-key").Should().BeFalse();
        test._cache.Exists("object-key").Should().BeFalse();
    }
}
