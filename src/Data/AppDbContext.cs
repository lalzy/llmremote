// AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using LLMRemote.Models;

namespace LLMRemote.Data;

///<summary>EF Core Database Context</summary>
public class AppDbContext : DbContext{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base (options){}

    public DbSet<LLMModel> LLMModel => Set<LLMModel>();

    ///<summary>Configures LLMModel</summary>
    ///<param name="modelBuilder">Builder to configure the model</param>
    ///<remarks>Ignores changes to CreatedAt on Update</remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<LLMModel>()
            .Property(m => m.CreatedAt)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        base.OnModelCreating(modelBuilder);
    }
}
