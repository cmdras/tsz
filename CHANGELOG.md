# Changelog

## 2026-05-19

- refactor(counters): drop Counters table, inline MAX+1 per module
- chore(auth): remove debug access token log from API client middleware
- chore(auth): log access token in API client middleware
- refactor(auth): replace manual JWT setup with Microsoft.Identity.Web
- refactor(customers): extract data access into CustomerRepository
- fix(navbar): replace non-compliant green brand text with theme foreground color
- chore(.claude): update skills for response DTOs and fix PostToolUse hook exit code
- refactor(api): introduce response DTOs and centralize domain exception handling
- chore(lint): fix oxlint config and suppress false-positive warnings
- feat(admin): add dashboard with active entity count stats
- refactor: relocate oxlint config to packages/web and chain error causes
- feat(auth): implement S6 OAuth with Entra ID, fix v2 issuer and claim extraction
- refactor(api): split Program.cs into per-module configuration extensions
- test(auth): replace HTTP-based auth tests with endpoint metadata introspection
- refactor(api): move AppDb connection string to appsettings.json

## 2026-05-18

- feat(shell): replace sidebar with Euricom top navbar
- feat(users): implement S5 user leave allowance configuration
- docs(s5): add VALIDATION.md for users leave config
- fix(users): use step="any" on leave days input to allow decimals

## 2026-05-17

- chore(skills): add piv-plan, piv-implement, piv-validate to project level
- feat(contracts): implement S3 contracts and tasks with shared counters
- chore(skills): sync git-commit skill from awesome-copilot
- feat(leave-types): implement S4 leave types admin catalog

## 2026-05-14

- chore(agents): add CRUD endpoint and service skills
- refactor(web): consolidate admin entity logic in features directory
- refactor(admin): consolidate sort params into slug-based URL param

## 2026-05-13

- feat(users): implement basic CRUD with pagination, sorting, and search
- refactor: apply descriptive variable naming throughout codebase
- feat(customers): implement sorting and pagination
- chore(agents): add crud-entity pattern reference skill
- chore(customers): expand customer seed data to 30 records

## 2026-05-12

- feat(customers): implement S1 customers admin with database foundation

## 2026-05-11

- chore(gitignore): exclude Claude Code scheduled_tasks lock file
- feat(shell): implement S0 shell with layout and routes
- chore(git-commit): add changelog automation to skill
- docs(planning): introduce S0 shell slice, adjust S1 scope
- docs(planning): revise S0 scope and slice numbering - defer auth to S6
- chore: add concision convention to agent instructions
