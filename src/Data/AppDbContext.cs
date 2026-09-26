// AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using LLMRemote.Models;

namespace LLMRemote.Data;

public class AppDbContext : DbContext{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base (options){}

    public DbSet<LLMModel> LLMModel => Set<LLMModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<LLMModel>()
            .Property(m => m.CreatedAt)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        base.OnModelCreating(modelBuilder);
    }
}
