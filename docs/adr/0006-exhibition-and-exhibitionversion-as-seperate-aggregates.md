# ADR-0006: Exhibition and ExhibitionVersion as seperate aggregates

**Status:** Proposed


## Context

According to ADR-0002, every new publish creates a new and immutable version. This means an exhibition can/will accummulate many versions over time.
Each version contains the exhibition's full content. The exhibition must still be renamable and achivable, and it must point to exactly one active version.
Version numbers must be unique and sequential per exhibition.


## Decision

Exhibition and ExhibitionVersion are two seperate aggregate roots. A version references its exhibition through ExhibitionId, and the exhibition points to the active version through ActiveVersionId.
Exhibition own the version numbering rule through LatestVersionNumber and is the only place new versions can be created { Publish() }.

Because ADR-0002 requires the active version to switch in one step, Publish() saves the new version and the updated exhibition in **one transaction**.
This is a deliberate exception to the rule of one aggregate per transaction.


## Alternatives considered

- **A: Versions as part of the Exhibition aggregate.**
Rejected because an aggregate is loaded as a whole. Even a simple rename load every version with it's full content and concurrent edits would block each other unnecessarily.
- **B: Seperate aggregates updated in two transactions through a domain event.** ( VersionPublished -> set ActiveVersionId )
Rejected because it breaks ADR-0002: Between the two transactions the new version exists but is not active.
It also adds complexity without any real benefit, while both aggregates live in the same database, anyways.


## Consequences

Positive:

- Small aggregates: Renaming and archiving do not touch the versions
- The numbering rule lives in the domain and can be unit tested without loading the entire history or database.
- A unique index on ExibitionId and VersionNummber is the safety net, if two publishes run at the same time.

Negative:
- Publish() spans two aggregates and depends on both being the same database (does not work with microservices)
- ActiveVersionId cannot be a Foreign Key, because Exhibition -> Version -> Exhibition is a circular depepndency.
Only the domain guarantees that the reference is valid.

**REVISIT** if Exhibition and ExhibitionVersion are moved to different services or database. Alterantive B then becommes necessary and ADR-0002 has to be renegotiated. 
