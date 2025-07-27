using DataAccess;
using DataAccess.Entities;

namespace Api.Etc;

public class DbSeeder(AppDbContext context)
{
    public async Task SetupAsync(String defaultPassword)
    {
        context.Database.EnsureCreated();
        if (!context.Users.Any())
        {
            await CreateRoles(Role.Admin, Role.Editor, Role.Reader);
            await CreateUsers(
                [
                    (email: "admin@example.com", role: Role.Admin),
                    (email: "editor@example.com", role: Role.Editor),
                    (email: "othereditor@example.com", role: Role.Editor),
                    (email: "reader@example.com", role: Role.Reader),
                ],
                defaultPassword
            );
            await context.SaveChangesAsync();
        }

        if (!context.Posts.Any(p => p.PublishedAt != null))
        {
            var admin = context.Users.Single((user) => user.Email == "admin@example.com");
            context.Posts.Add(
                new Post
                {
                    Title = "First post",
                    Content =
                        @"## Hello Python
Have you ever wondered how to make a hello-world application in Python?

The answer is simply:
```py
print('Hello World!')
```
                    ",
                    AuthorId = admin!.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PublishedAt = DateTime.UtcNow,
                }
            );
        }
        if (!context.Posts.Any(p => p.PublishedAt == null))
        {
            var editor = context.Users.Single((user) => user.Email == "editor@example.com");
            context.Posts.Add(
                new Post
                {
                    Title = "Draft",
                    Content = "This is a draft post",
                    AuthorId = editor!.Id,
                    CreatedAt = DateTime.UtcNow,
                    PublishedAt = null,
                }
            );
        }
        await context.SaveChangesAsync();

        if (!context.Comments.Any())
        {
            var reader = context.Users.Single((user) => user.Email == "reader@example.com");
            context.Comments.Add(
                new Comment
                {
                    Content = "First one to comment",
                    AuthorId = reader.Id,
                    PostId = context.Posts.First().Id,
                }
            );
            await context.SaveChangesAsync();
        }
    }

# pragma warning disable CS1998
    private async Task CreateRoles(string admin, string editor, string reader)
    {
        // TODO implement when adding Identity
    }

# pragma warning disable CS1998
    private async Task CreateUsers((string email, string role)[] users, string defaultPassword)
    {
        foreach (var user in users)
        {
            context.Users.Add(
                new User
                {
                    UserName = user.email.Split("@")[0],
                    Email = user.email,
                    EmailConfirmed = true,
                    Role = user.role,
                }
            );
        }
        await context.SaveChangesAsync();
    }
}
