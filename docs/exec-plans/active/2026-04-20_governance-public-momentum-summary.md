## Goal
- lift the new xun-facing county-gate / yamen heat into a neutral governance read-model trend
- keep the result projection-only so hall / desk can consume it later without hard-wiring shell object grammar into contracts

## Scope in
- add a read-only public-momentum summary to governance lane / focus / docket snapshots
- derive that summary from existing `PublicLifeAndRumor`, `OfficeAndCareer`, and optional `OrderAndBanditry` projections only
- thread the summary into governance docket `WhyNowSummary` so the hall-facing read model can explain current tightening without new UI logic
- add integration coverage for governance bundle output
- update integration / acceptance notes

## Scope out
- no schema bump
- no new authority state
- no new commands
- no hall object grammar fields
- no UI code changes

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Integration.Tests`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- no root schema bump
- no module schema bump
- this slice adds only read-model fields inside application/contracts

## Determinism risk
- low
- the summary is derived from existing snapshots only
- ordering and authority cadence remain unchanged

## Milestones
1. add neutral public-momentum fields to governance read models
2. derive the summary from public-life plus office pressure
3. carry it into governance docket text without changing sort rules
4. add integration assertions
5. run build/test verification

## Tests to add/update
- `Zongzu.Integration.Tests`
- full solution build / test verification

## Rollback / fallback plan
- if the new wording proves too specific, keep the new field but remove it from `WhyNowSummary`
- if governance lane starts feeling too presentation-shaped, keep the field on the lane only and drop it from focus/docket

## Open questions
- whether a later slice should also thread this trend into desk-settlement summaries directly, or keep that mapping in presentation adapters
- whether future xun trend summaries should distinguish route-led tightening from paper-led tightening with separate neutral fields

## Result notes
- governance lane / focus / docket now carry a read-only public-momentum summary
- the summary compresses current county-gate tightening from public-life notice / dispatch / report-lag plus office task-load / clerk drag
- no new cadence state was introduced; the summary is rebuilt each read-model pass from existing projections only
- hall/governance `WhyNowSummary` now includes that momentum signal, so month-end review can read ongoing tightening without inventing authority in UI code

## Verification
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
