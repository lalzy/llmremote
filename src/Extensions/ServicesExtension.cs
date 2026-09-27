using System.Reflection;
using Microsoft.EntityFrameworkCore;
using LLMRemote.Services;

namespace LLMRemote.Extensions;

public static class ServiceExtension{
    ///<summary>Register non-nested classes in LLMRemote.Services namespace as scoped</summary>
    ///<param name="services">The collection to register into</param>
    ///<remarks>Classes registered as themselves and not their interfaces</remarks>
    public static void AddServices(this IServiceCollection services){
        var serviceTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "LLMRemote.Services" && !t.IsNested);

        foreach (var type in serviceTypes){
            services.AddScoped(type);
        }
    }
}
