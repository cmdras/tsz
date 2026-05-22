# API Tests Package

## Seeding test data

`SeedUser` helpers must accept a unique email per user — `Users.Email` has a unique index, so seeding two users with the same default email will fail with a constraint violation. Always pass a distinct email when seeding more than one user in a test.
