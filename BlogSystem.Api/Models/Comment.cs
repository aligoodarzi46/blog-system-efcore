using System;

namespace BlogSystem.Api.Models;

public class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int PostId { get; set; }

    public Post Post { get; set; } = null!;

    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}

