# Codebase Alignment Long Pass

## Goal
Bring the current modular-monolith codebase closer to the M3/P1/P3 boundary by aligning thin-chain event contracts, read-side notification scope helpers, long-run diagnostics, and Unity/presentation boundary docs with live implementation facts.

## Initial Context
- Existing tracked edits before this pass: `AGENTS.md`, `docs/DESIGN_CODE_ALIGNMENT_AUDIT.md`, `docs/MODULE_INTEGRATION_RULES.md`, `docs/TECH_STACK.md`.
- Existing untracked plans before this pass: `2026-04-24_doc-code-alignment-audit.md`, `2026-04-24_notification-scope-read-helpers.md`, `2026-04-24_project-skill-pack.md`.
- Treat those as prior/user work. Do not overwrite or roll them back.

## Touched Modules / Docs
- Contracts/read-side helpers:
  - `src/Zongzu.Contracts/Queries/NarrativeProjectionQueries.cs`
- Thin-chain authority/event contract:
  - `src/Zongzu.Modules.OfficeAndCareer/OfficeAndCareerModule/OfficeAndCareerModule.cs`
  - `src/Zongzu.Modules.PublicLifeAndRumor/PublicLifeAndRumorModule/PublicLifeAndRumorModule.cs`
- Application presentation selector:
  - `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.Selection.cs`
- Unity-facing presentation adapter:
  - `src/Zongzu.Presentation.Unity/Adapters/Warfare/WarfareAftermathShellAdapter.cs`
- Focused tests:
  - `tests/Zongzu.Modules.OfficeAndCareer.Tests/OfficeAndCareerModuleTests/OfficeAndCareerModuleTests.DebtPressure.cs`
  - `tests/Zongzu.Modules.PublicLifeAndRumor.Tests/YamenOverloadHandlerTests.cs`
  - `tests/Zongzu.Integration.Tests/ProjectionSelectorAlignmentTests.cs`
- Diagnostics / docs:
  - `docs/MODULE_INTEGRATION_RULES.md`
  - `docs/RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` if event topology drift is confirmed
  - this ExecPlan
- Repo hygiene candidates:
  - `ConsoleApp1/`
  - `content/generated/presentation-unity-tests-decompiled/`

## Query / Command / DomainEvent Impact
- Query: add narrow read-only helper methods on existing notification snapshots for settlement/module scope matching.
- Command: no command route or command payload changes planned.
- DomainEvent: no new event names. Chain-1 drift classified as an alignment bug: `YamenOverloaded` had been emitted with official `PersonId` as `EntityKey`, forcing PublicLife to fan out globally. It now carries settlement `EntityKey`, plus `settlementId`, `personId`, cause/source, backlog, and task-load metadata.
- Scheduler: no scheduler behavior changes planned.

## Determinism Impact
- Expected runtime determinism impact is low and bounded. Read-side helper work has none. Chain-1 behavior is now settlement-scoped instead of all-settlement public-life heat; the path stays deterministic and module-owned.
- If any pressure formula or module authority code is touched after diagnostics, add targeted determinism/replay validation before full solution tests.

## Save / Schema Impact
- Expected save/schema impact is none.
- Do not change module state, root save version, module schema versions, migrations, or feature manifests unless a proven bug requires it and this plan is updated first.

## Unity / Presentation Boundary Impact
- `Zongzu.Presentation.Unity.ViewModels` and `Zongzu.Presentation.Unity` remain projection/read-model adapters only.
- Unity host shell exists under `unity/Zongzu.UnityShell`, but authoritative simulation remains under `src/`.
- No Unity scene, prefab, asset, or MonoBehaviour authority changes planned.

## Milestones
1. [x] Confirm git status and existing untracked/tracked work ownership.
2. [x] Read selected skills and mandatory docs at summary level.
3. [x] Inspect live scheduler, module runner, bootstrap, command catalog, presentation selector, Unity adapter, and diagnostics tests.
4. [x] Build the real PublishedEvents / ConsumedEvents / scheduler / projection behavior ledger.
5. [x] Classify thin-chain event drift as projection-only receipt, future contract, dormant source, or alignment bug.
6. [x] Implement notification scope read helpers without changing authority, scheduler, save, or schema.
7. [ ] Run ten-year health diagnostic and classify saturation/unconsumed/dormant risks.
8. [x] Verify Unity/presentation dependency boundary and docs status.
9. [x] Inspect `ConsoleApp1/` and `content/generated/presentation-unity-tests-decompiled/` references and record cleanup risk.
10. [ ] Run focused tests, `git diff --check`, and `dotnet test Zongzu.sln --no-restore`.

## Tests To Run
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter ProjectionSelectorAlignmentTests`
- `dotnet test tests/Zongzu.Modules.OfficeAndCareer.Tests/Zongzu.Modules.OfficeAndCareer.Tests.csproj --no-restore --filter HouseholdDebtSpiked`
- `dotnet test tests/Zongzu.Modules.PublicLifeAndRumor.Tests/Zongzu.Modules.PublicLifeAndRumor.Tests.csproj --no-restore --filter YamenOverloaded`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter TenYearSimulationHealthCheckTests`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-restore`

## Ten-Year Diagnostic Classification
- Run: `TenYearSimulationHealthCheckTests.CampaignEnabledStressSandbox_TenYearHealthReport`, seed `20260421`, 120 months, final date `1210-01`, replay hash `3DA3A73A883CC6F3A1AB8B796B6E9903BCB4F7F488BCBDB396BCE60786B602E1`.
- Result: diagnostic test passed; no determinism failure observed.
- Pressure health debt: Lanxi and Yonghe saturate long-run `Security`, `BanditThreat`, `RoutePressure`, `DisorderPressure`, `CommonerDistress`, and `MigrationPressure` under the campaign-enabled stress sandbox. This is classified as balance/recovery-model debt, not a single contract bug. Do not hide it by lowering event numbers; investigate recovery cadence, response activation, recurring demand, suppression relief, household recovery, and campaign fatigue/aftermath dampening as separate follow-up work.
- Social-memory debt: several clans reach `grudge=100`, `fear=100`, `shame=100`, and `favor=-100`; many memories have `intensity=100` but `weight=0`. This needs a focused memory decay/weighting and pressure-tempering pass before formula changes.
- Emitted but no authority consumer classification:
  - projection-only receipts today: `FamilyCore.ClanPrestigeAdjusted`, `SocialMemoryAndRelations.ClanNarrativeUpdated`, `FamilyCore.FamilyMembersAged`, `PersonRegistry.PersonDeceased`, `PersonRegistry.PersonCreated`, `OfficeAndCareer.OfficeGranted`, `OfficeAndCareer.OfficeLost`, `PopulationAndHouseholds.MigrationStarted`, `WorldSettlements.SettlementPressureChanged`.
  - future/dormant downstream contracts: `ConflictAndForce.ConflictResolved`, `ConflictAndForce.CommanderWounded`, `ConflictAndForce.ForceReadinessChanged`, `WorldSettlements.SeasonalFestivalArrived`, `WorldSettlements.CanalWindowChanged`, `SocialMemoryAndRelations.EmotionalPressureShifted`, `SocialMemoryAndRelations.PressureTempered`.
- Declared but never emitted classification in this seed:
  - dormant source / condition not reached: `OfficeAndCareer.YamenOverloaded`, `OfficeAndCareer.AmnestyApplied`, `OfficeAndCareer.PolicyWindowOpened`, `OfficeAndCareer.OfficeDefected`, `OrderAndBanditry.DisorderSpike`, `PopulationAndHouseholds.HouseholdBurdenIncreased`, `PopulationAndHouseholds.HouseholdSubsistencePressureChanged`, `TradeAndIndustry.GrainPriceSpike`, `WarfareCampaign.CampaignSupplyStrained`, `WorldSettlements.ComplianceModeShifted`.
  - future contract or acceptance-debt branch: `ConflictAndForce.DeathByViolence`, `ConflictAndForce.MilitiaMobilized`, `EducationAndExams.StudyAbandoned`, `EducationAndExams.TutorSecured`, `FamilyCore.BranchSeparationApproved`, `FamilyCore.HeirAppointed`, `FamilyCore.HeirSuccessionOccurred`, `FamilyCore.LineageMediationOpened`, `FamilyCore.MarriageAllianceArranged`, `OrderAndBanditry.SuppressionSucceeded`, `PersonRegistry.FidelityRingChanged`, `PublicLifeAndRumor.StreetTalkSurged`, `SocialMemoryAndRelations.FavorIncurred`, `SocialMemoryAndRelations.GrudgeSoftened`.

## Unity / Presentation Boundary Check
- `unity/Zongzu.UnityShell` exists with `Assets/`, `Packages/`, and `ProjectSettings/`; `Library/`, `Temp/`, `Logs/`, and `UserSettings/` are local Unity workspace artifacts.
- `Zongzu.Presentation.Unity.ViewModels` references only `Zongzu.Contracts` and synchronizes DTO sources into the Unity host after build.
- `Zongzu.Presentation.Unity` references only `Zongzu.Contracts` and `Zongzu.Presentation.Unity.ViewModels`.
- Source search found no `UnityEngine` / `UnityEditor` references under `src/` simulation projects.
- `AGENTS.md`, `docs/TECH_STACK.md`, and `docs/UI_AND_PRESENTATION.md` already state that the Unity host exists while authoritative simulation remains under `src/`.
- `Zongzu.Architecture.Tests` passed, including project-reference and no-Unity-reference checks.

## Repo Hygiene Notes
- `ConsoleApp1/` appears to be a standalone hello-world project with local `.vs/`, `bin/`, and `obj/`; it is not referenced by `Zongzu.sln`, docs, `src/`, tests, scripts, or content except this ExecPlan. Cleanup candidate, but not removed in this pass.
- `content/generated/presentation-unity-tests-decompiled/` appears to be generated/decompiled test scratch output targeting `netcoreapp8.0`, with direct assembly references into old test bin output. It is not referenced by `Zongzu.sln`, docs, `src/`, tests, scripts, or authoring content except this ExecPlan. Cleanup/archive candidate, but not removed in this pass.

## Validation Results
- Passed: `dotnet test tests/Zongzu.Modules.OfficeAndCareer.Tests/Zongzu.Modules.OfficeAndCareer.Tests.csproj --no-restore --filter HouseholdDebtSpiked /p:UseSharedCompilation=false`
- Passed: `dotnet test tests/Zongzu.Modules.PublicLifeAndRumor.Tests/Zongzu.Modules.PublicLifeAndRumor.Tests.csproj --no-restore --filter YamenOverloaded /p:UseSharedCompilation=false`
- Passed: `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter ProjectionSelectorAlignmentTests /p:UseSharedCompilation=false`
- Passed: `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter "FullyQualifiedName~RenzongPressureChainTests.Chain1" /p:UseSharedCompilation=false`
- Passed: `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter TenYearSimulationHealthCheckTests --logger "console;verbosity=detailed" /p:UseSharedCompilation=false`
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- Passed: `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- Passed: `git diff --check`
- Passed: `dotnet test Zongzu.sln --no-restore /p:UseSharedCompilation=false`

## Rollback / Fallback Notes
- Read-helper changes can be reverted from the three code/test files without save or determinism consequences.
- If ten-year diagnostics reveal formula debt but not enough evidence for a safe formula fix, keep the runtime unchanged and record it as diagnostic debt with a focused guardrail or follow-up branch.
- Do not delete repo hygiene candidates in this pass unless references prove they are safe to remove and this plan is updated.
