# Goal

Extend `PublicLifeAndRumor.Lite` from venue/channel metrics into a v4 slice where posted notices, street talk, road reports, and prefecture dispatches can visibly diverge.

# Scope in

- bump `PublicLifeAndRumor` schema from `3` to `4`
- add public-life-owned channel wording fields such as:
  - `OfficialNoticeLine`
  - `StreetTalkLine`
  - `RoadReportLine`
  - `PrefectureDispatchLine`
  - `ContentionSummary`
- keep those lines authoritative inside `PublicLifeAndRumor` only
- upgrade public-life event summaries so they surface the gap between declared order and lived order
- project contention wording into great hall and desk settlement public-life summaries
- add save migration for `PublicLifeAndRumor` `3 -> 4`

# Scope out

- no new prefecture-state or circuit-state module
- no new command owner inside `PublicLifeAndRumor`
- no detached rumor minigame
- no authority rules inside `Presentation.Unity`
- no full temple / guild / granary expansion in this slice

# Affected modules

- `Zongzu.Modules.PublicLifeAndRumor`
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- `Zongzu.Modules.NarrativeProjection` only indirectly through richer public-life event summaries

# Save/schema impact

- `PublicLifeAndRumor` schema `3 -> 4`
- new channel-line wording fields become part of save roundtrip
- no new save namespaces
- no root schema change

# Determinism risk

- low
- all new channel-line descriptors must derive only from deterministic monthly inputs plus published query state
- no new random branch and no cross-module mutation

# Milestones

1. Add v4 public-life wording fields plus `3 -> 4` migration.
2. Extend public-life refresh to derive official notice, street talk, road report, prefecture dispatch, and contention wording.
3. Upgrade public-life event summaries to reflect channel contention.
4. Project contention wording into great hall and desk settlement summaries.
5. Add/update tests, then run focused and solution-level verification.

# Tests to add/update

- `PublicLifeAndRumorModuleTests`
  - v4 channel-line wording populates deterministically
  - official line and street-talk line diverge under stressed county conditions
- `SaveMigrationPipelineTests`
  - `PublicLifeAndRumor` `3 -> 4` migration backfills v4 wording fields
- `M2LiteIntegrationTests`
  - governance/local-conflict path shows public-life contention wording on the desk shell
- `FirstPassPresentationShellTests`
  - great hall and settlement public-life summaries include contention wording

# Rollback / fallback plan

- if player-facing wording gets noisy, keep the v4 fields but reduce the shell to showing only `ContentionSummary`
- if migration becomes brittle, backfill conservative defaults and defer more ornate wording to a follow-up

# Open questions

- whether future prefecture/circuit work should split `PrefectureDispatchLine` into prefecture versus circuit voices
- whether later public-life commands should explicitly target a venue rather than only a settlement
