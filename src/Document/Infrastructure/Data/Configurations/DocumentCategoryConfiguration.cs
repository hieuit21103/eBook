using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class DocumentCategoryConfiguration : IEntityTypeConfiguration<DocumentCategory>
{
    public void Configure(EntityTypeBuilder<DocumentCategory> builder)
    {
        builder.HasKey(dc => dc.Id);
        builder.Property(dc => dc.CategoryId).IsRequired();
        builder.Property(dc => dc.DocumentId).IsRequired();

        builder.HasOne(dc => dc.Document)
               .WithMany()
               .HasForeignKey(dc => dc.DocumentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dc => dc.Category)
               .WithMany()
               .HasForeignKey(dc => dc.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}