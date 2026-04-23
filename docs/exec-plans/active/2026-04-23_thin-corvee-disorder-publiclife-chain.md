# Thin Chain: CorveeWindowChanged → DisorderSpike → PublicLife

> 目标：按 skill `pressure-chains-and-causality.md` 的 15 条 guardrails，实现一条最薄的新压力链。
> 日期：2026-04-23
> 范围：M2-lite，不涉及 P5+ 事件

---

## 链定义

```
WorldSettlements (monthly pass / seasonal band)
  └── WorldSettlements.CorveeWindowChanged { EntityKey: CorveeWindow enum }
      └──→ OrderAndBanditry (HandleEvents)
            解析 CorveeWindow 状态（Pressed / Emergency）
            对所有 settlement 增加 disorder pressure
            若 settlement disorder 从 <50 升至 ≥50：
              发出 OrderAndBanditry.DisorderSpike { EntityKey: settlementId }
      └──→ PublicLifeAndRumor (HandleEvents, round N+1)
            解析 settlementId
            对应 settlement 的 StreetTalkHeat += 12
            LastPublicTrace = "徭役加急，街面不安，街谈热度升至{X}。"
```

**Cadence**: 同月解决（bounded event drain）。
`CorveeWindowChanged` 在 WorldSettlements 的 month pass 中发出，
OrderAndBanditry 在 Round 0 处理并发出 `DisorderSpike`，
PublicLifeAndRumor 在 Round 1 消费。

**Scope**: 
- `CorveeWindowChanged` — global phase（symbolic EntityKey，所有 settlements 受影响）
- `DisorderSpike` — settlement-scoped（typed-id EntityKey）
- PublicLife mutation — settlement-scoped（filter by settlementId）

**Player Window**: 暂无。本链是 ambient pressure，M2 阶段只产生公共生活热度。
Post-MVP 可接入 `FundLocalWatch` 或 `PetitionYamen`。

**Ignore Consequence**: 若玩家不介入，StreetTalkHeat 持续累积，
下月可能触发 `StreetTalkSurged`（已有事件，PublicLife 内部逻辑）。

---

## 模块边界检查

| 模块 | 拥有状态 | 行为 |
|------|----------|------|
| WorldSettlements | SeasonBand.CorveeWindow | 发出 CorveeWindowChanged |
| OrderAndBanditry | SettlementDisorderState.DisorderPressure | 消费 CorveeWindowChanged，更新 disorder，可能发出 DisorderSpike |
| PublicLifeAndRumor | SettlementPublicLifeState.StreetTalkHeat | 消费 DisorderSpike，更新 heat |

**禁止**: 
- OrderAndBanditry 不得直接写 PublicLife 状态
- PublicLifeAndRumor 不得直接写 OrderAndBanditry 状态
- UI / NarrativeProjection 不得产生权威状态变化

---

## 实施步骤

### Milestone 1: OrderAndBanditry handler（owner: OrderAndBanditry）✅

1. ~~确认 `OrderAndBanditry.ConsumedEvents` 已包含 `WorldSettlementsEventNames.CorveeWindowChanged`~~
   - `OrderAndBanditryModule.ConsumedEventNames` 未显式包含 `CorveeWindowChanged`，但 `HandleEvents` 通过 `DispatchWorldPulseEvents` 处理所有 `scope.Events` 中的事件，不依赖 `ConsumedEvents` 过滤（scheduler `HandleMonthEndEvents` 只检查 `ConsumedEvents.Count > 0`）。
2. ~~确认 `OrderAndBanditry.PublishedEvents` 已包含 `OrderAndBanditryEventNames.DisorderSpike`~~
   - 已确认 `PublishedEvents` 包含 `DisorderSpike`。
3. 在 `DispatchWorldPulseEvents` 中实现 `CorveeWindowChanged` case：✅
   - 解析 `CorveeWindow` 从 `domainEvent.EntityKey`
   - `Pressed` → disorder +8；`Emergency` → disorder +15
   - 遍历所有 settlement，应用 delta
   - 若 `oldDisorder < 50 && newDisorder >= 50`，发出 `DisorderSpike`
   - `EntityKey` = `settlementId.Value.ToString()`
4. 添加单元测试：✅
   - `CorveeWindowChanged_Pressed_RaisesDisorder`
   - `CorveeWindowChanged_Emergency_CrossesThreshold_EmitsDisorderSpike`
   - `CorveeWindowChanged_Quiet_DoesNothing`

**文件**: `src/Zongzu.Modules.OrderAndBanditry/OrderAndBanditryModule/OrderAndBanditryModule.cs`（`DispatchWorldPulseEvents` + `ApplyCorveeWindowDisorderPressure`）
**测试**: `tests/Zongzu.Modules.OrderAndBanditry.Tests/CorveeDisorderHandlerTests.cs`

### Milestone 2: PublicLifeAndRumor handler（owner: PublicLifeAndRumor）✅

1. `PublicLifeAndRumor.ConsumedEvents` 中添加 `OrderAndBanditryEventNames.DisorderSpike` ✅
2. `HandleEvents` 中实现 `DisorderSpike` case：✅
   - 解析 `settlementId` 从 `domainEvent.EntityKey`
   - 找到对应 `SettlementPublicLifeState`
   - `StreetTalkHeat = Math.Clamp(StreetTalkHeat + 12, 0, 100)`
   - `LastPublicTrace = "徭役加急，街面不安，街谈热度升至{heat}。"`
3. 添加单元测试：✅
   - `DisorderSpike_ForKnownSettlement_RaisesStreetTalkHeat`
   - `DisorderSpike_ForUnknownSettlement_IsNoOp`
   - `DisorderSpike_ForMatchedSettlement_DoesNotAffectOtherSettlements`（off-scope negative assertion）

**文件**: `src/Zongzu.Modules.PublicLifeAndRumor/PublicLifeAndRumorModule/PublicLifeAndRumorModule.cs`
**测试**: `tests/Zongzu.Modules.PublicLifeAndRumor.Tests/DisorderSpikeHandlerTests.cs`

### Milestone 3: Scheduler 级 end-to-end 测试 ✅

1. 新建 `CorveeDisorderPublicLifeChainTests`：✅
   - 配置 PersonRegistry + WorldSettlements + PopulationAndHouseholds + FamilyCore + SocialMemoryAndRelations + OrderAndBanditry + PublicLifeAndRumor
   - 初始化 WorldSettlementsState，设置 `CurrentSeason.AgrarianPhase = Tending`（month 8 进入 Harvest → CorveeWindow.Pressed）
   - 运行 `MonthlyScheduler.AdvanceOneMonth`
   - 断言：
     - `result.DomainEvents` 包含 `CorveeWindowChanged(Pressed)`
     - `result.DomainEvents` 包含 `DisorderSpike`（settlement 1，threshold crossing）
     - `result.DomainEvents` 不包含 `DisorderSpike`（settlement 2，below threshold）
     - PublicLife settlement 1 的 `StreetTalkHeat > 30` 且 `LastPublicTrace` 包含 `徭役加急`
     - PublicLife settlement 2 的 `LastPublicTrace` 不包含 `徭役加急`（off-scope negative assertion）

2. 同时保留逐步调用测试 `ThinCorveeChain_CorveePressed_DisorderSpike_PublicLifeReacts`：✅
   - 直接按模块顺序调用，不经过 scheduler
   - 验证事件传播逻辑与 handler 行为

**文件**: `tests/Zongzu.Integration.Tests/CorveeDisorderPublicLifeChainTests.cs`

### Milestone 4: NarrativeProjection 注册 ✅

1. `NarrativeProjectionModule.Surfaces.cs` — `DetermineTier`：
   - `OrderAndBanditryEventNames.DisorderSpike` → `NotificationTier.Urgent` ✅
2. `NarrativeProjectionModule.Surfaces.cs` — `BuildTitle`：
   - `OrderAndBanditryEventNames.DisorderSpike` → `"失序骤起"` ✅
3. `NarrativeProjectionModule.NextSteps.cs` — `BuildWhatNext`：
   - `OrderAndBanditryEventNames.DisorderSpike` → cause-neutral settlement-disorder guidance ✅
4. `DetermineSurface` 已覆盖 `OrderAndBanditry` → `NarrativeSurface.DeskSandbox`，无需修改。

**文件**: `src/Zongzu.Modules.NarrativeProjection/NarrativeProjectionModule/NarrativeProjectionModule.Surfaces.cs`
**文件**: `src/Zongzu.Modules.NarrativeProjection/NarrativeProjectionModule/NarrativeProjectionModule.NextSteps.cs`

---

## 保存/模式影响

- `OrderAndBanditryState` 的 `SettlementDisorderState.DisorderPressure` 已在现有 schema 中，无新增字段
- `PublicLifeAndRumorState` 的 `SettlementPublicLifeState.StreetTalkHeat` 已在现有 schema 中，无新增字段
- 无 save 格式变更

## 确定性风险

- `CorveeWindowChanged` 的 EntityKey 使用 enum name（"Pressed"/"Emergency"/"Quiet"），是稳定的 symbolic key
- Settlement 遍历顺序使用 `SettlementId.Value` 排序，确保确定性
- 无随机数调用

## 规范检查清单

- [x] Cadence 明确：同月解决（bounded drain）
- [x] 每轮只处理 fresh events
- [x] 模块顺序稳定（scheduler 固定）
- [x] 发出的事件在 PublishedEvents 中
- [x] 处理的事件在 ConsumedEvents 中
- [x] 事件名使用 Contracts 常量
- [x] Chain test 使用 real scheduler
- [x] Scope 分类明确（global → settlement-scoped）
- [x] EntityKey 包含稳定 metadata
- [x] Consumer 在 mutation 前 filter（settlement-scoped）
- [x] Global effect 是有意的（CorveeWindowChanged 全局）
- [x] Off-scope negative assertion（unaffected settlement 不变 / 无 `徭役加急` trace）
- [x] 可见结果从最终 state / DomainEvent 断言
- [x] Drain cap 不触发（本链只有 2 跳）
