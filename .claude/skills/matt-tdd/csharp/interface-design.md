# Interface Design for Testability

Good interfaces make testing natural:

1. **Accept dependencies via the constructor, don't `new` them inside**

   ```csharp
   // Testable
   public class OrderService(IPaymentGateway gateway) { /* ... */ }

   // Hard to test
   public class OrderService
   {
       private readonly StripeGateway _gateway = new();
   }
   ```

   Constructor injection is the unit of composability in .NET. Every collaborator that crosses a seam should arrive through the constructor, typed against an interface. Production wires it via `IServiceCollection`; tests pass a `Substitute.For<...>()` directly — no DI container required.

2. **Return results, don't mutate hidden state**

   ```csharp
   // Testable
   public Discount Calculate(Cart cart) { /* ... */ }

   // Hard to test
   public void ApplyDiscount(Cart cart) => cart.Total -= /* ... */;
   ```

3. **Small surface area**
   - Fewer public methods = fewer tests needed
   - Fewer params = simpler test setup
   - One interface per collaborator role (Interface Segregation) — avoid god-interfaces that force tests to stub members they don't care about
