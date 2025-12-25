using Domain.Interfaces;
using Application.Services;

namespace Documents.Test.Services.Documents;

public abstract class DocumentServiceBase
{
    protected readonly IDocumentRepository _documentRepository;
    protected readonly ICategoryRepository _categoryRepository;
    protected readonly IDocumentCategoryRepository _documentCategoryRepository;
    protected readonly DocumentService _documentService;

    protected DocumentServiceBase()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _documentCategoryRepository = Substitute.For<IDocumentCategoryRepository>();
        _documentService = new DocumentService(_documentRepository, _categoryRepository, _documentCategoryRepository);
    }
}