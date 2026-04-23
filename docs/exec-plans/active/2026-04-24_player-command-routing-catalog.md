# Player Command Routing Catalog

## Goal
Harden the post-seam player-command application layer so `PlayerCommandService` becomes a thin router over one shared command catalog rather than a scattering of hand-maintained `switch` branches and partial helpers.

This pass should make the current live command spine clearer:
- module-owned `HandleCommand(...)` remains the only authority path
- `PlayerCommandService` routes by shared metadata and returns disabled/unknown receipts only
- presentation-side command labels and surfaces reuse the same metadata source instead of a parallel application switch
- current command ownership stays aligned with module `AcceptedCommands`

## Scope in / out
In scope:
- Introduce one application-level catalog for live player-command metadata.
- Route `PlayerCommandService.IssueIntent(...)` through that catalog.
- Repoint presentation/read-model label lookups to the shared catalog.
- Add tests that guard catalog completeness and owner/surface alignment.
- Update integration docs for the routing/catalog seam.

Out of scope:
- No new player commands.
- No new domain formulas in modules.
- No save/schema expansion.
- No changes to read-model affordance heuristics beyond label/surface metadata reuse.

## Touched code / docs
- `src/Zongzu.Application/PlayerCommandService/*`
- `src/Zongzu.Application/PresentationReadModelBuilder/*PlayerCommands*`
- `tests/Zongzu.Architecture.Tests/*`
- `tests/Zongzu.Integration.Tests/*`
- `docs/MODULE_INTEGRATION_RULES.md`

## Query / Command / DomainEvent notes
- No new query contracts.
- No new domain events.
- No new command names.
- This is a routing/catalog cleanup only; module ownership and command consequences stay where they are.

## Determinism
No intended simulation behavior change. Accepted commands must still resolve through the same module-owned handlers; rejected/disabled paths must remain pure receipts and must not perturb replay-hash state.

## Save compatibility
None expected. No authoritative state or schema changes.

## Milestones
- [x] Audit current player-command routing, label lookup, and disabled-result duplication.
- [x] Add a shared player-command catalog and slim `PlayerCommandService` to pure routing.
- [x] Repoint application read-model label lookups to the shared catalog.
- [x] Add tests for catalog completeness, owner alignment, and disabled-route behavior.
- [x] Update docs and run build/test verification.

## Tests
- Architecture or integration test proving the catalog covers every current `PlayerCommandNames` entry exactly once.
- Integration test proving catalog module keys align with live command-owner modules.
- Integration test proving a disabled-module command still returns the correct module/surface/label receipt without mutating simulation state.

## Risks
- Presentation code currently calls `PlayerCommandService` static label helpers directly; missing a callsite would keep a hidden second metadata source alive.
- Command labels currently originate in module resolvers; the catalog must reuse them rather than copy strings into a new drift-prone table.

## Result
Completed on 2026-04-24.

Implemented:
- added `PlayerCommandCatalog` as the shared application metadata source for live command name -> module/surface/label/disabled-route mapping
- slimmed `PlayerCommandService` to catalog-based routing plus disabled/unknown receipts only
- removed the old partial routing files that duplicated module ownership and disabled-result handling
- kept presentation-side label access aligned by routing helper lookups through the shared catalog
- extended command-seam integration coverage to assert catalog completeness, uniqueness, owner alignment, and disabled-route replay-hash safety
- updated `MODULE_INTEGRATION_RULES.md` so application read-model builders explicitly reuse the shared command catalog instead of growing a second metadata switch

Verification:
- `git diff --check`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~CommandSeamIntegrationTests"`
- `dotnet build src/Zongzu.Application/Zongzu.Application.csproj -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

Residual risk:
- Application affordance heuristics are still hand-built in `PresentationReadModelBuilder`; this pass unifies command metadata, not affordance availability logic. If future work wants to reduce duplication further, it should do so without moving any domain consequence logic out of modules.
