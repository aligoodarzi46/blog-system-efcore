using BlogSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSystem.Api.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors { get; set; }

    public DbSet<Post> Posts { get; set; }
}
