using System.Reflection;

namespace LLMRemote.Util;

public static class ModelConverter{
    ///<summary>Load up models automatically</summary>
    ///<returns>A new <typeparamref name="T"/>with mapped properties</returns>
    public static T ConvertModelToDTO<T>(this object source){
        var target = Activator.CreateInstance<T>();
        var sourceProps = source.GetType().GetProperties();
        var targetProps = typeof(T).GetProperties();

        foreach (var tp in targetProps){
            // ensure collections get initialized correctly.
            if (tp.PropertyType.IsGenericType && tp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                tp.SetValue(target, Activator.CreateInstance(tp.PropertyType));
          
            var sp = sourceProps.FirstOrDefault(p => p.Name == tp.Name && p.PropertyType == tp.PropertyType);
            if (sp != null && tp.CanWrite)
                tp.SetValue(target, sp.GetValue(source));
        }
        return target;
    }
}
