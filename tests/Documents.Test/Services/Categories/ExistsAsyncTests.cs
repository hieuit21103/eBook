using System.Linq.Expressions;

namespace Documents.Test.Services.Categories;
    
public class ExistsAsyncTests : CategoryServiceBase
{
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);

        // Act
        var result = await _categoryService.ExistsAsync(categoryId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(false);

        // Act
        var result = await _categoryService.ExistsAsync(categoryId);

        // Assert
        result.Should().BeFalse();
    }
}
