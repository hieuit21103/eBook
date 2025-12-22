using Domain.Entities;

namespace Domain.Interfaces;

public interface IDocumentCategoryRepository : IRepository<DocumentCategory>
{
    Task<IEnumerable<DocumentCategory>> GetByDocumentIdAsync(Guid documentId);
    Task<IEnumerable<DocumentCategory>> GetByCategoryIdAsync(Guid categoryId);
    Task DeleteByDocumentIdAsync(Guid documentId);
    Task AddRangeAsync(IEnumerable<DocumentCategory> documentCategories);
}
