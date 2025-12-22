using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Data.Configurations;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Bookmark> Bookmarks { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Page> Pages { get; set; } = null!;
    public DbSet<DocumentCategory> DocumentCategories { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new PageConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new BookmarkConfiguration());
    }
}