# Database Design

This document describes the database design of the Blog System, including its tables, keys, relationships, indexes, and delete behaviors.

The database is implemented using **SQL Server** and managed through **Entity Framework Core Code First**.

---

## 1. Database Overview

The Blog System uses a relational database to manage:

* Authors
* Blog Posts
* Tags
* Comments
* Post–Tag relationships

The current database is:

```text
Database: BlogSystemDb
Database Engine: SQL Server
```

The database schema is designed around the following core relationships:

```text
Author
  │
  ├── 1:N ──> Post
  │             │
  │             ├── 1:N ──> Comment
  │             │
  │             └── N:N ──> Tag
  │                       │
  │                       └── PostTag
  │
  └── 1:N ──> Comment
```

---

# 2. Database Tables

The database currently contains five application tables:

| Table      | Purpose                                                   |
| ---------- | --------------------------------------------------------- |
| `Authors`  | Stores blog authors                                       |
| `Posts`    | Stores blog posts                                         |
| `Tags`     | Stores post tags                                          |
| `Comments` | Stores comments written on posts                          |
| `PostTags` | Junction table for the Post–Tag many-to-many relationship |

EF Core also creates:

```text
__EFMigrationsHistory
```

This table is maintained by EF Core and stores the migrations that have already been applied to the database.

---

# 3. Authors

The `Authors` table stores information about blog authors.

| Column      | Type      | Key / Constraint |
| ----------- | --------- | ---------------- |
| `Id`        | int       | Primary Key      |
| `Name`      | nvarchar  | Required         |
| `Email`     | nvarchar  | Required         |
| `CreatedAt` | datetime2 | Required         |

### Primary Key

```text
Authors.Id
```

Each author is uniquely identified by `Id`.

### Relationships

An author can have:

* Many posts
* Many comments

```text
Author 1 ──── N Post
Author 1 ──── N Comment
```

---

# 4. Posts

The `Posts` table stores blog posts.

| Column      | Type      | Key / Constraint |
| ----------- | --------- | ---------------- |
| `Id`        | int       | Primary Key      |
| `Title`     | nvarchar  | Required         |
| `Content`   | nvarchar  | Required         |
| `CreatedAt` | datetime2 | Required         |
| `UpdatedAt` | datetime2 | Nullable         |
| `AuthorId`  | int       | Foreign Key      |

### Primary Key

```text
Posts.Id
```

### Foreign Key

```text
Posts.AuthorId → Authors.Id
```

This represents the relationship:

```text
Author 1 ──── N Post
```

An author can create multiple posts, while each post belongs to one author.

---

# 5. Tags

The `Tags` table stores reusable tags that can be assigned to posts.

| Column | Type     | Key / Constraint |
| ------ | -------- | ---------------- |
| `Id`   | int      | Primary Key      |
| `Name` | nvarchar | Required         |

### Primary Key

```text
Tags.Id
```

Tags are connected to posts through the `PostTags` junction table.

---

# 6. Comments

The `Comments` table stores comments made on blog posts.

| Column      | Type      | Key / Constraint |
| ----------- | --------- | ---------------- |
| `Id`        | int       | Primary Key      |
| `Content`   | nvarchar  | Required         |
| `CreatedAt` | datetime2 | Required         |
| `PostId`    | int       | Foreign Key      |
| `AuthorId`  | int       | Foreign Key      |

### Foreign Keys

```text
Comments.PostId   → Posts.Id
Comments.AuthorId → Authors.Id
```

Therefore, a comment:

* belongs to one post
* belongs to one author

The relationships are:

```text
Post   1 ──── N Comment
Author 1 ──── N Comment
```

---

# 7. PostTags

`PostTags` is the junction table used to implement the many-to-many relationship between posts and tags.

| Column   | Type | Key / Constraint                    |
| -------- | ---- | ----------------------------------- |
| `PostId` | int  | Composite Primary Key + Foreign Key |
| `TagId`  | int  | Composite Primary Key + Foreign Key |

### Composite Primary Key

The primary key is:

```text
(PostId, TagId)
```

This ensures that the same tag cannot be associated with the same post more than once.

### Foreign Keys

```text
PostTags.PostId → Posts.Id
PostTags.TagId  → Tags.Id
```

The resulting relationship is:

```text
Post N ──── N Tag
      \     /
       PostTags
```

This is the project's explicit implementation of a many-to-many relationship using a join entity.

---

# 8. Relationships

## 8.1 Author → Posts

```text
Author 1 ──── N Post
```

Foreign key:

```text
Posts.AuthorId
```

One author can have multiple posts.

---

## 8.2 Author → Comments

```text
Author 1 ──── N Comment
```

Foreign key:

```text
Comments.AuthorId
```

One author can create multiple comments.

---

## 8.3 Post → Comments

```text
Post 1 ──── N Comment
```

Foreign key:

```text
Comments.PostId
```

One post can contain multiple comments.

---

## 8.4 Post ↔ Tag

```text
Post N ──── N Tag
```

The relationship is implemented through:

```text
PostTags
```

with:

```text
PostTags.PostId → Posts.Id
PostTags.TagId  → Tags.Id
```

This avoids storing multiple tag IDs directly inside the `Posts` table and follows the relational database model.

---

# 9. Primary Keys

The database uses the following primary keys:

```text
Authors.Id
Posts.Id
Tags.Id
Comments.Id
PostTags.(PostId, TagId)
```

The `PostTags` table is different because it uses a **composite primary key** instead of a single-column identity key.

---

# 10. Foreign Keys

The current foreign-key structure is:

```text
Posts.AuthorId
    ↓
Authors.Id
```

```text
Comments.PostId
    ↓
Posts.Id
```

```text
Comments.AuthorId
    ↓
Authors.Id
```

```text
PostTags.PostId
    ↓
Posts.Id
```

```text
PostTags.TagId
    ↓
Tags.Id
```

---

# 11. Indexes

SQL Server automatically creates indexes associated with primary keys.

EF Core also creates indexes for the foreign keys where required by the relational model.

The current database includes indexes such as:

```text
IX_Comments_AuthorId
IX_Comments_PostId
IX_PostTags_TagId
```

These indexes improve lookup and join performance when querying related records.

For example:

```text
Comments.PostId
```

can be efficiently used when retrieving all comments belonging to a specific post.

---

# 12. Delete Behavior

Delete behavior is particularly important in this database because several entities are connected through foreign keys.

The intended cascade paths are:

```text
Author
   │
   └── Cascade ──> Post
                      │
                      └── Cascade ──> Comment
```

For the direct relationship between `Author` and `Comment`, delete behavior is configured as:

```text
Author ── NoAction ──> Comment
```

Therefore:

| Relationship     | Delete Behavior |
| ---------------- | --------------- |
| Author → Post    | Cascade         |
| Post → Comment   | Cascade         |
| Author → Comment | NoAction        |
| Post → PostTag   | Cascade         |
| Tag → PostTag    | Cascade         |

---

# 13. Multiple Cascade Paths

During the database migration process, SQL Server reported a **Multiple Cascade Paths** error.

The problem was caused by two possible cascade paths from `Authors` to `Comments`:

```text
Authors
   │
   ├──> Posts ───> Comments
   │
   └────────────> Comments
```

If both relationships used cascade delete, SQL Server would have multiple cascade paths leading to the same table.

SQL Server does not allow this configuration.

### Solution

The direct:

```text
Author → Comment
```

relationship was changed to:

```text
DeleteBehavior.NoAction
```

while keeping:

```text
Author → Post     = Cascade
Post → Comment    = Cascade
```

This produces a valid SQL Server schema while preserving the intended ownership behavior.

---

# 14. Referential Integrity

Foreign keys enforce referential integrity between related records.

For example, a comment cannot reference a non-existing post:

```text
Comments.PostId → Posts.Id
```

Likewise, a `PostTags` record must reference existing records in both:

```text
Posts
Tags
```

This prevents orphaned relationships at the database level.

---

# 15. Database Schema

The current schema can be summarized as:

```text
┌──────────────┐
│   Authors    │
├──────────────┤
│ Id PK        │
│ Name         │
│ Email        │
│ CreatedAt    │
└──────┬───────┘
       │
       │ 1:N
       ▼
┌──────────────┐
│    Posts     │
├──────────────┤
│ Id PK        │
│ Title        │
│ Content      │
│ CreatedAt    │
│ UpdatedAt    │
│ AuthorId FK  │
└───┬──────┬───┘
    │      │
    │      │ 1:N
    │      ▼
    │  ┌──────────────┐
    │  │   Comments   │
    │  ├──────────────┤
    │  │ Id PK        │
    │  │ Content      │
    │  │ CreatedAt    │
    │  │ PostId FK    │
    │  │ AuthorId FK  │
    │  └──────────────┘
    │
    │ 1:N
    ▼
┌──────────────┐
│   PostTags   │
├──────────────┤
│ PostId PK/FK │
│ TagId  PK/FK │
└──────┬───────┘
       │
       │ N:1
       ▼
┌──────────────┐
│     Tags     │
├──────────────┤
│ Id PK        │
│ Name         │
└──────────────┘
```

---

# 16. Migrations and Database History

The database schema is managed through Entity Framework Core migrations.

The project currently contains:

```text
20260911063544_InitialCreate
20260912052731_AddTagsCommentsAndRelationships
```

### InitialCreate

Created the initial:

```text
Authors
Posts
```

schema and the relationship:

```text
Author 1 ──── N Post
```

### AddTagsCommentsAndRelationships

Added:

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
* Composite key for PostTag
* Foreign-key indexes
* Delete behaviors

The migrations are applied to the database through:

```bash
dotnet ef database update --project BlogSystem.Api
```

---

# 17. Current Database State

The current database contains:

```text
Authors
Posts
Tags
Comments
PostTags
__EFMigrationsHistory
```

The latest migration has been successfully applied.

The database therefore represents the completed **Sprint 1 domain model and EF Core foundation**.

---

# 18. Design Summary

The database design follows a normalized relational structure:

* Each major domain concept has its own table.
* Primary keys uniquely identify records.
* Foreign keys enforce relationships.
* The Post–Tag many-to-many relationship uses a dedicated junction entity.
* `PostTags` uses a composite primary key.
* Foreign-key indexes support relational queries.
* Delete behaviors are explicitly configured where SQL Server requires special handling.
* EF Core migrations are used to version and apply schema changes.

This schema provides the foundation for the next stage of the project:

```text
Database
    ↓
EF Core
    ↓
Repository
    ↓
LINQ / IQueryable
    ↓
Filtering / Searching / Sorting / Pagination
    ↓
API
```
