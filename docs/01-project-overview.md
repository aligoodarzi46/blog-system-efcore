# Blog System with EF Core

## Overview

Blog System is a RESTful Web API project built with ASP.NET Core and Entity Framework Core.

The project is designed as a practical learning project to develop a deeper understanding of Entity Framework Core, relational database design, LINQ, and data access patterns.

The project focuses on building a realistic blog backend rather than implementing isolated examples.

## Goals

The main goals of this project are:

* Build a RESTful Blog API
* Gain deeper practical experience with Entity Framework Core
* Understand Code First development
* Work with EF Core migrations
* Design and implement relational database relationships
* Practice LINQ to Entities queries
* Implement search, sorting, and pagination
* Learn the Repository Pattern
* Practice manual DTO mapping
* Work with SQL Server
* Improve Git and GitHub workflow
* Produce clear technical documentation

## Main Features

The system will support:

* Posts CRUD
* Authors CRUD
* Tags management
* Many-to-many relationship between Posts and Tags
* Comments for Posts
* One-to-many relationship between Posts and Comments
* Search Posts by title and content
* Sorting Posts by date and author
* Pagination
* Initial data seeding

## Technology Stack

* C#
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* LINQ
* Swagger / OpenAPI
* Git
* GitHub

## Architecture

The project will follow a simple layered approach:

```text
Client
  ↓
Controller
  ↓
Repository
  ↓
Entity Framework Core
  ↓
SQL Server
```

DTOs will be used at the API boundary, with manual mapping between DTOs and entities.

The architecture is intentionally kept simple because the primary goal of this project is learning and understanding the data-access layer.

## Core Domain

The initial domain consists of four main entities:

* Author
* Post
* Tag
* Comment

Relationships:

```text
Author 1 ──────── * Post

Post   1 ──────── * Comment

Post   * ──────── * Tag
```

The many-to-many relationship between `Post` and `Tag` will be represented through a join table named `PostTag`.

## Learning Focus

The most important learning areas of this project are:

1. EF Core Code First
2. Migrations
3. Database Relationships
4. LINQ to Entities
5. `IQueryable`
6. Repository Pattern
7. DTOs and manual mapping
8. SQL Server
9. Data Seeding

A particular focus will be placed on understanding the flow:

```text
LINQ
  ↓
IQueryable
  ↓
EF Core
  ↓
SQL
  ↓
SQL Server
```

## Project Documentation

Detailed documentation will be developed throughout the project.

Documentation areas include:

* Architecture
* Database Design
* Entity Framework Core
* API
* Testing
* Learning Notes

The documentation will evolve alongside the implementation instead of being written entirely at the beginning.

## Project Status

**Status:** In Development

**Level:** Junior+

**Planned Duration:** 3 weeks

**Target Repository:** `blog-system-efcore`
