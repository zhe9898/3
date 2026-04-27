# Court / Regime / Official Pressure Docs

## Goal
补全活世界设计里的政权、官员、朝会、官署执行与其它路线连接方式，让皇权不只是气氛词，也不变成玩家早期的上帝按钮。

## In scope
- 补充 CourtAndThrone / WorldEvents 的未来状态形状。
- 补充官员、朝会、政权合法性、任命、文书、地方执行的压力链。
- 更新路线矩阵、玩家范围、后 MVP 包、路线图与验收标准。
- 保持“路线”作为设计/架构路径，不做玩家职业树或固定身份选择。

## Out of scope
- 不实现新代码。
- 不新增保存 schema。
- 不开启 MVP 皇权/朝会玩法。
- 不设计自由时间线编辑、皇帝按钮或脱离地方社会的大战略层。

## Touched modules
- 文档层涉及未来 `CourtAndThrone` / `WorldEvents` 或等价 pack。
- 当前落地只通过既有 `OfficeAndCareer`、`WorldSettlements`、`PublicLifeAndRumor`、`OrderAndBanditry`、`ConflictAndForce`、`WarfareCampaign`、`TradeAndIndustry`、`PopulationAndHouseholds`、`SocialMemoryAndRelations`、`NarrativeProjection` 的边界描述。

## Schema/save impact
- 无当前 schema 变更。
- 未来任何 court/regime/official actor authority state 必须先有明确 owner、schema version、manifest gating 与 migration tests。

## Determinism risk
- 当前无代码风险。
- 未来风险在于朝会/政权流程容易变成脚本事件池或全局年份触发器；必须保留 seed + state + command 的可重放因果链。

## Milestones
1. 补活世界总图里的 CourtProcess / OfficialActor / RegimeAuthority 骨架。
2. 补社会路径与多路线矩阵里的官员、朝会、政权链。
3. 补 roadmap / post-MVP / player scope / influence 文档。
4. 补验收标准并运行构建确认文档变更不破坏工程。

## Tests
- `dotnet build .\Zongzu.sln -c Debug -m:1`

## Rollback path
- 回退本 ExecPlan 与相关文档补充，不影响代码或存档。
