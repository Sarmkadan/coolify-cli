#nullable enable

using CoolifyCli.Extensions;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class EnumExtensionsTests
{
    // ---- GetDescription ------------------------------------------------------

    [Fact]
    public void GetDescription_WithNoDescriptionAttribute_ReturnsMemberName()
    {
        var status = DeploymentStatus.Deployed;

        status.GetDescription().Should().Be("Deployed");
    }

    // ---- ToDisplayString -----------------------------------------------------

    [Fact]
    public void ToDisplayString_SimpleEnumValue_ReturnsFormattedString()
    {
        var status = DeploymentStatus.InProgress;

        // "InProgress" has no description, so display formats the raw name
        var display = status.ToDisplayString();

        display.Should().NotBeEmpty();
        display.Should().ContainAll("I");
    }

    [Fact]
    public void ToDisplayString_AllDeploymentStatuses_ReturnNonEmptyStrings()
    {
        foreach (DeploymentStatus s in Enum.GetValues(typeof(DeploymentStatus)))
        {
            s.ToDisplayString().Should().NotBeNullOrEmpty();
        }
    }

    // ---- ParseEnum -----------------------------------------------------------

    [Fact]
    public void ParseEnum_WithExactMatch_ReturnsCorrectValue()
    {
        var result = "Deployed".ParseEnum<DeploymentStatus>();

        result.Should().Be(DeploymentStatus.Deployed);
    }

    [Fact]
    public void ParseEnum_CaseInsensitive_ReturnsCorrectValue()
    {
        var result = "failed".ParseEnum<DeploymentStatus>();

        result.Should().Be(DeploymentStatus.Failed);
    }

    [Fact]
    public void ParseEnum_WithInvalidValue_ThrowsArgumentException()
    {
        var act = () => "NotAStatus".ParseEnum<DeploymentStatus>();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*not a valid value*");
    }

    [Fact]
    public void ParseEnum_WithEmptyString_ThrowsArgumentException()
    {
        var act = () => "".ParseEnum<DeploymentStatus>();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    // ---- TryParseEnum --------------------------------------------------------

    [Fact]
    public void TryParseEnum_WithValidString_ReturnsValue()
    {
        var result = "pending".TryParseEnum<DeploymentStatus>();

        result.Should().Be(DeploymentStatus.Pending);
    }

    [Fact]
    public void TryParseEnum_WithInvalidString_ReturnsNull()
    {
        var result = "garbage".TryParseEnum<DeploymentStatus>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryParseEnum_WithNullString_ReturnsNull()
    {
        string? input = null;
        var result = input.TryParseEnum<DeploymentStatus>();

        result.Should().BeNull();
    }

    // ---- GetAllValues --------------------------------------------------------

    [Fact]
    public void GetAllValues_ReturnsAllDefinedEnumMembers()
    {
        var values = EnumExtensions.GetAllValues<DeploymentStatus>();

        values.Should().HaveCount(Enum.GetValues(typeof(DeploymentStatus)).Length);
        values.Should().Contain(DeploymentStatus.Pending);
        values.Should().Contain(DeploymentStatus.Failed);
    }

    // ---- GetValueDescriptionMap ----------------------------------------------

    [Fact]
    public void GetValueDescriptionMap_ContainsEntryForEachEnumMember()
    {
        var map = EnumExtensions.GetValueDescriptionMap<DatabaseType>();

        map.Should().HaveCount(Enum.GetValues(typeof(DatabaseType)).Length);
        map.Should().ContainKey(DatabaseType.PostgreSQL);
    }

    // ---- ToCliFormat ---------------------------------------------------------

    [Fact]
    public void ToCliFormat_CamelCaseValue_ProducesKebabCase()
    {
        var status = DeploymentStatus.InProgress;

        var cli = status.ToCliFormat();

        cli.Should().Be("in-progress");
    }

    [Fact]
    public void ToCliFormat_SingleWordValue_ReturnsLowercased()
    {
        var status = DeploymentStatus.Deployed;

        status.ToCliFormat().Should().Be("deployed");
    }

    // ---- ToInt / ToLong ------------------------------------------------------

    [Fact]
    public void ToInt_ReturnsUnderlyingIntegerValue()
    {
        DeploymentStatus.Pending.ToInt().Should().Be(0);
        DeploymentStatus.InProgress.ToInt().Should().Be(1);
    }

    [Fact]
    public void ToLong_ReturnsUnderlyingLongValue()
    {
        DeploymentStatus.Deployed.ToLong().Should().Be(2L);
    }

    // ---- EqualsIgnoreCase ----------------------------------------------------

    [Fact]
    public void EqualsIgnoreCase_WithMatchingName_ReturnsTrue()
    {
        DeploymentStatus.Failed.EqualsIgnoreCase("FAILED").Should().BeTrue();
    }

    [Fact]
    public void EqualsIgnoreCase_WithDifferentName_ReturnsFalse()
    {
        DeploymentStatus.Failed.EqualsIgnoreCase("Deployed").Should().BeFalse();
    }

    // ---- GetDisplayStrings ---------------------------------------------------

    [Fact]
    public void GetDisplayStrings_ReturnsOneStringPerEnumMember()
    {
        var displays = EnumExtensions.GetDisplayStrings<SeverityLevel>();

        displays.Should().HaveCount(Enum.GetValues(typeof(SeverityLevel)).Length);
        displays.Should().AllSatisfy(s => s.Should().NotBeNullOrEmpty());
    }
}
