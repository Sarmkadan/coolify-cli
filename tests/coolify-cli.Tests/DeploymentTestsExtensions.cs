using System;

namespace CoolifyCli.Tests
{
    /// <summary>
    /// Extension methods that make it easier to work with the <see cref="DeploymentTests"/> test class.
    /// These helpers simply invoke the existing public test methods in useful combinations.
    /// </summary>
    public static class DeploymentTestsExtensions
    {
        /// <summary>
        /// Executes all validation related test methods on the supplied <see cref="DeploymentTests"/> instance.
        /// </summary>
        public static void RunAllValidationTests(this DeploymentTests tests)
        {
            tests.Validate_WhenNameIsEmpty_IncludesNameRequiredError();
            tests.Validate_WithCompleteValidConfiguration_ReturnsNoErrors();
        }

        /// <summary>
        /// Simulates a typical deployment scenario by resetting failure state and then validating a correct configuration.
        /// </summary>
        public static void SimulateDeploymentScenario(this DeploymentTests tests)
        {
            tests.MarkAsDeployed_AfterPreviousFailures_ResetsFailureStateAndSetsTimestamp();
            tests.Validate_WithCompleteValidConfiguration_ReturnsNoErrors();
        }

        /// <summary>
        /// Executes the cache‑provider test that ensures a missing key triggers the factory delegate.
        /// </summary>
        public static void ExecuteCacheProviderTest(this DeploymentTests tests)
        {
            tests.CacheProvider_GetOrAdd_WhenKeyAbsent_DelegatesValueCreationToFactory();
        }

        /// <summary>
        /// Calls the failure‑accumulation test method multiple times.
        /// The default of three repetitions mirrors the typical failure‑threshold used elsewhere in the code base.
        /// </summary>
        /// <param name="repetitions">How many times to invoke the failure test.</param>
        public static void RunFailureSequence(this DeploymentTests tests, int repetitions = 3)
        {
            for (int i = 0; i < repetitions; i++)
            {
                tests.MarkAsFailed_CalledRepeatedly_AccumulatesFailureCountWithLatestMessage();
            }
        }
    }
}
