# Chain 6 Thin Slice: DisasterDeclared → DisorderSpike → PublicLife

> 规格：`RENZONG_PRESSURE_CHAIN_SPEC.md` §链6（灾荒-赈济-秩序）
> 日期：2026-04-23
> 范围：M2-lite，最薄可行。只打通一骨，不实现赈济决策、市场恐慌、移民逻辑。

---

## 链定义

```
WorldSettlements (monthly pass)
  └── WorldSettlements.DisasterDeclared { EntityKey: settlementId, Metadata: cause/disasterKind/severity/floodRisk/embankmentStrain }
      └──→ OrderAndBanditry (HandleEvents, Round 0)
            解析 settlementId，按 metadata severity 增加 disorder
            若 old < 50 && new >= 50：
              发出 OrderAndBanditry.DisorderSpike { EntityKey: settlementId, Metadata: cause/sourceEventType/disorderDelta/... }
      └──→ PublicLifeAndRumor (HandleEvents, Round 1)
            解析 settlementId，StreetTalkHeat += 12
            LastPublicTrace = metadata cause-aware trace
```

**Cadence**: 同月解决（bounded drain，2 跳）。
**Scope**: settlement-scoped（typed-id EntityKey）。

---

## 模块边界

| 模块 | 拥有状态 | 行为 |
|------|----------|------|
| WorldSettlements | SeasonBandData.FloodRisk / EmbankmentStrain / LastDeclaredFloodDisasterBand | 判定灾荒条件，发出 DisasterDeclared，并用水位防止重复宣告 |
| OrderAndBanditry | SettlementDisorderState.DisorderPressure | 消费 DisasterDeclared，更新 disorder，可能发 DisorderSpike |
| PublicLifeAndRumor | SettlementPublicLifeState.StreetTalkHeat | 消费 DisorderSpike，更新 heat + trace |

---

## 实施步骤

### Milestone 1: WorldSettlements 发射 DisasterDeclared

1. `WorldSettlementsModule.PublishedEvents` 添加 `WorldSettlementsEventNames.DisasterDeclared`。
2. `SeasonBandAdvancer` 增加灾荒判定逻辑：
   - `FloodRisk >= 70` → `severity = "flood-severe"`
   - `FloodRisk >= 50` → `severity = "flood-moderate"`
   - 薄切片暂只实现 flood；drought/locust/epidemic 标记 ⏳ NOT IMPLEMENTED
3. `EmitDisasterIfDue` 逻辑：
   - `FloodRisk >= 50` 时发射
   - severity: `flood-severe` (>=70) / `flood-moderate` (>=50)
   - 通过 `DomainEvent.Metadata` 携带 cause/disasterKind/severity/floodRisk/embankmentStrain
   - 通过 `LastDeclaredFloodDisasterBand` 防止同一活动水患每月重复宣告；低于 50 后清水位
   - EntityKey 优先 CanalJunction，其次 CountySeat
4. 单元测试：
   - `RunMonth_FloodDisasterDeclared_CarriesMetadataAndLatchesBand`
   - `RunMonth_ActiveFloodDisasterBand_DoesNotRedeclare`
   - `RunMonth_FloodRiskFallsBelowDisasterBand_ClearsDisasterWatermark`

### Milestone 2: OrderAndBanditry 消费 DisasterDeclared

1. `OrderAndBanditryModule.ConsumedEvents` 添加 `WorldSettlementsEventNames.DisasterDeclared`。
2. `HandleEvents` / `DispatchWorldPulseEvents` 增加 case：
   - `ApplyDisasterDisorderPressure(scope, domainEvent)`
   - 解析 settlementId
   - `Metadata[severity]` 映射：`flood-severe` → +15，`flood-moderate` → +8
   - 只影响指定 settlement（filter before mutation）
   - 若越 50 阈值，发带 cause/source/severity metadata 的 `DisorderSpike`
3. 单元测试：
   - `DisasterDeclared_Severe_RaisesDisorder`
   - `DisasterDeclared_CrossesThreshold_EmitsDisorderSpike`
   - `DisasterDeclared_OffScopeSettlement_DoesNotAffect`
   - `DisasterDeclared_TextSeverityWithoutMetadata_IsNoOp`

### Milestone 3: PublicLifeAndRumor cause-aware trace ✅

1. `ApplyDisorderSpikeHeat` 不再硬编码 `"徭役加急"` ✅
2. 改为从 `domainEvent.Metadata[DomainEventMetadataKeys.Cause]` 提取 cause hint：
   - `corvee` → `"徭役加急，街面不安，街谈热度升至{X}。"`
   - `amnesty` → `"大赦释囚，街面不安，街谈热度升至{X}。"`
   - `disaster` + `disasterKind=flood` → `"水患告急，街面不安，街谈热度升至{X}。"`
   - 否则 → `"街面失序，人心浮动，街谈热度升至{X}。"`
3. 新增 `DisorderSpike_CauseHintComesFromMetadata_NotSummary`，证明 Summary 不能误导投影原因 ✅

### Milestone 4: Scheduler 级 end-to-end 测试 ✅

1. 新建 `DisasterDisorderPublicLifeChainTests.cs`（2 个测试全部通过）：
   - 配置 WorldSettlements + OrderAndBanditry + PublicLifeAndRumor（+ 必要 substrate）
   - seed WorldSettlements：某 settlement `FloodRisk = 75`
   - seed OrderAndBanditry：同一 settlement `DisorderPressure = 42`
   - 运行 `AdvanceOneMonth`
   - 断言：
     - `DomainEvents` 包含 `DisasterDeclared`
     - `DomainEvents` 包含 `DisorderSpike`
     - PublicLife 该 settlement `StreetTalkHeat >= 50`
     - PublicLife 该 settlement `LastPublicTrace` 含灾荒相关字样
     - 另一 settlement 不受影响（off-scope negative assertion）

---

## 保存/模式影响

- `WorldSettlements` 新增 `LastDeclaredFloodDisasterBand`，schema `6 -> 7`。
- 默认迁移把旧档水位初始化为 `0`；`DisorderPressure`、`StreetTalkHeat` 已存在，无需迁移。

## 确定性风险

- `DisasterDeclared` 的 EntityKey 使用 typed-id 字符串，稳定。
- `DisasterDeclared` 和 `DisorderSpike` 的规则因果使用 metadata，不解析 Summary。
- settlement 遍历顺序用 `SettlementId.Value` 排序。
- 无随机数调用（severity 由状态阈值决定）。

## 规范检查清单

- [x] Cadence 明确：同月 bounded drain
- [x] 每轮只处理 fresh events
- [x] 模块顺序稳定
- [x] 发出的事件在 PublishedEvents 中
- [x] 处理的事件在 ConsumedEvents 中
- [x] 事件名使用 Contracts 常量
- [x] Chain test 使用 real scheduler
- [x] Scope 分类明确（settlement-scoped）
- [x] EntityKey 含稳定 metadata
- [x] Consumer 在 mutation 前 filter
- [x] Off-scope negative assertion
- [x] 可见结果从最终 state / DomainEvent 断言
- [x] Generic downstream event 有 cause discipline（DisorderSpike trace 不再硬绑徭役）
- [x] Persistent upstream band has a watermark and does not repeat monthly at the same active band
