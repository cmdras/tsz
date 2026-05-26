# Changelog

## 2026-05-26

- feat(leave-overview): a new `/leave-overview` page shows the whole year at a glance — 12 mini-calendars in a 4×3 grid alongside a balance sidebar; days with any logged leave are outlined, today is highlighted with a primary-colour pill, and weekend cells are tinted
- feat(leave-overview): the sidebar lists each Limited leave type (alphabetical) with a coloured dot, inline progress bar, "rem / total LEFT" text, and a big remaining-days number that clamps at 0 when allowance is exceeded; the header summarises the year as "N types · X days left · Y taken"
- feat(leave-overview): clickable legend chips above the calendar (one per Limited and Unlimited type) toggle a focus mode — matching days light up in the type's colour, a "FOCUSED ON …" banner appears, and the `?focus=<id>` URL param round-trips so the view restores on refresh or share
- feat(leave-overview): year prev/next arrows and a Today button move across years; the URL `?year=YYYY` round-trips, arrows are bound-disabled at 2000/2100, and year navigation now preserves an active `?focus` selection
- feat(leave-overview): clicking any day cell on any month navigates to `/time-entry?week=<ISO Monday>` so the calendar doubles as a year-wide week-navigator for past, current, and future weeks
- feat(timesheets): a new `/timesheets` page shows the current month as a Mon–Sun calendar grid; prev/next chevrons, a clickable month label with a date-picker popover, a Today button, and an Export month button (toast-only for now) round out the header
- feat(timesheets): each day cell shows total hours plus up to two colored chips (project chips share the customer avatar color; leave chips are amber) with a `+N more` chip when there are extras, and clicking any cell jumps to that week in `/time-entry`
- feat(timesheets): a right-hand sidebar summarises the month — total workdays + hours, and a breakdown per customer and per leave type sorted by hours descending
- feat(timesheets): past weeks the user never submitted are marked with an amber left-border so they stand out at a glance; submitted and future weeks have no decoration
- feat(skills): a dedicated `/app-changelog` skill writes one dated CHANGELOG.md section per feature branch in user-facing terms; changelog authorship is now decoupled from committing
- feat(skills): a new `/app-orchestrate` skill processes all `ready-for-agent` issues sequentially — each lands on its own sub-branch, merges into a shared feature branch, and one PR is opened to master for human QA

## 2026-05-25

- refactor(admin): ContractForm and ContractDetailPanel decomposed — tasks table moved into a dedicated `ContractTasksField` component, status badge into `ContractStatusBadge`, and archive description text into `archiveMessage`; CRAP for both functions drops below 30 and neither is flagged HIGH by fallow

- refactor(time-entries): the Time Entry page is decomposed — copy-last-week logic lives in a dedicated `useCopyLastWeek` hook, the status card is its own component, and the header toolbar is extracted into `TimeEntryHeader`; behavior is unchanged
- fix(admin): users list now refreshes immediately after saving a new or edited user, instead of showing stale data for up to 30 seconds
- refactor(admin): UserForm decomposed into focused helpers — `useNavigateOnDone`, `useUserFormSubmit`, `UserInfoSection`, `UserLeavesSection`, `LeaveTableRow`, `LeaveModeCell`, `LeavePicker`; no function exceeds 60 lines and UserForm CRAP drops below 30
- refactor(time-entry): the week grid is broken into focused modules — row rendering, keyboard navigation, and model helpers each live in their own file; `week-grid.tsx` drops from 332 to 143 lines with no change in behavior
- refactor(time-entries): the shared input schema for save-draft and submit-week is now defined once instead of duplicated inline in each server function

- refactor(admin): the Contracts and Leave Types list panels now share a common shell — search input, filter tabs, pagination footer — reducing duplication; behavior is unchanged
- fix(admin): the search input in list panels now correctly reflects the URL state on browser back/forward navigation
- refactor(admin): the cancel/save button row in all four admin forms is now a shared FormFooter component; the save button is also disabled while a submission is in flight to prevent double-submit
- fix(admin): list panels no longer show a blank body without explanation when the current page has no items but the total result count is non-zero (e.g. navigating to a stale page number after filtering)

## 2026-05-23

- feat(admin): the archive filter on Customers, Contracts, Leave Types, and Users list panels is now a unified three-tab control — All / Active / Archived — replacing the old per-page boolean toggles; all pages default to showing all records

- refactor(repositories): pagination logic extracted into a shared `ToPagedResultAsync` helper; the offset formula is now enforced in one place across all list endpoints

- fix: the progress bar on the "Logged this week" card now correctly reflects the current value in accessibility attributes (`aria-valuenow`) and Radix data attributes
- fix(time-entries): "Copy last week" now shows a distinct message when last week had entries but all were archived/unavailable, instead of the misleading "No entries last week."
- style(time-entries): Logged, Last week, and Status summary cards now sit side-by-side above the grid with refreshed styling — large hours display, chips with primary-color dots, and a corner-bracketed status card

## 2026-05-22

- feat(time-entries): a "Logged this week" card shows total hours vs the 40h target with a progress bar and +/- indicator
- feat(time-entries): a "Last week" card shows the top 5 customer·contract chips (with overflow count) from the previous week's entries
- feat(time-entries): "Copy last week" button populates the current week's grid with last week's rows and hours; if the current week already has entries, an overwrite confirmation is shown; archived tasks/leave types are silently skipped
- feat(time-entries): `GET /api/time-entries/weeks/{weekStart}` now includes a populated `previousWeekSummary` derived from the previous ISO Monday

- feat(time-entries): consultants can now submit a week as final — a "Submit week" button opens a confirmation dialog, locks the week, and switches the grid to read-only; cells show static hours or "—", pickers and Save/Submit buttons are hidden
- feat(time-entries): submitted weeks cannot be edited via the API (PUT returns 409), and the submit endpoint is also idempotent-safe (second submit returns 409)
- feat(time-entries): a Status card above the grid shows "Draft" or "Submitted" and the last-saved timestamp ("Not saved yet" when empty)
- feat(time-entries): the page title reads "Time entry submitted." for locked weeks, in addition to the existing "logged." and "empty." states
- feat(time-entries): week navigation from a submitted week to an unsubmitted week restores the full editable UI

- refactor(api): archiving and unarchiving any entity now goes through a single shared helper — future archivable entities get the behavior for free by implementing `IArchivable`
- fix(admin): archiving or unarchiving a Customer, Contract, Leave Type, or User now navigates back to the list instead of showing "not found"

- chore(web): removed unused components, dead exports, and unreachable code flagged by fallow — zero warnings project-wide

- feat(admin): the Leave Types admin page now uses the same split-panel layout as Customers, Contracts, and Users — list panel on the left with search and archive filter, detail panel on the right with Edit and Archive/Unarchive actions
- fix(time-entries): leave row initials chip is now amber instead of a random color
- feat(time-entries): consultants can now log leave alongside project work — an "Add leave" button opens a searchable popover showing non-archived leave types not already on the grid
- feat(time-entries): leave rows display with an amber initials chip and amber hour values to distinguish them from project task rows
- feat(time-entries): project rows always appear above leave rows; within each group rows are sorted alphabetically
- feat(time-entries): the TODAY badge turns amber when the current day has any logged leave hours
- feat(time-entries): the server now validates leave type foreign keys on save — unknown or archived leave types are rejected
- feat(time-entries): leave types already on the grid for the current week are excluded from the "Add leave" picker

- fix(time-entries): keyboard navigation shortcuts (d, h, Enter, Del) now work on rows that were loaded from a saved draft, not only on rows added in the current session
- fix(time-entries): typing a value greater than 24 in a single cell is now flagged with a red border and rejected on blur, reverting to the previously saved value
- fix(time-entries): "Save draft" no longer sends an empty payload when the grid ref is unexpectedly null, which would have silently deleted all week entries
- fix(tests): all 96 integration tests now pass — JIT user provisioning no longer creates a ghost user mid-test, breaking user list count assertions
- feat(time-entries): time entries are now persisted — clicking "Save draft" on the week grid writes changes to the server and the data survives a page reload
- feat(time-entries): the page title changes to "logged." once at least one task row is saved
- feat(time-entries): navigating away with unsaved changes now triggers a confirmation dialog (both in-app navigation and browser close/reload)
- fix(time-entries): "Save draft" button correctly reappears after editing a previously saved week
- fix(time-entries): dirty indicator is cleared when navigating to a different week
- feat(time-entries): week grid cells are now editable — click any weekday cell to type hours (supports `d`=8h, `h`=4h, Del/Backspace=clear hotkeys and comma-as-decimal-separator)
- feat(time-entries): daily column totals and per-row weekly totals update live as you type
- feat(time-entries): today's column header always shows a TODAY badge in primary color
- feat(time-entries): adding a task row automatically focuses its Monday cell
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
