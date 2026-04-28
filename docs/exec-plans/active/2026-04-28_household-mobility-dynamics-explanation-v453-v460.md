# Household Mobility Dynamics Explanation V453-V460

## Intent

V453-V460 adds the first structured explanation layer for the existing multi-dimensional household mobility dynamics.

This is not a new social-class rule and not a new mobility algorithm. `PopulationAndHouseholds` already owns the household livelihood, labor, debt, grain, land, migration, and pool carriers. This pass makes the player-facing read model explain which dimensions are currently shaping a household's mobility posture, while preserving the v445-v452 rule: near detail, far summary.

## Baseline

- `main` at `121bfe1 Preflight fidelity scale budget`.
- V381-V452 closed social-position visibility, owner-lane preflight, and fidelity scale-budget preflight.
- Existing runtime facts: `HouseholdSocialPressureSnapshot` already carries per-household signals; `SettlementMobilitySnapshot` already carries pool/focus/scale readbacks; `PersonRegistry` owns identity plus `FidelityRing` only.

## Scope

- Add runtime read-model fields for household mobility dynamics explanation and dimension keys.
- Assemble the explanation from existing structured household/social-pressure signals, not from prose.
- Copy the projected explanation into the Unity desk settlement node when a local household pressure exists.
- Add focused integration, presentation, and architecture proof.
- Update docs and acceptance evidence.

## Non-Goals

- No new production authority rule.
- No change to monthly/xun cadence or migration/livelihood formulas.
- No fidelity-ring mutation beyond existing owner-owned paths.
- No commoner status state, class ladder, zhuhu/kehu conversion, office-service route, trade-attachment route, or durable social-position residue.
- No global person scan, regional selector, class engine, commoner status engine, movement engine, or personnel engine.
- No new module, manager, controller, ledger, cooldown, selector, scheduler pass, or projection cache.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity authority.
- No parsing of `DomainEvent.Summary`, person dossier prose, social-position text, source-key display, scale-budget text, notification prose, receipt prose, mobility text, public-life lines, or docs text.

## Target Schema / Migration

Target schema/migration impact: none.

The new fields are runtime read-model / ViewModel fields only. If future work stores mobility explanations, dimension histories, precision bands, target-cardinality state, selector watermarks, route history, durable residue, or projection caches, stop and write a separate schema/migration plan first.

## Determinism / Performance

- No scheduler, save/load, event flow, command route, or authority algorithm changes.
- Explanation assembly uses already-built household pressure signals and bounded local projection data.
- Hot path: presentation build, over existing household read-model rows.
- Expected touched count: one explanation per existing `HouseholdSocialPressureSnapshot`.
- Ordering/cap: deterministic by signal score descending, then signal key ordinal; top dimension keys are capped.

## Validation Plan

- Focused integration test for structured multi-dimensional explanation.
- Focused Unity presentation test for copy-only desk node field.
- Focused architecture guard for no schema/ledger/prose/parser/authority drift.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration or production data rollback is required.

## Milestones

- [x] Add ExecPlan.
- [x] Add runtime read-model and Unity copy-only field.
- [x] Add focused tests and docs.
- [x] Run final validation lane.

## Completion Evidence

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "Name=MonthlyPressure_DrivesLivelihoodDriftFocusRingAndProjectedMobilityReadback"` passed.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter "Name=DeskSandbox_CopiesProjectedHouseholdMobilityDynamicsWithoutComputingRules|Name=PresentationShellViewModel_SystemTextJson_Serialize_NewtonsoftJson_Deserialize"` passed.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_dynamics_explanation_v453_v460_must_project_existing_dimensions_without_schema_or_authority_drift"` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.

Schema/migration impact remains none: all new data is runtime read-model/ViewModel projection only.
