#nullable enable
using CoolifyCli.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class ConstantsTests
{
    [Fact]
    public void ApiConstants_HaveExpectedEndpoints()
    {
        Constants.Api.ApplicationsEndpoint.Should().Be("/api/v1/applications");
        Constants.Api.DatabasesEndpoint.Should().Be("/api/v1/databases");
    }
}
