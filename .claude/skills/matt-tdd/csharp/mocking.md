# When to Mock

Mocking library: **NSubstitute**.

Mock at **system boundaries** only:

- External APIs (payment, email, etc.)
- Databases (sometimes — prefer EF `UseInMemoryDatabase` or a real SQLite test DB)
- Time / randomness (`IClock`, `IRandom`)
- File system (sometimes)

Don't mock:

- Your own services / classes
- Internal collaborators
- Anything you control

## Designing for mockability

### 1. Constructor injection at the class level

Inject every cross-boundary collaborator through the **class constructor**, typed against an interface. This is THE seam for unit-testable services in .NET. Production wires it up via `IServiceCollection`; tests construct the class directly with substitutes — no service provider, no DI container in tests.

```csharp
// Easy to mock
public class OrderService(IPaymentClient payments, IClock clock)
{
    public Task<Receipt> ProcessAsync(Order order)
        => payments.ChargeAsync(order.Total, clock.UtcNow);
}

// Hard to mock — constructs its own dependency
public class OrderService
{
    public Task<Receipt> ProcessAsync(Order order)
    {
        var client = new StripeClient(Environment.GetEnvironmentVariable("STRIPE_KEY")!);
        return client.ChargeAsync(order.Total, DateTime.UtcNow);
    }
}
```

Rules of thumb:

- **No `new` on a collaborator inside a class.** If the class can't function without it, inject it.
- **No service locator / `IServiceProvider`** as a parameter — that hides what the class actually needs.
- **Avoid statics** for anything time-, environment-, or I/O-bound. Wrap them in an interface.

### 2. Substitute interfaces, not concrete classes

NSubstitute can stub virtual members on a class, but doing so couples tests to the class's inheritance. Define an interface for every cross-boundary collaborator and substitute that:

```csharp
var payments = Substitute.For<IPaymentClient>();
payments.ChargeAsync(Arg.Any<decimal>(), Arg.Any<DateTime>())
        .Returns(new Receipt("ok"));

var service = new OrderService(payments, Substitute.For<IClock>());
```

### 3. Prefer SDK-style interfaces over generic clients

Specific methods per operation, not one generic `SendAsync`:

```csharp
// GOOD — each method is independently stubbable
public interface IBillingApi
{
    Task<User> GetUserAsync(int id);
    Task<Order[]> GetOrdersAsync(int userId);
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
}

// BAD — stubs need conditional logic on endpoint/payload
public interface IBillingApi
{
    Task<T> SendAsync<T>(string endpoint, object? body = null);
}
```

The SDK approach means:

- Each substitute returns one specific shape
- No conditional logic in test setup
- Easier to see which endpoints a test exercises
- Type safety per endpoint
