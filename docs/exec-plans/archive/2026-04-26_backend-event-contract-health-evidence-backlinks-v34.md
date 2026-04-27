# Backend Contract Health v34: Classification Evidence Backlinks

## Goal
- Extend the v32-v33 backend event-contract diagnostics so each visible classification can show a structured owner/module lane and an evidence backlink.
- Keep this as diagnostic/readback evidence only. V34 is not a new gameplay rule, command system, event pool, pressure formula, scheduler phase, projection surface, or owner-lane ledger.
- Preserve `DomainEvent` as a deterministic fact-propagation tool after module rules resolve, not the design body of a pressure chain.

## Scope In
- Add runtime-only diagnostic formatting for event-contract classifications:
  - `owner=<module lane>`
  - `evidence=<doc/test backlink>`
  - existing `contract=<kind>` and rationale text
- Ensure the owner lane is read from the structured event key/module prefix, never from `DomainEvent.Summary`, report prose, receipt prose, or presentation text.
- Add focused integration tests proving the formatted classification readback includes owner and evidence backlinks.
- Add architecture guard coverage so the diagnostic remains test-only and no schema/save path appears.
- Update docs with v34 evidence and no schema/migration impact.

## Scope Out
- No new event handlers, module state, event ledger, owner-lane ledger, cooldown ledger, save field, migration, or schema version bump.
- No new player-facing UI/Unity shell behavior. Developer diagnostics may include evidence backlinks; player surfaces must not read these labels as authority.
- No Application-owned command result calculation, no UI/Unity mutation path, no SocialMemory write path, no `PersonRegistry` expansion, and no manager/god-controller layer.
- No parsing of `DomainEvent.Summary`, `LastInterventionSummary`, `LastLocalResponseSummary`, receipt prose, SocialMemory prose, or diagnostic report prose.

## Affected Modules
- `tests/Zongzu.Integration.Tests`: ten-year simulation health diagnostics and focused evidence-backlink tests.
- `tests/Zongzu.Architecture.Tests`: source guards for runtime-only diagnostics and no summary parsing.
- `docs`: acceptance, boundaries, schema/no-migration, simulation, UI/presentation, and topology notes.

## Save / Schema Impact
- V34 has no persisted state, no root save-version bump, no module schema bump, no migration, no save-manifest change, no save roundtrip behavior, and no runtime diagnostic ledger.
- If implementation requires persisting event-health evidence, owner-lane links, or report state, stop and convert this into a schema/migration plan before code changes.

## Determinism Risk
- Low. V34 only changes integration-test diagnostic formatting and assertions after the deterministic ten-year run has completed.
- It reads structured event keys, module declarations, and diagnostic consumer counts; it does not feed back into simulation, command routing, save/load, projection authority, or Unity.

## Milestones
1. Add this ExecPlan with no-schema target.
2. Add owner/evidence backlink formatting to event-contract health diagnostics.
3. Add focused integration tests for `owner=` and `evidence=` readback.
4. Update architecture guard and docs.
5. Run build, focused integration/architecture/Unity tests, `git diff --check`, and full no-build solution tests.
6. Commit and push `codex/backend-contract-health-v34`.

## Tests To Add / Update
- focused integration:
  - diagnostic readback for at least one emitted-without-authority-consumer entry includes `owner=<module>` and `evidence=<doc/test backlink>`;
  - diagnostic readback for at least one declared-but-not-emitted entry includes `owner=<module>` and `evidence=<doc/test backlink>`;
  - current diagnostic classifications still reject unclassified debt.
- architecture:
  - diagnostics remain runtime/test-only;
  - no `DomainEvent.Summary` parsing;
  - no schema/migration/save ledger tokens;
  - no Application/UI/Unity authority drift, no forbidden manager/controller names, no `PersonRegistry` expansion.

## Rollback / Fallback Plan
- If the evidence backlinks create noisy or unstable report output, keep the no-unclassified gate and move backlinks to a focused assertion-only helper.
- If any current event key cannot produce a structured owner lane from its module prefix, classify that as a diagnostic alignment bug before accepting the health report as evidence.
- If persisted state appears necessary, stop and document owning module schema/migration impact before writing code.

## Open Questions
- Which `FutureContract` entries should graduate into real owner-module handlers after V34. This remains a later backend pass.

## Evidence Checklist
- [x] ExecPlan created
- [x] owner/evidence backlink formatting added
- [x] focused integration tests added
- [x] architecture guard updated
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration tests
- [x] focused architecture tests
- [x] focused Unity presentation tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] commit and push

## Validation Evidence
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~EventContractHealth|FullyQualifiedName~CampaignEnabledStressSandbox_TenYearHealthReport"` passed 5 tests; the 120-month health report now displays `owner=<module>` and `evidence=<doc/test backlink>` for classified event-contract debt.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Event_contract_health|FullyQualifiedName~summary|FullyQualifiedName~forbidden_manager|FullyQualifiedName~PersonRegistry|FullyQualifiedName~Presentation_Unity"` passed 8 tests.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 31 tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed; full integration count is now 129 tests.
- Commit pushed to `origin/codex/backend-contract-health-v34`.
