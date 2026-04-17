# CHANGELOG

## Modular rebuild line

This package supersedes earlier spec bundles by restructuring the project around a **modular monolith**.

### Why this rebuild exists
Earlier bundles described:
- product direction
- MVP vs post-MVP scope
- engineering rules
- conflict and visual shell

But they were still too close to a **system pile**:
- broad world objects risked becoming overly fat
- expansions like exams, trade, office, banditry, and warfare were not cleanly separated
- structural extensibility was weaker than the product ambition

### What changed in this rebuild
1. Introduced a formal **kernel + modules + projections + presentation** architecture.
2. Added explicit module docs:
   - `MODULE_BOUNDARIES.md`
   - `EXTENSIBILITY_MODEL.md`
   - `MODULE_INTEGRATION_RULES.md`
   - `SCHEMA_NAMESPACE_RULES.md`
3. Reframed social positions as pathways:
   - family
   - commoner
   - exams
   - trade
   - office
   - outlaw/banditry
4. Split force into two layers:
   - `ConflictAndForce`
   - `WarfareCampaign`
5. Updated scope/alignment docs so MVP and post-MVP are structurally additive, not just thematically aligned.
6. Rewrote repo layout and schema ownership to avoid strong cross-module coupling.

### Current status
This bundle should be treated as the authoritative spec line for future Codex work.
Older bundles should be ignored except for historical reference.
