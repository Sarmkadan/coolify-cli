using CoolifyCli.Tests;
using Xunit;

public static class DeploymentDiffTestsExtensions
{
    public static bool HasComputeTests(this DeploymentDiffTests tests) 
    {
        return tests.GetType().GetMethods().Any(m => 
            m.Name == "Compute" && 
            m.GetParameters().Length == 0 && 
            typeof(void) == m.ReturnType);
    }

    public static bool HasDeploymentDiffEntryTests(this DeploymentDiffTests tests) 
    {
        return tests.GetType().GetMethods().Any(m => 
            m.Name == "DeploymentDiffEntry_HasChange" && 
            m.GetParameters().Length == 2 && 
            typeof(bool) == m.ReturnType);
    }

    public static void RunAllComputeTests(this DeploymentDiffTests tests) 
    {
        tests.Compute_WhenBothConfigurationsIdentical_ReportsNoChanges();
        tests.Compute_WhenBranchChanged_DetectsOneBranchChange();
        tests.Compute_WhenRepositoryChanged_FlagsHighRisk();
        tests.Compute_WhenOnlyBuildCommandChanged_IsNotHighRisk();
        tests.Compute_WhenEnvVarAdded_IncludesEnvVarChange();
        tests.Compute_WhenEnvVarRemoved_IncludesDeletionChange();
        tests.Compute_SetsApplicationIdAndName();
    }

    public static void RunAllDeploymentDiffEntryTests(this DeploymentDiffTests tests) 
    {
        tests.DeploymentDiffEntry_HasChange_ReturnsFalseForIdenticalValues();
        tests.DeploymentDiffEntry_HasChange_ReturnsTrueForDifferentValues();
    }
}
