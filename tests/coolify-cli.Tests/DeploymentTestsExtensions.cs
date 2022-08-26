using System;

namespace CoolifyCli.Tests
{
    /// <summary>
    /// Extension methods that make it easier to work with the <see cref="DeploymentTests"/> test class.
    /// These helpers compose existing public test methods into higher-level test scenarios.
    /// </summary>
    public static class DeploymentTestsExtensions
    {
        /// <summary>
        /// Executes all validation related test methods on the supplied <see cref="DeploymentTests"/> instance.
        /// </summary>
        /// <param name="tests">The test instance to execute against.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
        public static void RunAllValidationTests(this DeploymentTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            tests.Validate_WhenNameIsEmpty_IncludesNameRequiredError();
            tests.Validate_WithCompleteValidConfiguration_ReturnsNoErrors();
        }

        /// <summary>
        /// Simulates a typical deployment scenario by resetting failure state and then validating a correct configuration.
        /// </summary>
        /// <param name="tests">The test instance to execute against.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
        public static void SimulateDeploymentScenario(this DeploymentTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            tests.MarkAsDeployed_AfterPreviousFailures_ResetsFailureStateAndSetsTimestamp();
            tests.Validate_WithCompleteValidConfiguration_ReturnsNoErrors();
        }

        /// <summary>
        /// Executes the cache‑provider test that ensures a missing key triggers the factory delegate.
        /// </summary>
        /// <param name="tests">The test instance to execute against.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
        public static void ExecuteCacheProviderTest(this DeploymentTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            tests.CacheProvider_GetOrAdd_WhenKeyAbsent_DelegatesValueCreationToFactory();
        }

        /// <summary>
        /// Calls the failure‑accumulation test method multiple times.
        /// The default of three repetitions mirrors the typical failure‑threshold used elsewhere in the code base.
        /// </summary>
        /// <param name="tests">The test instance to execute against.</param>
        /// <param name="repetitions">How many times to invoke the failure test.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repetitions"/> is less than 1.</exception>
        public static void RunFailureSequence(this DeploymentTests tests, int repetitions = 3)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentOutOfRangeException.ThrowIfLessThan(repetitions, 1);

            for (int i = 0; i < repetitions; i++)
            {
                tests.MarkAsFailed_CalledRepeatedly_AccumulatesFailureCountWithLatestMessage();
            }
        }
    }
}