using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MyMediator
{
    public static class MediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            // Register IMediator
            services.AddScoped<IMediator, Mediator>();

            // By default, scan all currently loaded assemblies so handlers from the web app are included
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            RegisterHandlers(services, assemblies);

            return services;
        }

        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddScoped<IMediator, Mediator>();
            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            RegisterHandlers(services, assemblies);
            return services;
        }

        public static IServiceCollection AddMediatorFromAssemblyContaining<T>(this IServiceCollection services)
        {
            return services.AddMediator(typeof(T).Assembly);
        }

        private static void RegisterHandlers(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray()!;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface)
                        continue;

                    foreach (var iface in type.GetInterfaces())
                    {
                        if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name.StartsWith("IRequestHandler"))
                        {
                            services.AddScoped(iface, type);
                        }
                        else if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name.StartsWith("INotificationHandler"))
                        {
                            services.AddScoped(iface, type);
                        }
                        else if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name.StartsWith("IPipelineBehavior"))
                        {
                            services.AddScoped(iface, type);
                        }
                    }
                }
            }
        }
    }
}