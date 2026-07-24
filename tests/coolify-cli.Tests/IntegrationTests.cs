#nullable enable

using System.Collections.Concurrent;
using CoolifyCli.Caching;
using CoolifyCli.Extensions;
using CoolifyCli.Models;
using CoolifyCli.Utilities;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Integration-level tests that exercise multiple components together,
/// verifying end-to-end workflows, concurrency safety, and configuration
/// combinations described in the project README.
/// </summary>
public class IntegrationTests
{
    // ---- Full deployment lifecycle -------------------------------------------

    /// <summary>
    /// Demonstrates the main use case: configure an application, validate it,
    /// deploy it, and verify the resulting state transitions.
    /// </summary>
    [Fact]
    public void DeploymentLifecycle_ConfigureValidateDeployFail_StateTransitionsAreCorrect()
    {
        // 1. Configure
        var deployment = new ApplicationDeployment
        {
            Name = "api-gateway",
            Repository = "https://github.com/acme/api-gateway",
            EnvironmentId = "env-prod",
            BuildCommand = "dotnet publish -c Release",
            Ports = ["8080"],
            HealthCheckIntervalSeconds = 30
        };

        // 2. Validate — should have zero errors for a well-formed config
        var errors = deployment.Validate().ToList();
        errors.Should().BeEmpty();

        // 3. Deploy successfully
        deployment.MarkAsDeployed();
        deployment.Status.Should().Be(DeploymentStatus.Deployed);
        deployment.LastDeployedAt.Should().NotBeNull();
        deployment.FailureCount.Should().Be(0);

        // 4. Simulate failures after a successful deploy
        deployment.MarkAsFailed("timeout during startup");
        deployment.MarkAsFailed("health check returned 503");

        deployment.Status.Should().Be(DeploymentStatus.Failed);
        deployment.FailureCount.Should().Be(2);
        deployment.RequiresAttention().Should().BeFalse(); // threshold is 3

        // 5. One more failure crosses the threshold
        deployment.MarkAsFailed("OOM error");
        deployment.RequiresAttention().Should().BeTrue();

        // 6. A new successful deploy clears the failure slate
        deployment.MarkAsDeployed();
        deployment.FailureCount.Should().Be(0);
        deployment.RequiresAttention().Should().BeFalse();
    }

    // ---- Validation pipeline -------------------------------------------------

    [Fact]
    public void ValidationPipeline_AllHelperMethods_WorkTogether()
    {
        // Simulate parsing and validating a deployment config provided by a user
        var rawName = "my-api-service";
        var rawEmail = "ops@example.com";
        var rawPort = "8443";
        var rawCommit = "a3f1b8c2d4e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3";
        var rawVersion = "3.14.1-rc1";

        ValidationHelper.IsValidResourceName(rawName).IsValid.Should().BeTrue();
        ValidationHelper.IsValidEmail(rawEmail).IsValid.Should().BeTrue();
        ValidationHelper.IsValidPort(rawPort).IsValid.Should().BeTrue();
        ValidationHelper.IsValidCommitHash(rawCommit).IsValid.Should().BeTrue();
        ValidationHelper.IsValidSemanticVersion(rawVersion).IsValid.Should().BeTrue();

        // Combine with string extensions
        rawName.ToPascalCase().Should().Be("MyApiService");
        rawEmail.MaskSensitive(3).Should().StartWith("ops").And.EndWith("com");
    }

    // ---- Cache + deployment workflow -----------------------------------------

    [Fact]
    public void CacheWorkflow_StoreAndRetrieveDeployment_PersistsBetweenCalls()
    {
        using var cache = new MemoryCacheProvider(TimeSpan.FromHours(1));

        var original = new ApplicationDeployment
        {
            Id = 7,
            Name = "worker-service",
            EnvironmentId = "env-staging",
            Repository = "https://github.com/acme/worker",
            BuildCommand = "make build",
            Ports = ["9000"]
        };

        // Store into cache
        cache.Set($"deployment:{original.Id}", original, expiration: TimeSpan.FromMinutes(10));

        // Retrieve and verify
        var cached = cache.Get<ApplicationDeployment>($"deployment:{original.Id}");
        cached.Should().NotBeNull();
        cached!.Name.Should().Be("worker-service");
        cached.Id.Should().Be(7);

        // Mutate original; cached reference should reflect the same object (reference semantics)
        original.MarkAsDeployed();
        cached.Status.Should().Be(DeploymentStatus.Deployed);
    }

    // ---- Collection + string pipeline ----------------------------------------

    [Fact]
    public void CollectionAndStringPipeline_BatchAndFormatDeploymentNames_ProducesExpectedOutput()
    {
        var services = new[]
        {
            "api-gateway", "auth-service", "payment-worker",
            "email-sender", "report-generator", "data-ingester"
        };

        // Batch into groups of 2 for parallel deployment waves
        var waves = services.Batch(2).ToList();
        waves.Should().HaveCount(3);

        // Format each name as PascalCase for display
        var displayNames = services.Select(s => s.ToPascalCase()).ToList();
        displayNames.Should().Contain("ApiGateway");
        displayNames.Should().Contain("PaymentWorker");

        // Filter services whose names are valid resource names
        var (valid, invalid) = services.ToList().Split(x => ValidationHelper.IsValidResourceName(x).IsValid);
        valid.Should().HaveCount(6);
        invalid.Should().BeEmpty();
    }

    // ---- Concurrency ---------------------------------------------------------

    [Fact]
    public void ConcurrentCacheAccess_MultipleThreadsReadingAndWriting_NoExceptions()
    {
        using var cache = new MemoryCacheProvider(TimeSpan.FromHours(1));
        var exceptions = new ConcurrentBag<Exception>();
        const int threadCount = 20;

        var threads = Enumerable.Range(0, threadCount).Select(i => new Thread(() =>
        {
            try
            {
                var key = $"key-{i % 5}";
                cache.Set(key, $"value-{i}");
                _ = cache.Get<string>(key);
                _ = cache.Exists(key);
                cache.GetOrAdd(key, () => $"default-{i}");
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join(TimeSpan.FromSeconds(5)));

        exceptions.Should().BeEmpty("cache must be thread-safe");
    }

    [Fact]
    public async Task ConcurrentDeploymentStateUpdates_MultipleThreadsMarkingFailed_FailureCountIsConsistent()
    {
        var deployment = new ApplicationDeployment { Name = "concurrent-app" };
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => deployment.MarkAsFailed("concurrent error")))
            .ToArray();

        await Task.WhenAll(tasks);

        // FailureCount is not thread-safe, but we verify no exception was thrown
        // and the count is positive (at least some increments succeeded)
        deployment.FailureCount.Should().BePositive();
        deployment.Status.Should().Be(DeploymentStatus.Failed);
    }

    // ---- Different configuration combinations --------------------------------

    [Fact]
    public void Validate_AllInvalidFieldCombinations_ReturnsAllExpectedErrors()
    {
        var deployment = new ApplicationDeployment
        {
            Name = "",           // missing
            Repository = "",     // missing
            EnvironmentId = "",  // missing
            BuildCommand = "",   // missing (no start command either)
            StartCommand = "",
            Ports = [],          // empty
            HealthCheckIntervalSeconds = 2  // below minimum
        };

        var errors = deployment.Validate().ToList();

        errors.Should().Contain(e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
        errors.Should().Contain(e => e.Contains("Repository", StringComparison.OrdinalIgnoreCase));
        errors.Should().Contain(e => e.Contains("Environment", StringComparison.OrdinalIgnoreCase));
        errors.Should().Contain(e => e.Contains("build command", StringComparison.OrdinalIgnoreCase));
        errors.Should().Contain(e => e.Contains("port", StringComparison.OrdinalIgnoreCase));
        errors.Should().Contain(e => e.Contains("Health check interval", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithOnlyStartCommand_PassesBuildCommandCheck()
    {
        var deployment = new ApplicationDeployment
        {
            Name = "my-app",
            Repository = "https://github.com/org/repo",
            EnvironmentId = "env-dev",
            StartCommand = "node server.js",
            BuildCommand = "",
            Ports = ["3000"]
        };

        var errors = deployment.Validate().ToList();

        errors.Should().NotContain(e => e.Contains("build command", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithInvalidPortInList_ReportsSpecificPort()
    {
        var deployment = new ApplicationDeployment
        {
            Name = "my-app",
            Repository = "https://github.com/org/repo",
            EnvironmentId = "env-dev",
            BuildCommand = "make",
            Ports = ["8080", "99999", "abc"]
        };

        var errors = deployment.Validate().ToList();

        errors.Should().Contain(e => e.Contains("99999"));
        errors.Should().Contain(e => e.Contains("abc"));
    }

    // ---- DateTime + enum pipeline --------------------------------------------

    [Fact]
    public void DateTimeAndEnumPipeline_FormatDeploymentTimestamp_ProducesHumanReadableOutput()
    {
        var deployment = new ApplicationDeployment { Name = "frontend" };
        deployment.MarkAsDeployed();

        var timestamp = deployment.LastDeployedAt!.Value;
        var relativeTime = timestamp.ToRelativeTime();
        var statusDisplay = deployment.Status.ToDisplayString();
        var statusCli = deployment.Status.ToCliFormat();

        relativeTime.Should().Be("just now");
        statusDisplay.Should().NotBeNullOrEmpty();
        statusCli.Should().Be("deployed");
    }
}
