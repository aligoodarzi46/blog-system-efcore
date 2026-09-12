namespace BlogSystem.Api.Models;

public class Post
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;

    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
