using MCOP.Utils.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace MCOP.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection serviceCollection, Assembly? assembly = null)
    {
        IEnumerable<Type> services = GetClassesByType(typeof(ISharedService), assembly)
            .Except(serviceCollection.Select(s => s.ServiceType));

        foreach (Type service in services)
        {
            serviceCollection.AddSingleton(service);
            Log.Debug("Added service: {Service}", service.FullName);
        }

        return serviceCollection;
    }

    public static IServiceCollection AddScopedClasses(this IServiceCollection serviceCollection, Assembly? assembly = null)
    {
        IEnumerable<Type> services = GetClassesByType(typeof(IScoped), assembly)
            .Except(serviceCollection.Select(s => s.ServiceType));

        foreach (Type service in services)
        {
            serviceCollection.AddTransient(service);
            Log.Debug("Added service: {Service}", service.FullName);
        }

        return serviceCollection;

    }

    private static IEnumerable<Type> GetClassesByType(Type type, Assembly? assembly = null)
    {
        List<Type> types = new List<Type>();
        foreach (var assemblyItem in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        {
            if (assemblyItem.Name == "MCOP.Core")
            {
                Assembly assemblyLoad = Assembly.Load(assemblyItem.Name);
                types.AddRange(assemblyLoad.GetTypes());
            }
        }
        return types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
    }
}
