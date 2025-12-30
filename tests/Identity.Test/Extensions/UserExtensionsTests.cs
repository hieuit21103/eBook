using Domain.Entities;
using Domain.Enums;
using Domain.Filters;
using Extensions;
using FluentAssertions;
using Xunit;

namespace Identity.Test.Extensions;

public class UserExtensionsTests
{
    private readonly IQueryable<User> _users;

    public UserExtensionsTests()
    {
        _users = new List<User>
        {
            new User { Username = "alice", Email = "alice@example.com", Role = Role.Admin, IsActive = true },
            new User { Username = "bob", Email = "bob@test.com", Role = Role.User, IsActive = true },
            new User { Username = "charlie", Email = "charlie@example.com", Role = Role.User, IsActive = false }
        }.AsQueryable();
    }

    [Fact]
    public void ApplyFilters_WhenKeywordMatchesUsername_ShouldReturnMatchingUsers()
    {
        // Arrange
        var filter = new UserFilterParams { Keyword = "ali" };

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("alice");
    }

    [Fact]
    public void ApplyFilters_WhenKeywordMatchesEmail_ShouldReturnMatchingUsers()
    {
        // Arrange
        var filter = new UserFilterParams { Keyword = "test.com" };

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("bob");
    }

    [Fact]
    public void ApplyFilters_WhenRoleIsSpecified_ShouldFilterByRole()
    {
        // Arrange
        var filter = new UserFilterParams { Role = Role.Admin };

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("alice");
    }

    [Fact]
    public void ApplyFilters_WhenIsActiveIsSpecified_ShouldFilterByStatus()
    {
        // Arrange
        var filter = new UserFilterParams { IsActive = false };

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("charlie");
    }
    
    [Fact]
    public void ApplyFilters_WhenNoFilters_ShouldReturnAll()
    {
        // Arrange
        var filter = new UserFilterParams();

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void ApplyFilters_WhenEmailIsSpecified_ShouldFilterByEmail()
    {
        // Arrange
        var filter = new UserFilterParams { Email = "alice@example.com" };

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("alice");
    }

    [Fact]
    public void ApplyFilters_WhenUsernameIsSpecified_ShouldFilterByUsername()
    {
        // Arrange
        var filter = new UserFilterParams { Username = "bob" };

        // Act
        var result = _users.ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("bob");
    }
}
