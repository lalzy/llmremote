// Controllers.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using LLMRemote.Services;
using LLMRemote.Data;
using LLMRemote.Models;
using LLMRemote.Tests.Factories;

namespace LLMRemote.Tests.Util;

public static class ControllersUtil{
    public static (WebApplicationFactory<Program>, HttpClient) Setup(WebApplicationFactory<Program> factory){
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var retFactory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
                services.RemoveAll<IManagedProcess>();
                services.AddTransient(_ => new Mock<IManagedProcess>().Object);
            });
        });
        
    // Seed the DB for tests
    using (var scope = retFactory.Services.CreateScope()){
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

        return (retFactory, retFactory.CreateClient());
    }

    /// <summary>Database wrapper</summary>
    /// <param name="factory">The mock webserver context</param>
    /// <param name="action">The DB Factory call</param>
    /// <returns>The DB Factory return</returns>
    private static T WithDB<T>(WebApplicationFactory<Program> factory, Func<AppDbContext, T> action){
        using var scope = factory.Services.CreateScope();
        return action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
    
    /// <summary>Create a procedural-filled LLMModel in the database</summary>
    /// <param name="factory">The mock webserver context</param>
    /// <returns>The LLMModel object</returns>
    public static LLMModel CreateLLMModel (WebApplicationFactory<Program> factory){
        return WithDB(factory, db => LLMModelFactory.Create(db));
    }

}
