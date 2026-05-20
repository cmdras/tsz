# PLAN: Test restructure — business-intent naming + integration coverage

Source: feature.md

## Goal
Restructure the test suite so it reads as business intent, drop the duplicated
service-layer suites left over from the repository refactor, and fill the
integration gaps that let real domain rules go unverified at the HTTP boundary.

## Approach

**Collapse.** Delete `CustomerServiceTests`, `ContractServiceTests`,
`UserServiceTests`, `LeaveTypeServiceTests`, and the nested
`CustomerResponseTests` mapper test inside `CustomerRepositoryTests.cs`. Keep
`UserLeaveAllowanceServiceTests` — the service owns real logic.

**Rename.** Every surviving test class becomes `…Should`; every method becomes
a verb-first phrase (`Reject_Duplicate_Email_On_Create`, `Find_Customer_By_Id`).
Applies to repository tests, the kept service test, `ValidationFilter`, and
all endpoint test classes.

**Extract `IntegrationFactory`.** Replaces `TestApiFactory` and every per-module
`*ApiFactory`. One shared `WebApplicationFactory<Program>` in
`api.tests.integration/Common/IntegrationFactory.cs` that:
- configures Test auth + in-memory `AppDbContext`,
- exposes `HttpClient Client` and static `JsonSerializerOptions JsonOptions`,
- implements `IAsyncLifetime.InitializeAsync` to wipe every table.

Each endpoint test class becomes ~3 lines of setup
(`IClassFixture<IntegrationFactory>`, constructor stores the factory).

**Extract `CustomerBuilder`.** `Common/Builders/CustomerBuilder.cs` with
defaults that satisfy domain rules; fluent opt-ins
(`Named("Globex").Archived().Build()`). Add named fixtures `Customer.Globex`
and `Customer.Acme` as static instances. Used by `CustomerEndpointsShould`;
do not expand to other entities in this slice.

**Fill coverage** (after structural work, all integration tests):
- *Contracts*: `Reject_Contract_When_Customer_Is_Archived`,
  `Reject_Contract_When_Consultant_Is_Archived`,
  `Reject_Contract_When_Consultant_Is_ClientManager` (×2 for create + update),
  `Persist_Task_Add_Remove_Reorder_On_Update`,
  `Return_Tasks_Ordered_By_Order_Field`.
- *Users*: `Reject_Unknown_LeaveType_On_Update` (**fix the underlying 5xx bug
  in `UserService`/`UserRepository` update path so it returns 422**),
  `Persist_New_Allowance_When_LeaveType_Not_Yet_On_User`,
  `Reject_Duplicate_Email_Different_Case_On_Create`,
  `Reject_Duplicate_Email_Different_Case_On_Update`.
- *OAuth provisioning*: resolve `IUserService` from the integration factory's
  service provider and call `GetOrProvisionAsync` directly (no HTTP endpoint
  exists). Three tests: `Create_User_With_Default_Allowances_On_First_Login`,
  `Return_Existing_User_On_Repeated_Provision`,
  `Match_Existing_User_By_Email_Case_Insensitively`.
- *Error contract*: on one 409 (duplicate email) and one 422 (validation),
  assert `type`, `title`, `status`, `detail` on the response body.
- *Query-string roundtrip*: one `RoundTrip_Sort_And_Pagination_Query_String`
  test per paged endpoint — Customers, Contracts, Users, LeaveTypes. Seed
  enough rows that page size and sort direction observably differ.

**Out of scope** (carried over from source): Stats module, Admin role-gating,
concurrent-create duplicate-email race (needs real SQLite).

## Acceptance criteria
- `CustomerServiceTests`, `ContractServiceTests`, `UserServiceTests`,
  `LeaveTypeServiceTests`, and the nested `CustomerResponseTests` are deleted.
- Every surviving test class ends in `Should`; every method is a
  verb-first phrase with no leading `Should_`.
- `IntegrationFactory` is the only test factory in
  `api.tests.integration`; `TestApiFactory` and per-module `*ApiFactory`
  classes are gone. New endpoint test class setup is ≤ 3 lines.
- `CustomerBuilder` + `Customer.Globex` / `Customer.Acme` exist and are used
  by `CustomerEndpointsShould`.
- All new coverage tests are present and green, including the LeaveType
  unknown-id fix returning 422.
- `dotnet test` passes across `api.tests` and `api.tests.integration`.
