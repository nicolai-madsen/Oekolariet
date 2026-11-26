# Architecture

This file is the map. It tells where things live and which source to trust.
It deliberately does not list classes, fields or method signatures. The code, itself, is the source for that.


## Source of truth

If/when sources disagree, the higher one wins and the lower one gets fixed:

1. The code: the domain model is `src/Oekolariet.Domain`.
2. `openapi.yaml`: the API contract (API first).
3. ADRs in `docs/adr/`: why we decided something.
4. Everything else (diagrams, Word docs, chat logs) is a proposal or a snapshot.

Changes to `openapi.yaml`, ADRs or this file go through a PR. A change based on a new design decision also gets a new ADR.


## Solution structure

| Project | Responsibility | May reference |
|---|---|---|
| `Oekolariet.Domain` | Aggregates, value objects, business rules | Nothing |
| `Oekolariet.Application` | Use cases (handlers), repository interfaces (ports) | Domain, Shared |
| `Oekolariet.Infrastructure` | EF Core, `DbContext`, repository implementations, seeding | Application (and Domain through it) |
| `Oekolariet.Api` | Minimal API endpoints, ProblemDetails, CORS, composition root | Application, Infrastructure |
| `Oekolariet.Shared` | DTOs: the C# side of `openapi.yaml` | Nothing |
| `Oekolariet.GuestClient` | Blazor WebAssembly app on the touchscreen (anonymous) | Shared only |
| `Oekolariet.CmsClient` | Blazor WebAssembly app for the staff: Edits drafts and publishes versions (requires JWT and role: Editor) | Shared only |
| `tests/Oekolariet.Domain.Tests` | Unit tests of domain rules | Domain |
| `tests/Oekolariet.Application.Tests` | Unit tests of use cases with fake repositories | Application |

Planned, not created yet: `tests/Oekolariet.Infrastructure.Tests` project for integration tests with a real database.


## Dependency rules

- Dependencies point inwards. Domain knows nothing about EF Core, HTTP or JSON.
- Repository interfaces live in Application, next to the use cases that need them. Infrastructure implements them (dependency inversion).
- The clients only know the API through `Shared` DTOs. A client never references Domain or Application, because everything in a WASM app is downloaded to the browser.
- Guest DTOs never contain correct answers. Guest and editor types are separate, so answers cannot leak by accident (ADR-003).


## Domain in one paragraph

`Exhibition` and `ExhibitionVersion` are separate aggregates that reference each other by id.
`Exhibition.Publish()` creates the next version and makes it active. It owns `LatestVersionNumber`, so version numbers are always +1 without loading the entire version history.
A version's content (`ExhibitionContent`: theme, pages, game) is an immutable value object stored as one JSON column.

Consequences: no foreign keys inside the content (the domain validates it), and `ActiveVersionId` is not a foreign key (it would be a circular dependency).


## Application

- Organised by feature (`Application/Exhibitions/…`). Domain is organised by aggregate.
- One handler per use case, for example `GetActiveVersionHandler`.
- A handler returns `null` for "not found". The endpoint turns that into `404`. Handlers know nothing about HTTP.


## API conventions

The full contract is in `openapi.yaml`. These are the rules the code must follow:

- Base path `/api/v1`, JSON in camelCase, ids are `Guid`, enums are sent as strings.
- Times are UTC (ISO 8601). `DateTime` in the domain is always UTC; Infrastructure enforces it with a converter.
- Errors are ProblemDetails (RFC 9457). Never stack traces.
- Endpoints return typed results (`Results<Ok<T>, NotFound>`) so the OpenAPI output is correct.
- Drafts use optimistic concurrency: `RowVersion` → `ETag`, client sends `If-Match`, mismatch → `412` (ADR-004).
- Default deny: everything requires login unless it is explicitly marked `AllowAnonymous` (guest endpoints).


## Conventions

- Language: everything a developer reads is English: code, comments, commits, PRs, ADRs, this file. Everything a user sees is Danish: UI text and messages shown in the CMS.
- Factory methods take `id` and `…AtUtc` as parameters, so tests and seeding control them.
- Package versions live in `Directory.Packages.props` (central package management). Tools (`dotnet-ef`) live in `.config/dotnet-tools.json`, so run `dotnet tool restore`.
- Seed data goes through the domain (`Exhibition.Create` + `Publish`), never raw inserts, so seed data obeys the same rules as real data.


## Glossary (ubiquitous language)

The PO and guests speaks Danish and the code speaks English. This table connects them. Use these words everywhere.

| Danish (PO, report, UI) | English (code, API) | Meaning |
|---|---|---|
| Udstilling | `Exhibition` | An exhibition, e.g. the waste sorting exhibition |
| Version | `ExhibitionVersion` | A published, immutable snapshot of an exhibition's content |
| Aktiv version | active version (`ActiveVersionId`) | The version guests see right now |
| Kladde | `Draft` | Editable content that is not published yet. At most one per exhibition |
| Udgive | `Publish` | Turn the draft into a new version and make it active |
| Gæsteklient | `GuestClient` | The touchscreen app in the exhibition |
| CMS | CMS | The staff app for editing and publishing |
| Redaktør / medarbejder | `Editor` (role) | A logged-in staff member who can edit and publish |
| Gæst | guest | Anonymous visitor. Has no account |
| Side | `Page` | One information page with text, layout and an optional image |
| Tema | `Theme` | Colours, font, logo and background. A value object |
| Sorteringsspil | `SortingGame` | The game's content: categories and items |
| Kategori | `Category` | Where an item can be placed, e.g. a bin |
| Genstand | item | A thing the guest sorts |
| Spil (et gennemspil) | `Game` | One guest's play-through, bound to one version |

When a decision is made: Write the ADR, update this file in the same PR.
