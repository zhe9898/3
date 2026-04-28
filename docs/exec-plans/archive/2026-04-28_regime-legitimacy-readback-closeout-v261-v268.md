# Regime Legitimacy Readback Closeout V261-V268

## Goal

Close the v253-v260 Chain 9 first readback-thickening branch as audit evidence only:

`WorldSettlements.RegimeLegitimacyShifted -> OfficeAndCareer.OfficeDefected -> PublicLifeAndRumor`

This closeout should make the docs and architecture guard say clearly that the branch is complete only at the first readback layer: mandate/regime pressure can be read through Office/PublicLife owner lanes as `天命摇动读回`, `去就风险读回`, `官身承压姿态`, and `公议向背读法`, while full regime recognition remains future debt.

## Scope In

- Add a closeout ExecPlan and archive it with evidence.
- Add docs/test governance for v253-v260 as a closed first readback branch only.
- Add architecture guard coverage proving the closeout is docs/tests only.
- Preserve the existing owner split:
  - `WorldSettlements` owns mandate/regime pressure source.
  - `OfficeAndCareer` owns defection risk and appointment mutation.
  - `PublicLifeAndRumor` owns public interpretation.
  - `SocialMemoryAndRelations` has no same-month durable residue in this branch.
  - Application projects structured owner-lane facts only.
  - Unity copies projected fields only.

## Scope Out

- No production rule changes.
- No full regime engine, dynasty-cycle system, faction AI, Court module, event pool, or policy/court economy.
- No regime-recognition ledger, legitimacy ledger, defection ledger, owner-lane ledger, cooldown ledger, scheduler ledger, projection cache, or public-allegiance ledger.
- No Application calculation of defection success, legitimacy repair, public allegiance, or regime outcome.
- No UI/Unity derivation of court/regime results.
- No parsing of `DomainEvent.Summary`, receipt prose, projection prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `PersonRegistry` expansion and no new manager/god-controller path.

## Save / Schema Impact

Target impact: none.

This pass must remain docs/tests only. It must not add persisted state, schema fields, root/module schema bumps, migrations, feature-manifest entries, or new save namespaces. If closeout evidence appears to require state, stop and split a schema/migration plan first.

## Determinism Risk

None expected. No runtime behavior, scheduler cadence, randomness, iteration order, or save/load behavior should change.

## Milestones

1. Create this ExecPlan and confirm no schema target.
2. Update topology, design audit, module boundaries, integration rules, data schema, schema namespace rules, simulation, UI/presentation, acceptance, and skill matrix docs.
3. Add architecture closeout guard.
4. Run focused architecture test, build, `git diff --check`, and full solution tests.
5. Archive this plan with validation evidence.

## Tests To Add / Update

- Architecture: v261-v268 docs must say first-readback-only, future full-regime debt remains explicit, no schema/migration/ledger/Court/faction/manager/`PersonRegistry` expansion is added, and production code does not gain closeout-only authority.

## Implementation Evidence

- Updated the topology index, design/code audit, module boundaries, integration rules, data schema, schema namespace rules, simulation, UI/presentation, acceptance tests, and skill matrix with v261-v268 closeout wording.
- Added an architecture closeout guard proving v261-v268 is docs/tests only, first-readback-only, schema-neutral, and not a source of production authority.
- Production source was not changed in this pass.
- Save/schema result remains none: no persisted state, schema bump, migration, manifest entry, ledger, Court module, faction AI, manager/god-controller path, or `PersonRegistry` expansion.

## Validation Evidence

- Focused architecture: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "FullyQualifiedName~Regime_legitimacy_readback_closeout_v261_v268"` passed, 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed, including the full integration suite.

## Closure

Closed as Chain 9 first readback branch audit only. Regime recognition, household compliance, public allegiance, ritual legitimacy, force backing, rebellion-to-polity consequences, dynasty-cycle model, multi-pressure public-life arbitration, and durable regime SocialMemory residue remain future rule-density debts.

## Rollback / Fallback

If the closeout reads too broad, narrow it to the evidence already proven by v253-v260: owner-lane readback tokens, schema neutrality, no prose parsing, no same-month SocialMemory durable residue, and Unity copy-only presentation.
