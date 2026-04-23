# Zongzu.Modules.PersonRegistry

Kernel-layer person identity anchor. Identity-only by design.

## 所有权

- `PersonId`、`DisplayName`、`BirthDate`、`Gender`
- `LifeStage`（Infant / Child / Youth / Adult / Elder / Deceased）
- `IsAlive`
- `FidelityRing`（Core / Local / Regional）

**不拥有**人物的性格、能力、健康细节、亲属关系、社会位置、活动 —— 这些由领域模块各自持有。见 `docs/PERSON_OWNERSHIP_RULES.md`、`docs/MODULE_BOUNDARIES.md §0`。

## 时间契约

`month` / `SimulationPhase.Prepare`：
- 年龄推进与生命阶段再判定
- 领域模块在后续 Phase 读到的都是当前生命阶段

## 事件

- **消费**：`ClanMemberDied` / `DeathByIllness` / `DeathByViolence`（因果特定死亡事件，由领域模块发出）
- **发出**：`PersonCreated` / `PersonDeceased` / `FidelityRingChanged`

`PersonDeceased` 是规范的"这个人死了"信号 —— 投影层只需要订这个。

## Design Guardrail

**不要**在这里加域字段。如果某字段回答的是"这个人在做什么 / 感受什么 / 能做什么 / 跟谁有关系"，它属于领域模块而不是 PersonRegistry。
