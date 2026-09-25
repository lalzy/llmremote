using Microsoft.EntityFrameworkCore;
using LLMRemote.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data source=models.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo{
            Title = "LLama Remote Service",
            Version = "v1"
        });
});


builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages(options => {options.RootDirectory = "/src/views";});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
