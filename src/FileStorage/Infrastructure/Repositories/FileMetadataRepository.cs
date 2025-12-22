using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;
using FileStorage.Domain.Interfaces;
using FileStorage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Repositories;

public class FileMetadataRepository : IFileMetadataRepository
{
    private readonly FileStorageDbContext _context;

    public FileMetadataRepository(FileStorageDbContext context)
    {
        _context = context;
    }

    public async Task<FileMetadata?> GetByIdAsync(Guid id)
    {
        return await _context.FileMetadata.FindAsync(id);
    }

    public async Task<FileMetadata?> GetByFilePathAsync(string filePath)
    {
        return await _context.FileMetadata
            .FirstOrDefaultAsync(f => f.FilePath == filePath);
    }

    public async Task<IEnumerable<FileMetadata>> GetByFileTypeAsync(FileType fileType)
    {
        return await _context.FileMetadata
            .Where(f => f.FileType == fileType)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FileMetadata>> GetAllAsync()
    {
        return await _context.FileMetadata
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<FileMetadata> CreateAsync(FileMetadata fileMetadata)
    {
        _context.FileMetadata.Add(fileMetadata);
        await _context.SaveChangesAsync();
        return fileMetadata;
    }

    public async Task<FileMetadata> UpdateAsync(FileMetadata fileMetadata)
    {
        _context.FileMetadata.Update(fileMetadata);
        await _context.SaveChangesAsync();
        return fileMetadata;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var fileMetadata = await _context.FileMetadata.FindAsync(id);
        if (fileMetadata == null)
            return false;

        _context.FileMetadata.Remove(fileMetadata);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.FileMetadata.AnyAsync(f => f.Id == id);
    }
}
