---
name: crud-entity
description: Reference for implementing a CRUD entity (Create, Read, Update, Archive) with paging, sorting, and filtering.
user-invocable: false
---

- Operations: `POST /` (create), `GET /` (list), `GET /{id}` (read), `PUT /{id}` (update), `PATCH /{id}/archive` + `PATCH /{id}/unarchive` (soft delete).
- PK is `Guid Id`.
- Delete is soft via `bool IsArchived`. No hard DELETE.
- List endpoint accepts `search`, `sort`, `dir`, `page`, `pageSize` as query params. Filter excludes archived rows by default.
- Optional API query params must be nullable (`int? page`, `Sort? sort`) — Minimal API treats non-nullable as required.
- Sort enum exposes the allowed sortable columns. `SortDirection` enum is `Asc | Desc`.
- Enums serialize as PascalCase strings (global `JsonStringEnumConverter`).
