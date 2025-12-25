using System.Linq.Expressions;

namespace Documents.Test.Services.Categories;

public class CreateAsyncTests : CategoryServiceBase
{
    [Fact]
    public async Task CreateAsync_ShouldCreateCategory_WhenValidInput()
    {
        // Arrange
        var request = new CategoryCreateRequest { Name = "Science" };

        var savedCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(new List<Category>());
        _categoryRepository.AddAsync(Arg.Any<Category>()).Returns(savedCategory);

        // Act
        var result = await _categoryService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Science");
        await _categoryRepository.Received(1).AddAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenCategoryNameExists()
    {
        // Arrange
        var request = new CategoryCreateRequest { Name = "Science" };

        var existingCategory = new Category { Id = Guid.NewGuid(), Name = "Science" };

        _categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(new List<Category> { existingCategory });

        // Act
        Func<Task> act = async () => await _categoryService.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category with name 'Science' already exists.");
    }
}
