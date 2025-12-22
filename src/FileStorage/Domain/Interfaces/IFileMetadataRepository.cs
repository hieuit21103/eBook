using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;

namespace FileStorage.Domain.Interfaces;

public interface IFileMetadataRepository
{
    Task<FileMetadata?> GetByIdAsync(Guid id);
    Task<FileMetadata?> GetByFilePathAsync(string filePath);
    Task<IEnumerable<FileMetadata>> GetByFileTypeAsync(FileType fileType);
    Task<IEnumerable<FileMetadata>> GetAllAsync();
    Task<FileMetadata> CreateAsync(FileMetadata fileMetadata);
    Task<FileMetadata> UpdateAsync(FileMetadata fileMetadata);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
