# Lab 3 – UqsBlog

This repository is updated to match the Lab 3 requirements:

- All projects target .NET 10
- Test project migrated to xUnit.net v3
- 9 unit tests under:
  - `AddPostService.AddPost(authorId)`
  - `UpdateTitleService.UpdateTitle(postId, title)`
  - “anemic model” awareness rule placement

## Build and Test

- From `Domain.Tests` unzip the Archive.zip folder and pull out the bin and obj folders into the `Domain.Tests` folder.

From the `UqsBlog` folder:

> dotnet restore
> dotnet build -c Release
> dotnet test -c Release --no-build


## Scope / Constraints

- No database usage in tests - repository behavior is isolated via test doubles (fakes)
- No SpecFlow/Reqnrol
- Focus strictly on responsibilities and rules (entities/value objects, aggregates, services, repositories, statelessness, ubiquitous language)


## Where to look

- Domain services: `Uqs.Blog.Domain/Services`
- Unit tests: `Uqs.Blog.Domain.Tests`
- Deliverables: `docs/`
