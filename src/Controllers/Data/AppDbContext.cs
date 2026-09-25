// AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using LLMRemote.Models;

namespace LLMRemote.Data;

public class AppDbContext : DbContext{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base (options){}

    public DbSet<LLMModel> LLMModel => Set<LLMModel>();
}
