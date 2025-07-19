namespace MyMediator
{
    public static class MediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.AddScoped<IMediator, Mediator>();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var handlerTypes = assembly.GetTypes();

            // Register IRequestHandler implementations
            foreach (var type in handlerTypes)
            {
                var interfaces = type.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name.StartsWith("IRequestHandler"))
                    {
                        services.AddScoped(iface, type);
                    }
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name.StartsWith("INotificationHandler"))
                    {
                        services.AddScoped(iface, type);
                    }
                }
            }

            return services;
        }
    }
}
