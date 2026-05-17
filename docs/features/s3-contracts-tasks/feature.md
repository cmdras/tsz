# S3 — Contracts + Tasks

- Contract fields: `Id` (Guid, PK), `Number` (6-digit, server-assigned via shared Counters service with key `contract`, starts at 100000), `CustomerId` (FK), `ConsultantId` (FK to User), `Subject`, `StartDate`, `EndDate`, `IsArchived`.
- Tasks (1:N under Contract): `Id`, `ContractId` (FK), `Name`, `DayRate`. The Task — not the Contract — is what Time Entry (S7) picks as a row.
- Tasks edited as a multi-row sub-form on the Contract edit page (add/remove rows). First instance of the sub-form pattern S5 reuses for per-user leave config.
- Customer + Consultant pickers = shadcn `Select`, archived rows hidden.
- No cascade when a referenced Customer or User is archived — Contracts keep their FK as-is.
- Copies S1 pattern for Contract: CRUD endpoints + soft-delete via PATCH `/archive` + `/unarchive`, list page with search + Show archived toggle, Card-wrapped form, dedicated `/admin/contracts/$id` and `/admin/contracts/new` routes, Sonner toasts, AlertDialog on archive only.
- Seed: a handful of contracts (consuming `contract` counter from 100000) with a couple of tasks each.

## Open / plan-time questions

- Tasks soft-delete: separate `IsArchived` per Task, or only at Contract level? Time Entry's task picker must hide archived rows either way.
- `DayRate` storage type — decimal, integer cents, or float.
- Tasks save semantics — committed together with the Contract form submit, or independent rows with their own endpoints?
