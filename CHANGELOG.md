# Changelog

## 2026-05-22

- feat(time-entries): week grid cells are now editable — click any weekday cell to type hours (supports `d`=8h, `h`=4h, Del/Backspace=clear hotkeys and comma-as-decimal-separator)
- feat(time-entries): daily column totals and per-row weekly totals update live as you type
- feat(time-entries): today's column header shows a TODAY badge that changes color when hours are logged
- fix(time-entries): typing a value that pushes the day total above 24h is flagged with a red border and rejected on blur, reverting to the previously saved value
- feat(time-entries): pressing Enter, `d`, or `h` in a cell moves focus to the next weekday cell; Del clears and moves focus left; Backspace now deletes a single character as expected
- fix(time-entries): logged-hour values are shown in primary color when blurred
- fix(time-entries): cell input now rejects non-numeric characters (only digits, `.`, and `,` are accepted)

## 2026-05-21

- feat(admin): contracts admin page now uses the same split-panel layout as customers and users — list on the left, detail on the right
- fix(admin): editing a contract or user now returns to that entity's detail view instead of the list
- fix(admin): contract list pagination now correctly sends page size to the API (was relying on the backend default by accident)
- fix(auth): JIT user provisioning no longer silently swallows unrelated database errors on first login
- feat(auth): new users are provisioned automatically on first login via Azure Entra — no manual DB entry needed
- fix(time-entries): task picker search now filters correctly as you type
- fix(time-entries): day column headers now align with task row cells
- fix(time-entries): weekend cells show an X icon to make them visually disabled

## 2026-05-21

- feat(time-entries): task picker — click "Add task" below the week grid to search contracts by customer, contract subject, or task name and add rows; each row shows a colored initials chip, bold customer name, and contract · task subtitle; rows sort A-Z by customer then task; picked tasks are removed from the picker until the page reloads
- fix(time-entries): calendar date picker now navigates to the correct week (was landing on a 404)
- feat(time-entries): week grid fades in when switching between weeks
- fix(time-entries): time entry endpoint rejects non-Monday week start dates with a 400 error
- fix(web): correct PopoverTitle props type to match the rendered element
- chore(claude): fix prep-worktree hook to derive repo root dynamically instead of using a hardcoded path
- feat(time-entries): week skeleton with navigation — navigate to /time-entry to see the current week grid with Mon–Sun headers, weekend columns dimmed, prev/next/Today buttons, and a calendar popover for jumping to any week
- chore(claude): add setup-tracking skill and agent config docs
- chore(claude): add WorktreeCreate hook to prep worktrees on creation

## 2026-05-20

- chore(claude): add 17 new agent skills to .claude/skills
- feat(customers): redesign customers admin as master-detail split-panel layout
- test(api): restructure suite for business-intent naming and fill integration gaps
- test(api): tighten assertions, fix duplicate test, and add WithContact builder
- feat(web): apply Euricom theme and polish customers admin
- feat(admin): users split-panel and server-side archive filtering
- fix(customers): include sort and page in loaderDeps and loader

## 2026-05-19

- test(users): merge duplicate leave allowance update test and add Mode assertion
- fix(api): correct update-before-exist check, case-insensitive email, and test FK seeds
- refactor(api): consolidate update logic from services into repositories
- refactor(api): roll out repository pattern across contracts, users, and leave types
- chore(docs): remove feature/IMPL/VALIDATION docs from all feature slots
- chore(.claude): piv-validate deletes feature/IMPL/VALIDATION docs after verdict
- refactor(contracts): drop default value from BuildTask contractId parameter
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
