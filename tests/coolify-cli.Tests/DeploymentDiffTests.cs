#nullable enable
using CoolifyCli.Models;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace CoolifyCli.Tests;

public class DeploymentDiffTests
{
    private static ApplicationDeployment BaseDeploy(int id = 1) => new()
    {
        Id            = id,
        Name          = "my-service",
        Repository    = "https://github.com/org/my-service",
        Branch        = "main",
        EnvironmentId = "env-prod",
        BuildCommand  = "dotnet publish",
        StartCommand  = "dotnet run",
        Ports         = new List<string> { "8080" },
        HealthCheckIntervalSeconds = 30,
        EnvironmentVariables = new Dictionary<string, string> { ["LOG_LEVEL"] = "info" }
    };

    /// <summary>
    /// Verifies that when both configurations are identical, the deployment diff reports no changes.
    /// </summary>
    [Fact]
    public void Compute_WhenBothConfigurationsIdentical_ReportsNoChanges()
    {
        var current  = BaseDeploy();
        var proposed = BaseDeploy();

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.HasChanges.Should().BeFalse();
        diff.Changes.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that when the branch is changed, the deployment diff detects one branch change.
    /// </summary>
    [Fact]
    public void Compute_WhenBranchChanged_DetectsOneBranchChange()
    {
        var current  = BaseDeploy();
        var proposed = BaseDeploy();
        proposed.Branch = "release/v2";

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.HasChanges.Should().BeTrue();
        diff.Changes.Should().ContainSingle(e => e.Property == "Branch");
        diff.Changes.Single(e => e.Property == "Branch").CurrentValue.Should().Be("main");
        diff.Changes.Single(e => e.Property == "Branch").ProposedValue.Should().Be("release/v2");
    }

    /// <summary>
    /// Verifies that when the repository is changed, the deployment diff flags high risk.
    /// </summary>
    [Fact]
    public void Compute_WhenRepositoryChanged_FlagsHighRisk()
    {
        var current  = BaseDeploy();
        var proposed = BaseDeploy();
        proposed.Repository = "https://github.com/org/new-repo";

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.IsHighRisk.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that when only the build command is changed, the deployment diff is not high risk.
    /// </summary>
    [Fact]
    public void Compute_WhenOnlyBuildCommandChanged_IsNotHighRisk()
    {
        var current  = BaseDeploy();
        var proposed = BaseDeploy();
        proposed.BuildCommand = "dotnet publish -c Release";

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.HasChanges.Should().BeTrue();
        diff.IsHighRisk.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that when an environment variable is added, the deployment diff includes the environment variable change.
    /// </summary>
    [Fact]
    public void Compute_WhenEnvVarAdded_IncludesEnvVarChange()
    {
        var current  = BaseDeploy();
        var proposed = BaseDeploy();
        proposed.EnvironmentVariables["NEW_VAR"] = "new-value";

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.HasChanges.Should().BeTrue();
        diff.Changes.Should().Contain(e => e.Property == "env:NEW_VAR");
        diff.Changes.Single(e => e.Property == "env:NEW_VAR").CurrentValue.Should().Be("(not set)");
        diff.Changes.Single(e => e.Property == "env:NEW_VAR").ProposedValue.Should().Be("new-value");
    }

    /// <summary>
    /// Verifies that when an environment variable is removed, the deployment diff includes the deletion change.
    /// </summary>
    [Fact]
    public void Compute_WhenEnvVarRemoved_IncludesDeletionChange()
    {
        var current  = BaseDeploy();
        var proposed = BaseDeploy();
        proposed.EnvironmentVariables.Remove("LOG_LEVEL");

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.HasChanges.Should().BeTrue();
        var logChange = diff.Changes.Single(e => e.Property == "env:LOG_LEVEL");
        logChange.CurrentValue.Should().Be("info");
        logChange.ProposedValue.Should().Be("(not set)");
    }

    /// <summary>
    /// Verifies that the deployment diff sets the application ID and name.
    /// </summary>
    [Fact]
    public void Compute_SetsApplicationIdAndName()
    {
        var current  = BaseDeploy(id: 42);
        var proposed = BaseDeploy(id: 42);
        proposed.Branch = "feature/x";

        var diff = DeploymentDiff.Compute(current, proposed);

        diff.ApplicationId.Should().Be(42);
        diff.ApplicationName.Should().Be("my-service");
    }

    /// <summary>
    /// Verifies that the DeploymentDiffEntry class has a HasChange property that returns false for identical values.
    /// </summary>
    [Fact]
    public void DeploymentDiffEntry_HasChange_ReturnsFalseForIdenticalValues()
    {
        var entry = new DeploymentDiffEntry
        {
            Property      = "Branch",
            CurrentValue  = "main",
            ProposedValue = "main"
        };

        entry.HasChange.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the DeploymentDiffEntry class has a HasChange property that returns true for different values.
    /// </summary>
    [Fact]
    public void DeploymentDiffEntry_HasChange_ReturnsTrueForDifferentValues()
    {
        var entry = new DeploymentDiffEntry
        {
            Property      = "Branch",
            CurrentValue  = "main",
            ProposedValue = "develop"
        };

        entry.HasChange.Should().BeTrue();
    }
}
