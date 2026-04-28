# Regime Legitimacy Readback V253-V260

## Goal

Add the first readback-thickening layer for Chain 9:

`WorldSettlements.RegimeLegitimacyShifted -> OfficeAndCareer.OfficeDefected -> PublicLifeAndRumor`

This pass should make the player-facing surfaces read as:

- the court/regime signal has a legitimacy tone rather than an abstract event
- the official lane reads a concrete going-or-staying risk
- the county gate and public street read the same shock through Office/PublicLife owner lanes
- the home household is not responsible for repairing court legitimacy

## Scope In

- Reuse existing `WorldSettlements`, `OfficeAndCareer`, and `PublicLifeAndRumor` owner lanes.
- Reuse existing structured event metadata and read-model snapshots.
- Add first-layer readback wording:
  - `天命摇动读回`
  - `去就风险读回`
  - `官身承压姿态`
  - `公议向背读法`
  - `仍由Office/PublicLife分读`
  - `不是本户替朝廷修合法性`
- Keep Application projection-only and Unity copy-only.
- Add focused tests for scheduler path, public-life metadata-only reading, presentation copy, and architecture guardrails.

## Scope Out

- No full regime engine, dynasty-cycle system, faction AI, court module, event pool, or policy economy.
- No regime-recognition ledger, legitimacy ledger, defection ledger, owner-lane ledger, cooldown ledger, dispatch ledger, or projection cache.
- No Application calculation of defection success, public legitimacy truth, or regime outcome.
- No UI/Unity derivation of court/regime results.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `PersonRegistry` expansion and no new manager/god-controller path.

## Ownership

- `WorldSettlements` owns the mandate/regime pressure source.
- `OfficeAndCareer` owns official risk, appointment mutation, county/yamen posture, and `OfficeDefected`.
- `PublicLifeAndRumor` owns public interpretation, street heat, dispatch pressure, public legitimacy, and street-facing readback.
- `SocialMemoryAndRelations` does not write same-month durable residue for this pass; any future durable regime residue needs its own structured aftermath plan.
- Application routes, assembles, and projects structured owner-lane facts only.
- Unity copies projected fields only.

## Save / Schema Impact

Target impact: none.

This pass must not add persisted state, schema fields, root/module schema bumps, migrations, feature-manifest entries, or new save namespaces. If implementation appears to require state, stop and split a schema/migration plan before coding.

## Determinism Risk

Low. Existing deterministic scheduler and existing owner-lane metadata are reused. No randomness, new iteration source, or save/load behavior is expected.

## Milestones

1. Create this ExecPlan and confirm no schema target.
2. Add first-layer Chain 9 readback in owner-lane production code without new state.
3. Add focused integration, PublicLife, architecture, and Unity presentation tests.
4. Update topology, boundary, schema, simulation, UI, acceptance, design audit, and skill docs.
5. Run build, focused tests, `git diff --check`, full solution tests.
6. Archive this plan with validation evidence.

## Tests To Add / Update

- Integration: regime pressure opens one defection path, public-life reads structured interpretation, governance read model projects first-layer readback.
- PublicLife module: `OfficeDefected` consumes settlement metadata and structured defection metadata, not summary/prose.
- Architecture: no Application/UI/Unity authority drift, no summary/prose parsing, no forbidden manager/ledger/schema drift, no `PersonRegistry` expansion.
- Unity presentation: desk/office/great-hall surfaces copy `RegimeOfficeReadbackSummary` from projected fields only.

## Implementation Evidence

- `PublicLifeAndRumor` now reads `OfficeAndCareer.OfficeDefected` structured metadata into `天命摇动读回`, `去就风险读回`, `官身承压姿态`, `公议向背读法`, `仍由Office/PublicLife分读`, `不是本户替朝廷修合法性`, and `不是UI判定归附成败`.
- `PresentationReadModelBuilder` now projects first-layer Chain 9 readback through `RegimeOfficeReadbackSummary` from existing `JurisdictionAuthoritySnapshot` values. Application remains projection-only.
- Unity/presentation adapters continue to copy `RegimeOfficeReadbackSummary` into desk, office, and governance ViewModels only.
- Focused integration isolates Chain 9 by using the existing `WorldSettlements.LastCourtAgendaPressureDeclared` watermark so this pass does not accidentally create a new Chain 8/Chain 9 multi-pressure summary arbitration rule.
- Documentation was updated in topology, design audit, module boundaries, integration rules, data schema, schema namespace rules, simulation, UI/presentation, acceptance tests, and skill matrix.
- Save/schema result remains none: no persisted state, schema bump, migration, manifest entry, ledger, Court module, faction AI, manager/god-controller path, or `PersonRegistry` expansion.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~OfficeCourtRegimePressureChainTests"` passed, 6/6.
- Focused PublicLife module: `dotnet test tests\Zongzu.Modules.PublicLifeAndRumor.Tests\Zongzu.Modules.PublicLifeAndRumor.Tests.csproj --no-build --filter "FullyQualifiedName~HandleEvents_OfficeDefected"` passed, 1/1.
- Focused Unity presentation: `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~Compose_CopiesOfficeYamenReadbackSpineWithoutShellAuthority"` passed, 1/1.
- Focused architecture: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Regime_legitimacy_readback_v253_v260"` passed, 1/1.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed, including the full integration suite.

## Closure

Closed as Chain 9 first readback-thickening only. Full regime recognition, household compliance, ritual legitimacy, force backing, rebellion-to-polity, dynasty-cycle consequences, multi-pressure public-life summary arbitration, and durable regime SocialMemory residue remain future rule-density debts.

## Rollback / Fallback

If wording becomes too broad, keep the metadata-only owner-lane test and narrow presentation text to the smallest tokens that prove ownership: `天命摇动读回`, `去就风险读回`, `公议向背读法`, and `不是本户替朝廷修合法性`.
