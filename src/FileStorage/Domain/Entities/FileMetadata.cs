using FileStorage.Domain.Enums;

using System.ComponentModel.DataAnnotations;

namespace FileStorage.Domain.Entities;

public class FileMetadata
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public FileType FileType { get; set; }
    [Required]
    [MaxLength(100)]
    public string FileName { get; set; } = string.Empty;
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
