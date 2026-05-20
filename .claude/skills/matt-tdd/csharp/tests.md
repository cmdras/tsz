# Good and Bad Tests

Framework: **xUnit**. Assertions: plain `Assert.X(...)` style.

## Good Tests

**Integration-style**: test through real interfaces, not mocks of internal parts.

```csharp
[Fact]
public async Task Given_ValidCart_When_CheckingOut_Then_ReturnsConfirmed()
{
    var cart = CreateCart();
    cart.Add(product);

    var result = await checkout.ProcessAsync(cart, paymentMethod);

    Assert.Equal("confirmed", result.Status);
}
```

Characteristics:

- Tests behaviour callers care about
- Uses public API only
- Survives internal refactors
- Describes WHAT, not HOW
- One logical assertion per test

## Bad Tests

**Implementation-detail tests**: coupled to internal structure.

```csharp
// BAD: tests implementation details
[Fact]
public async Task Given_Cart_When_CheckingOut_Then_PaymentServiceIsCalled()
{
    var payments = Substitute.For<IPaymentService>();
    var checkout = new Checkout(payments);

    await checkout.ProcessAsync(cart, payment);

    await payments.Received(1).ProcessAsync(cart.Total);
}
```

Red flags:

- Mocking internal collaborators
- Asserting on `Received(...)` call counts/order
- Reaching into `internal` members (avoid widening visibility just for tests)
- Test breaks when refactoring without behaviour change
- Verifying via `DbContext` queries instead of the service interface

```csharp
// BAD: bypasses interface to verify
[Fact]
public async Task Given_NewUserRequest_When_Creating_Then_RowExistsInDatabase()
{
    await service.CreateAsync(new() { Name = "Alice" });

    var row = await context.Users.SingleAsync(u => u.Name == "Alice");
    Assert.NotNull(row);
}

// GOOD: verifies through interface
[Fact]
public async Task Given_CreatedUser_When_RetrievingById_Then_ReturnsUser()
{
    var user = await service.CreateAsync(new() { Name = "Alice" });

    var retrieved = await service.GetByIdAsync(user.Id);
    Assert.Equal("Alice", retrieved!.Name);
}
```

## Naming

Use **Given / When / Then** — BDD mapped onto Arrange / Act / Assert. The name carries the spec; the body mirrors it section-for-section.

```
Given_<state>_When_<action>_Then_<outcome>
```

Examples:

- `Given_TwoPositiveNumbers_When_Adding_Then_ReturnsPositiveSum`
- `Given_ValidCart_When_CheckingOut_Then_ReturnsConfirmed`
- `Given_AlreadyCancelledOrder_When_Cancelling_Then_Throws`
- `Given_OverdueInvoice_When_Querying_Then_MarkedOverdue`
- `Given_CreatedUser_When_RetrievingById_Then_ReturnsUser`

Avoid `Method_State_Expected` — couples the name to the production method, not the behaviour:

- `Checkout_ValidCart_ReturnsConfirmed`
- `Cancel_AlreadyCancelled_ThrowsInvalidOperation`
- `CreateUser_HappyPath_ReturnsUser`

Rules:

- **One Given, one When, one Then** per test. If you need a second `When`, you have two tests.
- **PascalCase segments** separated by the literal `Given_` / `When_` / `Then_` markers — no extra underscores inside a segment.
- **Describe the domain action**, not the C# call: `When_Cancelling`, not `When_CallingCancelAsync`.
- **`Given_` is optional when there's no precondition** — `When_Adding_Then_ReturnsSum` is fine.
- **No `Should_` prefix, no `_Test` suffix.**
- The name doesn't save a bad body — if the `Then` asserts on `Received(1)` or a raw DB row, the test is still bad (see _Bad Tests_ above).

## Additional xUnit practices

- **AAA layout**: Arrange, Act, Assert separated by blank lines. At most one Act per test — if you have two, you have two tests.
- **Return `Task`, never `async void`** — `async void` swallows exceptions and xUnit can't observe failures.
- **`[Theory]` + `[InlineData]`** for the same behaviour over multiple inputs; one `[Fact]` per _distinct_ behaviour, not per input value.
- **Async exceptions**: `await Assert.ThrowsAsync<TException>(() => service.DoAsync())`.
- **Use the most specific assertion** — `Assert.Equal`, `Assert.NotNull`, `Assert.Contains`, `Assert.Empty`. They produce useful failure messages; `Assert.True(x == y)` produces `Expected: True, Actual: False`.
- **No control flow in tests** — no `if`/`for`/`switch`/`try`. If you need branching, you need separate tests or a `[Theory]`.
- **No shared mutable state** between tests. xUnit creates a fresh test-class instance per `[Fact]` — lean on that, don't fight it with statics.
- **Shared expensive setup**: `IClassFixture<T>` / `ICollectionFixture<T>`, not static fields.
- **Deterministic time/randomness**: inject `IClock` / `IRandom` rather than calling `DateTime.UtcNow` or `Random.Shared` inline.
- **Avoid `Thread.Sleep`** — wait on a `Task` or a signal.
- **Mirror the source tree**: `packages/api.tests/Modules/Foo/FooServiceTests.cs` for `packages/api/Modules/Foo/FooService.cs`.
