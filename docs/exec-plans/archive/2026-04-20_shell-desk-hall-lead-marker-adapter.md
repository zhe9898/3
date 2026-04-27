## Goal
- expose whether a desk settlement node is the current monthly lead hall-docket node
- keep the slice adapter-only so node emphasis can follow hall lead status without recomputing or inferring hall ordering

## Scope in
- add thin lead-marker fields to desk settlement nodes in `Zongzu.Presentation.Unity`
- derive those fields from `HallDocket.LeadItem` only
- add focused presentation coverage for matching and non-matching settlements
- update acceptance notes

## Scope out
- no changes to `Zongzu.Contracts`
- no authority changes
- no new application ordering logic
- no notification-center changes
- no shell object grammar fields in shared contracts

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- none

## Determinism risk
- none in authority
- adapter reads existing deterministic hall-docket projections only

## Milestones
1. add thin lead-marker fields to desk settlement nodes
2. derive lead status from `HallDocket.LeadItem` only
3. ensure secondary items never imply lead status
4. add focused shell coverage
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if lead markers prove premature for current shell work, keep the fields default-empty and leave the existing hall-agenda rows/counts in place
- if future adapter work needs richer emphasis metadata, derive it later from the same lead-item check rather than changing contracts now

## Result notes
- desk settlement nodes now expose read-only lead-marker fields derived from `HallDocket.LeadItem`
- matching settlements surface `HasLeadHallAgendaItem = true` plus the lead lane key
- secondary items alone do not mark a node as lead

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
