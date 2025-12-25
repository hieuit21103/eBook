using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Documents;

public class GetAllAsyncTests() : DocumentServiceBase
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnDocuments_WhenDocumentsExist()
    {
        // Arrange
        var filterParams = new DocumentFilterParams { PageNumber = 1, PageSize = 10 };

        var documents = new PagedResult<Document>
        {
            Items = new List<Document> {
                new Document { Id = Guid.NewGuid(), Title = "Doc A" },
                new Document { Id = Guid.NewGuid(), Title = "Doc B" }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        var expectedResult = new PagedResult<DocumentResponse>
        {
            Items = new List<DocumentResponse> {
                new DocumentResponse { Id = documents.Items.ElementAt(0).Id, Title = "Doc A" },
                new DocumentResponse { Id = documents.Items.ElementAt(1).Id, Title = "Doc B" }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _documentRepository.GetPagedAsync(Arg.Is<DocumentFilterParams>(x =>
                x.PageNumber == filterParams.PageNumber &&
                x.PageSize == filterParams.PageSize
            ))
            .Returns(documents);

        // Act
        var result = await _documentService.GetAllAsync(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
    }
}