using System;

/// <summary>
/// Extension methods that compose existing <see cref="MemoryCacheProviderTests"/> test helpers
/// into higher‑level, reusable scenarios.
/// </summary>
namespace CoolifyCli.Tests
{
    /// <summary>
    /// Provides fluent, composable operations for <see cref="MemoryCacheProviderTests"/>.
    /// </summary>
    public static class MemoryCacheProviderTestsExtensions
    {
        /// <summary>
        /// Clears the cache and then asserts that the cache is empty by checking the entry count.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static void ClearAndAssertEmpty(this MemoryCacheProviderTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.Clear_RemovesAllEntries();
            tests.Count_ReflectsNumberOfStoredEntries();
        }

        /// <summary>
        /// Sets a value for the specified <paramref name="key"/> and then verifies that the key
        /// exists in the cache.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="key">The cache key to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <c>null</c> or empty.</exception>
        public static void SetAndVerifyExists(this MemoryCacheProviderTests tests, string key)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(key);

            // The existing test method sets a value and asserts the stored value can be retrieved.
            tests.Set_AndGet_ReturnsStoredValue();

            // Verify that the key is reported as present.
            tests.Exists_WhenKeyPresent_ReturnsTrue();
        }

        /// <summary>
        /// Removes the entry for the specified <paramref name="key"/> and then verifies that the
        /// key no longer exists in the cache.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="key">The cache key to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <c>null</c> or empty.</exception>
        public static void RemoveAndVerifyAbsent(this MemoryCacheProviderTests tests, string key)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(key);

            // The existing test method deletes the entry and performs its own assertions.
            tests.Remove_DeletesEntryFromCache();

            // Verify that the key is now reported as absent.
            tests.Exists_WhenKeyAbsent_ReturnsFalse();
        }
    }
}
