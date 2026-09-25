using System.Reflection;
using Microsoft.EntityFrameworkCore;
using LLMRemote.Services;

namespace LLMRemote.Extensions;

public static class ServiceExtension{
    public static void AddServices(this IServiceCollection services){
        var serviceTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "LLMRemote.Services"
                   && !t.IsSubclassOf(typeof(DbContext))
                   && !t.IsNested);

        foreach (var type in serviceTypes){
            services.AddScoped(type);
        }
    }
}
