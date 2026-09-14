using BlogSystem.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using BlogSystem.Api.Models;

namespace BlogSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _repository;

    public PostsController(IPostRepository repository)
    {
        _repository = repository;
    }


    [HttpGet]
    public async Task<ActionResult<List<Post>>> GetAll()
    {
        var posts = await _repository.GetAllAsync();

        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetById(int id)
    {
        var post = await _repository.GetByIdAsync(id);

        if (post is null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Create(Post post)
    {
        await _repository.AddAsync(post);

        return CreatedAtAction(
            nameof(GetById),
            new { id = post.Id },
            post);
    }


    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Post post)
    {
        if (id != post.Id)
        {
            return BadRequest();
        }

        var existingPost = await _repository.GetByIdAsync(id);

        if (existingPost is null)
        {
            return NotFound();
        }

        existingPost.Title = post.Title;
        existingPost.Content = post.Content;
        existingPost.AuthorId = post.AuthorId;
        existingPost.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingPost);

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var post = await _repository.GetByIdAsync(id);

        if (post is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(post);

        return NoContent();
    }

    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<List<Post>>> GetByAuthor(int authorId)
    {
        var posts = await _repository.GetByAuthorIdAsync(authorId);

        return Ok(posts);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<List<Post>>> GetFiltered(
    [FromQuery] int? authorId,
    [FromQuery] string? title)
    {
        var posts = await _repository.GetFilteredAsync(authorId, title);

        return Ok(posts);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Post>>> Search([FromQuery] string? query)
    {
        Console.WriteLine($"SEARCH QUERY: '{query}'");

        var posts = await _repository.SearchAsync(query);

        return Ok(posts);
    }
}
