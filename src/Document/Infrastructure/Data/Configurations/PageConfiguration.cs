using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(dp => dp.Id);
        builder.Property(dp => dp.PageNumber).IsRequired();
        builder.Property(dp => dp.DocumentId).IsRequired();

        builder.HasOne(dp => dp.Document)
               .WithMany(d => d.Pages)
               .HasForeignKey(dp => dp.DocumentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}