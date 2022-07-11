#nullable enable

using CoolifyCli.Extensions;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the EnumExtensions class.
/// </summary>
public class EnumExtensionsTests
{
    // ---- GetDescription ------------------------------------------------------

    /// <summary>
    /// Verifies that GetDescription returns the member name when no Description attribute is present.
    /// </summary>
    [Fact]
    public void GetDescription_WithNoDescriptionAttribute_ReturnsMemberName()
    {
        var status = DeploymentStatus.Deployed;

        status.GetDescription().Should().Be("Deployed");
    }

    // ---- ToDisplayString -----------------------------------------------------

    /// <summary>
    /// Verifies that ToDisplayString returns a formatted string for a simple enum value.
    /// </summary>
    [Fact]
    public void ToDisplayString_SimpleEnumValue_ReturnsFormattedString()
    {
        var status = DeploymentStatus.InProgress;

        // "InProgress" has no description, so display formats the raw name
        var display = status.ToDisplayString();

        display.Should().NotBeEmpty();
        display.Should().ContainAll("I");
    }

    /// <summary>
    /// Verifies that ToDisplayString returns non-empty strings for all deployment statuses.
    /// </summary>
    [Fact]
    public void ToDisplayString_AllDeploymentStatuses_ReturnNonEmptyStrings()
    {
        foreach (DeploymentStatus s in Enum.GetValues(typeof(DeploymentStatus)))
        {
            s.ToDisplayString().Should().NotBeNullOrEmpty();
        }
    }

    // ---- ParseEnum -----------------------------------------------------------

    /// <summary>
    /// Verifies that ParseEnum returns the correct value when the input matches exactly.
    /// </summary>
    [Fact]
    public void ParseEnum_WithExactMatch_ReturnsCorrectValue()
    {
        var result = "Deployed".ParseEnum<DeploymentStatus>();

        result.Should().Be(DeploymentStatus.Deployed);
    }

    /// <summary>
    /// Verifies that ParseEnum returns the correct value when the input matches case-insensitively.
    /// </summary>
    [Fact]
    public void ParseEnum_CaseInsensitive_ReturnsCorrectValue()
    {
        var result = "failed".ParseEnum<DeploymentStatus>();

        result.Should().Be(DeploymentStatus.Failed);
    }

    /// <summary>
    /// Verifies that ParseEnum throws an ArgumentException when the input is invalid.
    /// </summary>
    [Fact]
    public void ParseEnum_WithInvalidValue_ThrowsArgumentException()
    {
        var act = () => "NotAStatus".ParseEnum<DeploymentStatus>();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*not a valid value*");
    }

    /// <summary>
    /// Verifies that ParseEnum throws an ArgumentException when the input is empty.
    /// </summary>
    [Fact]
    public void ParseEnum_WithEmptyString_ThrowsArgumentException()
    {
        var act = () => "".ParseEnum<DeploymentStatus>();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    // ---- TryParseEnum --------------------------------------------------------

    /// <summary>
    /// Verifies that TryParseEnum returns the correct value when the input is valid.
    /// </summary>
    [Fact]
    public void TryParseEnum_WithValidString_ReturnsValue()
    {
        var result = "pending".TryParseEnum<DeploymentStatus>();

        result.Should().Be(DeploymentStatus.Pending);
    }

    /// <summary>
    /// Verifies that TryParseEnum returns null when the input is invalid.
    /// </summary>
    [Fact]
    public void TryParseEnum_WithInvalidString_ReturnsNull()
    {
        var result = "garbage".TryParseEnum<DeploymentStatus>();

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that TryParseEnum returns null when the input is null.
    /// </summary>
    [Fact]
    public void TryParseEnum_WithNullString_ReturnsNull()
    {
        string? input = null;
        var result = input.TryParseEnum<DeploymentStatus>();

        result.Should().BeNull();
    }

    // ---- GetAllValues --------------------------------------------------------

    /// <summary>
    /// Verifies that GetAllValues returns all defined enum members.
    /// </summary>
    [Fact]
    public void GetAllValues_ReturnsAllDefinedEnumMembers()
    {
        var values = EnumExtensions.GetAllValues<DeploymentStatus>();

        values.Should().HaveCount(Enum.GetValues(typeof(DeploymentStatus)).Length);
        values.Should().Contain(DeploymentStatus.Pending);
        values.Should().Contain(DeploymentStatus.Failed);
    }

    // ---- GetValueDescriptionMap ----------------------------------------------

    /// <summary>
    /// Verifies that GetValueDescriptionMap contains an entry for each enum member.
    /// </summary>
    [Fact]
    public void GetValueDescriptionMap_ContainsEntryForEachEnumMember()
    {
        var map = EnumExtensions.GetValueDescriptionMap<DatabaseType>();

        map.Should().HaveCount(Enum.GetValues(typeof(DatabaseType)).Length);
        map.Should().ContainKey(DatabaseType.PostgreSQL);
    }

    // ---- ToCliFormat ---------------------------------------------------------

    /// <summary>
    /// Verifies that ToCliFormat produces kebab-case for a camel-case value.
    /// </summary>
    [Fact]
    public void ToCliFormat_CamelCaseValue_ProducesKebabCase()
    {
        var status = DeploymentStatus.InProgress;

        var cli = status.ToCliFormat();

        cli.Should().Be("in-progress");
    }

    /// <summary>
    /// Verifies that ToCliFormat returns a lowercased string for a single-word value.
    /// </summary>
    [Fact]
    public void ToCliFormat_SingleWordValue_ReturnsLowercased()
    {
        var status = DeploymentStatus.Deployed;

        status.ToCliFormat().Should().Be("deployed");
    }

    // ---- ToInt / ToLong ------------------------------------------------------

    /// <summary>
    /// Verifies that ToInt returns the underlying integer value.
    /// </summary>
    [Fact]
    public void ToInt_ReturnsUnderlyingIntegerValue()
    {
        DeploymentStatus.Pending.ToInt().Should().Be(0);
        DeploymentStatus.InProgress.ToInt().Should().Be(1);
    }

    /// <summary>
    /// Verifies that ToLong returns the underlying long value.
    /// </summary>
    [Fact]
    public void ToLong_ReturnsUnderlyingLongValue()
    {
        DeploymentStatus.Deployed.ToLong().Should().Be(2L);
    }

    // ---- EqualsIgnoreCase ----------------------------------------------------

    /// <summary>
    /// Verifies that EqualsIgnoreCase returns true when the input matches the enum member name.
    /// </summary>
    [Fact]
    public void EqualsIgnoreCase_WithMatchingName_ReturnsTrue()
    {
        DeploymentStatus.Failed.EqualsIgnoreCase("FAILED").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that EqualsIgnoreCase returns false when the input does not match the enum member name.
    /// </summary>
    [Fact]
    public void EqualsIgnoreCase_WithDifferentName_ReturnsFalse()
    {
        DeploymentStatus.Failed.EqualsIgnoreCase("Deployed").Should().BeFalse();
    }

    // ---- GetDisplayStrings ---------------------------------------------------

    /// <summary>
    /// Verifies that GetDisplayStrings returns one string per enum member.
    /// </summary>
    [Fact]
    public void GetDisplayStrings_ReturnsOneStringPerEnumMember()
    {
        var displays = EnumExtensions.GetDisplayStrings<SeverityLevel>();

        displays.Should().HaveCount(Enum.GetValues(typeof(SeverityLevel)).Length);
        displays.Should().AllSatisfy(s => s.Should().NotBeNullOrEmpty());
    }
}
