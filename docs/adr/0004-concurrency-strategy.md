# ADR-0004: Concurrency strategy

**Status:** Proposed


## Context

Many guests finish games at the same time. Several editors may draft the same draft.


## Desicion

- **Scores:** One row per game (insert-only). Averages and highscores are calculated with queries. Inserts do not collide.
- **Drafts:** Optimistic concurrency with RowVersion. Conflicts are rare, and an editor can be told about the conflict and reload the draft. 


## Alternatives considered

- **RowVersion on a shared counter** for scores. High contention would cause many conflicts and retries. 


## Consequences

- No lost updates
- Published versions need no concurrency control, because they are immutable.
