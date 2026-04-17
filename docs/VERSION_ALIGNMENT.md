# VERSION_ALIGNMENT

This is the anti-drift contract between MVP and later releases.

| Foundation | MVP | Post-MVP | Must never change |
|---|---|---|---|
| Time | one authoritative monthly tick | same tick, more modules and projections | no parallel hidden clocks |
| Identity | stable typed IDs | same IDs with more modules and migrations | IDs never recycled |
| Architecture | modular monolith | same modular monolith, more packs | no giant world-manager rewrite |
| State ownership | module-owned namespaces | more namespaces and migrations | no cross-module direct mutation |
| Integration | Query / Command / DomainEvent | same integration model | no shortcut backdoors |
| Diff pipeline | simulation -> diff -> projection | same with richer projections | no narrative-driven authority |
| Spatial shell | great hall, ancestral hall, desk sandbox | richer rooms and overlays | no switch to raw panel-only fantasy |
| Family core | lineage backbone | same backbone with more pressure layers | family data model remains central |
| Commoner base | household and labor substrate | richer migration/disorder/economic pressure | commoners remain the social base |
| Exams | lite upward-mobility loop | richer schools, patronage, rank ladders | exam outcomes remain world-derived |
| Trade | lite wealth/network loop | larger trade web and finance | trade remains economy module, not family hardcode |
| Office | indirect or absent | dedicated module | office cannot be hard-baked into family core |
| Outlaw/banditry | lite or absent | dedicated module | disorder cannot be hidden in generic events |
| Conflict | abstract resolution only | richer force system | no tactical micro takeover |
| Warfare | absent | campaign sandbox | war extends force foundations, not replaces them |
| Save structure | root + module schemas + manifest | more schemas and migrations | unversioned saves forbidden |
| Debug/testing | replay and diff tests | richer anomaly and migration tests | determinism remains mandatory |

## Alignment checks for any new feature
- [ ] additive over existing kernel/modules
- [ ] uses the same command model
- [ ] emits structured events rather than direct foreign state writes
- [ ] fits existing save manifest and schema version rules
- [ ] has a clear module owner
- [ ] does not replace the spatial shell
- [ ] does not convert pathways into isolated modes
