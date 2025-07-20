using Microsoft.Extensions.DependencyInjection;

namespace MyMediator
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            // Resolve handler for the request
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);

            // Resolve pipeline behaviors
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var behaviors = (IEnumerable<object>)_serviceProvider.GetServices(behaviorType);

            // Build pipeline chain
            Func<Task<TResponse>> handlerDelegate = () => handler.Handle((dynamic)request, cancellationToken);
            foreach (var behavior in ((IEnumerable<object>)behaviors).Reverse())
            {
                var next = handlerDelegate;
                handlerDelegate = () => ((dynamic)behavior).Handle((dynamic)request, cancellationToken, next);
            }
            return await handlerDelegate();
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
            foreach (var handler in handlers)
            {
                await handler.Handle(notification, cancellationToken);
            }
        }
    }
}