// Controllers.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using LLMRemote.Data;

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
            });
        });

        return (retFactory, retFactory.CreateClient());
    }
}
