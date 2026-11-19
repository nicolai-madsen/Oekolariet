# ADR-0002: Draft and Immutable published versions

**Status:** Proposed


## Context

The PO wants changes to be saved atomically ("all or nothing"). Guests must never see a mix of old and new content.


## Decision

Editors work on a draft of the whole exhibition. When the draft is published, it becomes a new version that replaces the active version in one step.
Published versions are immutable. They can never be changed or deleted, only archived.


## Alternatives considered

- **Status per page (Draft/Published).** Simpler, but guests could see inconsistent content if only some changes are published.


## Consequences

- Guests always see consistent content, and we keep a full history.
- Games and scores can always refer to the version they were played on.
- Costs more storage and make the publishing logic more complex
