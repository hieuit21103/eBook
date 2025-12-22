using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DocumentCategoryRepository : Repository<DocumentCategory>, IDocumentCategoryRepository
{
    public DocumentCategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DocumentCategory>> GetByDocumentIdAsync(Guid documentId)
    {
        return await _context.DocumentCategories
            .Include(dc => dc.Category)
            .Where(dc => dc.DocumentId == documentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentCategory>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _context.DocumentCategories
            .Include(dc => dc.Document)
            .Where(dc => dc.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task DeleteByDocumentIdAsync(Guid documentId)
    {
        var documentCategories = await _context.DocumentCategories
            .Where(dc => dc.DocumentId == documentId)
            .ToListAsync();
        
        _context.DocumentCategories.RemoveRange(documentCategories);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<DocumentCategory> documentCategories)
    {
        await _context.DocumentCategories.AddRangeAsync(documentCategories);
        await _context.SaveChangesAsync();
    }
}
