using BlogSystem.Api.Models;

namespace BlogSystem.Api.Repositories;

public interface IPostRepository
{
    Task<List<Post>> GetAllAsync();

    Task<Post?> GetByIdAsync(int id);

    Task AddAsync(Post post);

    Task UpdateAsync(Post post);

    Task DeleteAsync(Post post);

    Task<List<Post>> GetByAuthorIdAsync(int authorId);

    Task<List<Post>> GetFilteredAsync(int? authorId, string? title);

    Task<List<Post>> SearchAsync(string? query);
}