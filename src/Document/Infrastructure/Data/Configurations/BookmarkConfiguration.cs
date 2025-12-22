using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.PageId).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.HasOne(b => b.Page)
               .WithMany()
               .HasForeignKey(b => b.PageId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}