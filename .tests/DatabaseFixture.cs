// DatabaseFixture.cs

using LLMRemote.Models;
using LLMRemote.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class DatabaseFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public DbContextOptions<AppDbContext> Options { get; }
    
    public DatabaseFixture(){
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .Options;

        using var db = new AppDbContext(Options);
        db.Database.EnsureCreated();
        db.SaveChanges();
    }

    public void Reset(){
        using var db = CreateContext();
        foreach (var entity in db.Model.GetEntityTypes()){
            // Table names come from the model but C# complains unecessarily.
            #pragma warning disable EF1002
            db.Database.ExecuteSqlRaw($"DELETE FROM \"{entity.GetTableName()}\"");
            #pragma warning restore EF1002
        }
    }
    
    public AppDbContext CreateContext() => new AppDbContext(Options);

    public void Dispose() => Connection.Dispose();
}
