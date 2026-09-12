# Entity Framework Core

This document describes how Entity Framework Core is implemented and configured in the Blog System.

The project uses **Entity Framework Core Code First** with **SQL Server** as the database provider.

---

## 1. EF Core in This Project

Entity Framework Core is responsible for:

* Mapping domain entities to database tables
* Configuring relationships between entities
* Managing database schema changes
* Generating and applying migrations
* Translating LINQ queries into SQL
* Maintaining the connection between the application and SQL Server

The current architecture uses EF Core as the data-access technology between the application and the database.

```text
Application
     ↓
Entity Framework Core
     ↓
SQL Server
     ↓
BlogSystemDb
```

---

# 2. Code First Approach

The project follows the **Code First** approach.

Instead of creating the database schema manually first, the database model is defined in C# through entity classes and EF Core configuration.

The general workflow is:

```text
Entity Classes
      ↓
DbContext
      ↓
EF Core Model
      ↓
Migration
      ↓
SQL Server Database
```

For example, the `Post` entity contains:

```csharp
public int Id { get; set; }

public string Title { get; set; } = string.Empty;

public int AuthorId { get; set; }

public Author Author { get; set; } = null!;
```

EF Core uses this model, together with the Fluent API configuration, to build the database model.

---

# 3. Domain Entities

The current EF Core model contains five domain entities:

```text
Author
Post
Tag
PostTag
Comment
```

These entities are mapped to the following database tables:

```text
Author  → Authors
Post    → Posts
Tag     → Tags
PostTag → PostTags
Comment → Comments
```

The domain relationships are:

```text
Author 1 ──── N Post
Author 1 ──── N Comment
Post   1 ──── N Comment
Post   N ──── N Tag
```

The many-to-many relationship between `Post` and `Tag` is implemented through the `PostTag` join entity.

---

# 4. Navigation Properties

Navigation properties allow EF Core to represent relationships between entities in the object model.

For example, `Post` contains:

```csharp
public int AuthorId { get; set; }

public Author Author { get; set; } = null!;
```

`AuthorId` is the foreign-key property, while `Author` is the navigation property.

The `Post` entity also contains:

```csharp
public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

public ICollection<Comment> Comments { get; set; } = new List<Comment>();
```

These represent the relationships between a post and:

* its tags
* its comments

Similarly, `Author` contains:

```csharp
public ICollection<Post> Posts { get; set; } = new List<Post>();

public ICollection<Comment> Comments { get; set; } = new List<Comment>();
```

---

# 5. BlogDbContext

The central EF Core component in the project is:

```text
BlogDbContext
```

It inherits from:

```csharp
DbContext
```

and receives `DbContextOptions<BlogDbContext>` through dependency injection.

The constructor is:

```csharp
public BlogDbContext(DbContextOptions<BlogDbContext> options)
    : base(options)
{
}
```

The context exposes the application's entities through `DbSet` properties:

```csharp
public DbSet<Author> Authors { get; set; }

public DbSet<Post> Posts { get; set; }

public DbSet<Tag> Tags { get; set; }

public DbSet<PostTag> PostTags { get; set; }

public DbSet<Comment> Comments { get; set; }
```

These properties allow EF Core to query and persist the corresponding entities.

---

# 6. Registering DbContext

`BlogDbContext` is registered in `Program.cs` using dependency injection:

```csharp
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

This configuration tells ASP.NET Core to:

1. Register `BlogDbContext` in the dependency injection container.
2. Use SQL Server as the database provider.
3. Read the database connection string from configuration.

The connection string is stored in:

```text
appsettings.json
```

The current database configuration is:

```text
Server=localhost;
Database=BlogSystemDb;
Trusted_Connection=True;
TrustServerCertificate=True;
```

---

# 7. Fluent API

The project uses the **Fluent API** to explicitly configure relationships and keys.

The configuration is implemented inside:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
```

This allows the database model to be configured independently from conventions.

The current Fluent API configuration is responsible for:

* Composite primary keys
* PostTag relationships
* Comment relationships
* Foreign keys
* Delete behavior

---

# 8. Composite Primary Key

The `PostTag` entity represents the many-to-many relationship between posts and tags.

A single `PostTag` record is uniquely identified by both:

```text
PostId
TagId
```

The composite key is configured with:

```csharp
modelBuilder.Entity<PostTag>()
    .HasKey(pt => new { pt.PostId, pt.TagId });
```

Therefore:

```text
PostId + TagId
```

together form the primary key.

This prevents duplicate relationships such as:

```text
Post 1 → Tag 5
Post 1 → Tag 5
```

from being stored multiple times.

---

# 9. Configuring PostTag Relationships

The relationship between `PostTag` and `Post` is configured as:

```csharp
modelBuilder.Entity<PostTag>()
    .HasOne(pt => pt.Post)
    .WithMany(p => p.PostTags)
    .HasForeignKey(pt => pt.PostId);
```

This means:

```text
Post 1 ──── N PostTag
```

The relationship between `PostTag` and `Tag` is configured as:

```csharp
modelBuilder.Entity<PostTag>()
    .HasOne(pt => pt.Tag)
    .WithMany(t => t.PostTags)
    .HasForeignKey(pt => pt.TagId);
```

This means:

```text
Tag 1 ──── N PostTag
```

Together these relationships create:

```text
Post N ──── N Tag
       \   /
       PostTag
```

---

# 10. Configuring Comment Relationships

A comment belongs to one post and one author.

The Post → Comment relationship is configured with:

```csharp
modelBuilder.Entity<Comment>()
    .HasOne(c => c.Post)
    .WithMany(p => p.Comments)
    .HasForeignKey(c => c.PostId);
```

This creates:

```text
Post 1 ──── N Comment
```

The Author → Comment relationship is configured with:

```csharp
modelBuilder.Entity<Comment>()
    .HasOne(c => c.Author)
    .WithMany(a => a.Comments)
    .HasForeignKey(c => c.AuthorId)
    .OnDelete(DeleteBehavior.NoAction);
```

This creates:

```text
Author 1 ──── N Comment
```

and explicitly configures the delete behavior.

---

# 11. DeleteBehavior.NoAction

The `Author → Comment` relationship uses:

```csharp
.OnDelete(DeleteBehavior.NoAction);
```

This configuration exists because SQL Server does not allow multiple cascade paths.

Without this configuration, there would be two possible cascade paths from `Author` to `Comment`:

```text
Author
   │
   ├──> Post ───> Comment
   │
   └────────────> Comment
```

SQL Server rejects this schema with a **Multiple Cascade Paths** error.

The final configuration is:

```text
Author → Post       Cascade
Post   → Comment    Cascade
Author → Comment    NoAction
```

This allows the database schema to be created successfully while avoiding SQL Server's cascade-path restriction.

---

# 12. Entity Framework Core Conventions vs Fluent API

EF Core can automatically infer many relationships through conventions.

For example:

```csharp
public int AuthorId { get; set; }

public Author Author { get; set; } = null!;
```

provides enough information for EF Core to recognize a typical foreign-key relationship.

However, the project uses Fluent API where explicit configuration is important.

Examples include:

```text
PostTag composite key
PostTag relationships
Comment relationships
Author → Comment delete behavior
```

This makes the database model explicit and easier to reason about.

---

# 13. Migrations

EF Core migrations are used to version the database schema.

The project currently contains:

```text
20260911063544_InitialCreate
20260912052731_AddTagsCommentsAndRelationships
```

Each migration represents a change between two versions of the EF Core model.

---

# 14. InitialCreate Migration

The first migration was:

```text
20260911063544_InitialCreate
```

It created the initial database structure for:

```text
Authors
Posts
```

and configured:

```text
Author 1 ──── N Post
```

The migration was generated from the initial EF Core model.

---

# 15. AddTagsCommentsAndRelationships Migration

The second migration was:

```text
20260912052731_AddTagsCommentsAndRelationships
```

It added:

```text
Tags
Comments
PostTags
```

and configured:

* Post → Comment
* Author → Comment
* Post → PostTag
* Tag → PostTag
* Composite key for `PostTag`
* Foreign-key indexes
* Delete behaviors

This migration represents the completion of the initial domain model.

---

# 16. Migration Snapshot

EF Core maintains a model snapshot alongside migrations.

The snapshot represents EF Core's understanding of the current model.

Conceptually:

```text
Current Entity Model
        ↓
Model Snapshot
        ↓
New Model Changes
        ↓
Migration
```

When a new migration is generated, EF Core compares the current model against the previous snapshot to determine what database changes are required.

The snapshot is therefore an important part of the Code First migration system.

---

# 17. Applying Migrations

Migrations are applied to the SQL Server database using:

```bash
dotnet ef database update --project BlogSystem.Api
```

This updates:

```text
BlogSystemDb
```

to the latest migration.

EF Core also maintains:

```text
__EFMigrationsHistory
```

in the database.

This table records which migrations have already been applied.

---

# 18. Migration Workflow

The project's migration workflow is:

```text
Modify Entity / EF Configuration
             ↓
       Add Migration
             ↓
      Review Migration
             ↓
      Database Update
             ↓
        SQL Server
```

Typical commands are:

### Create migration

```bash
dotnet ef migrations add MigrationName --project BlogSystem.Api
```

### Apply migration

```bash
dotnet ef database update --project BlogSystem.Api
```

### List migrations

```bash
dotnet ef migrations list --project BlogSystem.Api
```

### List DbContexts

```bash
dotnet ef dbcontext list --project BlogSystem.Api
```

---

# 19. Model → Migration → Database

The relationship between the application's model and the database can be summarized as:

```text
C# Entity Classes
        │
        ▼
    BlogDbContext
        │
        ▼
   Fluent API Model
        │
        ▼
 EF Core Migration
        │
        ▼
    SQL Commands
        │
        ▼
   SQL Server
        │
        ▼
  BlogSystemDb
```

This is the core Code First workflow used by the project.

---

# 20. Real Project Issue: Incorrect Tag Relationship

During development, an incorrect navigation property was temporarily added to the `Tag` entity:

```text
Tag
 └── Comments
```

This was not part of the intended domain model.

Because EF Core uses entity relationships to build its model, this incorrect navigation property caused EF Core to infer an unwanted relationship and generate an incorrect database change involving `TagId` on `Comments`.

### Resolution

The incorrect `Comments` navigation property was removed from `Tag`.

The intended `Tag` entity contains only:

```csharp
public int Id { get; set; }

public string Name { get; set; } = string.Empty;

public ICollection<PostTag> PostTags { get; set; }
    = new List<PostTag>();
```

The incorrect migration was then removed and regenerated.

This demonstrated an important Code First principle:

> Changes to entity relationships directly affect the generated database model.

---

# 21. Real Project Issue: Multiple Cascade Paths

Another issue occurred when the migration was applied to SQL Server.

The initial relationship configuration created two cascade paths:

```text
Author
   │
   ├──> Post ───> Comment
   │
   └────────────> Comment
```

SQL Server rejected the migration because multiple cascade paths to the `Comments` table are not allowed.

The error was resolved by configuring:

```csharp
.OnDelete(DeleteBehavior.NoAction);
```

for the direct `Author → Comment` relationship.

The migration was then regenerated and successfully applied.

This demonstrated why database-specific behavior must be considered when configuring EF Core relationships.

---

# 22. EF Core and SQL Server

The project uses:

```text
Microsoft.EntityFrameworkCore.SqlServer
```

as its database provider.

This provider allows EF Core to translate its model and LINQ queries into SQL Server-compatible operations.

The application therefore has the following data-access stack:

```text
ASP.NET Core
      ↓
Entity Framework Core
      ↓
SQL Server Provider
      ↓
SQL Server
```

---

# 23. Current EF Core State

At the end of Sprint 1, the EF Core foundation is complete.

The project currently has:

```text
✓ Entity classes
✓ Navigation properties
✓ BlogDbContext
✓ DbSet properties
✓ SQL Server provider
✓ Fluent API configuration
✓ One-to-many relationships
✓ Many-to-many relationship
✓ Join entity
✓ Composite primary key
✓ Delete behavior configuration
✓ Initial migration
✓ Domain relationship migration
✓ Database successfully updated
✓ Migration history
```

The EF Core layer now provides the foundation for the next project phase.

---

# 24. Next Step

The next stage of the project focuses on **Data Access & Querying**.

The planned flow is:

```text
EF Core
   ↓
Repository
   ↓
LINQ
   ↓
IQueryable
   ↓
Filtering
   ↓
Searching
   ↓
Sorting
   ↓
Pagination
   ↓
API
```

The project will continue to keep the data-access layer simple and avoid unnecessary abstractions or frameworks.
