# MyMediator

A lightweight, open-source Mediator pattern implementation for .NET, inspired by MediatR.

## Usage

1. **Define Requests and Handlers**

```csharp
public class MyQuery : IRequest<MyResponse> { }
public class MyQueryHandler : IRequestHandler<MyQuery, MyResponse>
{
    public Task<MyResponse> Handle(MyQuery request, CancellationToken cancellationToken)
    {
        // Handle logic
        return Task.FromResult(new MyResponse());
    }
}
```

2. **Define Notifications and Handlers**

```csharp
public class MyNotification : INotification { }
public class MyNotificationHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken)
    {
        // Handle notification
        return Task.CompletedTask;
    }
}
```

3. **Add Pipeline Behaviors (optional)**

```csharp
public class LoggingBehavior : IPipelineBehavior<MyQuery, MyResponse>
{
    public async Task<MyResponse> Handle(MyQuery request, CancellationToken cancellationToken, Func<Task<MyResponse>> next)
    {
        // Pre-processing
        var response = await next();
        // Post-processing
        return response;
    }
}
```

4. **Register with DI**

```csharp
services.AddMediator();
services.AddTransient<IRequestHandler<MyQuery, MyResponse>, MyQueryHandler>();
services.AddTransient<INotificationHandler<MyNotification>, MyNotificationHandler>();
services.AddTransient<IPipelineBehavior<MyQuery, MyResponse>, LoggingBehavior>();
```

5. **Use the Mediator**

```csharp
var mediator = serviceProvider.GetRequiredService<Mediator>();
var result = await mediator.Send(new MyQuery());
await mediator.Publish(new MyNotification());
```

## Migrating from MediatR
- Replace `IMediator` with `Mediator`.
- Replace MediatR interfaces with `IRequest<TResponse>`, `IRequestHandler<TRequest, TResponse>`, `INotification`, `INotificationHandler<TNotification>`, and `IPipelineBehavior<TRequest, TResponse>` from `MyMediator`.
- Register handlers and behaviors with DI as shown above.
- Update usages of `.Send()` and `.Publish()` to use the new Mediator.

## License
MIT
