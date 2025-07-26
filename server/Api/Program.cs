using Api.Etc;
using Api.Services;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class Program
{
    public static void Main(String[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Entity Framework
        var connectionString = builder.Configuration.GetConnectionString("AppDb");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );

        // Repositories
        builder.Services.AddScoped<IRepository<User>, UserRepository>();
        builder.Services.AddScoped<IRepository<Post>, PostRepository>();
        builder.Services.AddScoped<IRepository<Comment>, CommentRepository>();

        // Services
        builder.Services.AddScoped<IBlogService, BlogService>();
        builder.Services.AddScoped<IDraftService, DraftService>();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Setup database
        if (args is [.., "setup", var defaultPassword])
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                new DbSeeder(dbContext, defaultPassword).SetupAsync().Wait();
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
