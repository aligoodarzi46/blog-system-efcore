using BlogSystem.Api.Data;
using BlogSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSystem.Api.Repositories;

public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<List<Post>> GetAllAsync()
    {
        return await _context.Posts.ToListAsync();
    }

    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Post post)
    {
        post.CreatedAt = DateTime.UtcNow;

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Post post)
    {
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Post>> GetByAuthorIdAsync(int authorId)
    {
        var query = _context.Posts
            .Where(p => p.AuthorId == authorId);

        return await query.ToListAsync();
    }

    public async Task<List<Post>> GetFilteredAsync(int? authorId, string? title)
    {
        var query = _context.Posts.AsQueryable();

        if (authorId.HasValue)
        {
            query = query.Where(p => p.AuthorId == authorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(p => p.Title.Contains(title));
        }

        return await query.ToListAsync();
    }


    public async Task<List<Post>> SearchAsync(string? query)
    {
        var posts = _context.Posts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            posts = posts.Where(p => p.Title.Contains(query) || p.Content.Contains(query));
        }

        return await posts.ToListAsync();
    }
}
