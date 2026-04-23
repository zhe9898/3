# Chain 5 Thin Slice: FrontierStrainEscalated → OfficialSupplyRequisition → HouseholdBurden

> 规格：`RENZONG_PRESSURE_CHAIN_SPEC.md` §链5（边防-军需-家户-市场-衙门）
> 日期：2026-04-23
> 范围：M2-lite，避开 WarfareCampaign（MVP 排除），只取最薄一骨

---

## 链定义

```
WorldSettlements (xun/month)
  └── WorldSettlements.FrontierStrainEscalated { EntityKey: settlementId }
      └──→ OfficeAndCareer (HandleEvents, Round 0)
            解析 settlementId
            发出 OfficeAndCareer.OfficialSupplyRequisition { EntityKey: settlementId }
      └──→ PopulationAndHouseholds (HandleEvents, Round 1)
            解析 settlementId
            对该 settlement 的 households 增加 burden / distress
```

**Cadence**: 同月 bounded drain（2 跳）。
**Scope**: settlement-scoped（typed-id EntityKey）。

---

## 模块边界

| 模块 | 拥有状态 | 行为 |
|------|----------|------|
| WorldSettlements | SeasonBandData.FrontierPressure, LastDeclaredFrontierStrainBand | 推进 frontier 压力；当压力跨入更高 frontier band 时，选择一个受影响 settlement 发射 FrontierStrainEscalated |
| OfficeAndCareer | JurisdictionAuthorityState | 消费 settlement-scoped FrontierStrainEscalated，只对匹配 jurisdiction 发出 OfficialSupplyRequisition |
| PopulationAndHouseholds | PopulationHouseholdState | 消费 OfficialSupplyRequisition，更新 household burden；distress 跨 80 时发 receipt |

---

## 实施步骤

### Milestone 1: WorldSettlements 发射 FrontierStrainEscalated ✅

1. `SeasonBandData` 添加 `int FrontierPressure { get; set; }` ✅
2. `SeasonBandSnapshot`（Contracts）同步添加 `int FrontierPressure { get; init; }` ✅
3. `WorldSettlementsModule.CloneSeason` 同步映射 ✅
4. `SeasonBandAdvancer.AdvanceXun` 中推进 `FrontierPressure`（秋冬 +6~18，春夏 -6~+3）✅
5. `WorldSettlementsModule.RunMonth` 中检查 `FrontierPressure` band crossing，发射 settlement-scoped `FrontierStrainEscalated` ✅
6. `WorldSettlementsModule.PublishedEvents` 添加 `FrontierStrainEscalated` ✅
7. `LastDeclaredFrontierStrainBand` 防止持续高压每月重复征发 ✅

### Milestone 2: OfficeAndCareer 消费 FrontierStrainEscalated ✅

1. `OfficeAndCareerModule.ConsumedEvents` 添加 `FrontierStrainEscalated` ✅
2. `HandleEvents` 中新增 frontier dispatch ✅
3. 解析 settlementId，找到对应 jurisdiction；无匹配辖区则 no-op ✅
4. 只为匹配 jurisdiction 发出 `OfficialSupplyRequisition`，不向所有辖区 fan-out ✅
5. `OfficeAndCareerModule.PublishedEvents` 添加 `OfficialSupplyRequisition` ✅

### Milestone 3: PopulationAndHouseholds 消费 OfficialSupplyRequisition ✅

1. `PopulationAndHouseholdsModule.ConsumedEvents` 添加 `OfficialSupplyRequisition` ✅
2. `HandleEvents` 中新增 office-supply dispatch ✅
3. 解析 settlementId，对该 settlement 的 households +distress / +debtPressure ✅
4. 若某 household 的 distress 从 80 以下跨到 80+：发出 `HouseholdBurdenIncreased`，并携带 cause/source/settlement 元数据 ✅

### Milestone 4: 测试 ✅

1. Handler 测试（全部通过）：
   - `FrontierSupplyHandlerTests.cs`（OfficeAndCareer，5 个测试）
   - `OfficialSupplyBurdenHandlerTests.cs`（PopulationAndHouseholds，4 个测试）
2. Scheduler 级 end-to-end（全部通过）：
   - `FrontierSupplyHouseholdChainTests.cs`（3 个测试）
3. WorldSettlements 单模块测试：
   - settlement-scoped frontier event
   - active band 不重复声明
   - pressure 跌出 band 后清水位

---

## Schema / Save 影响

- `SeasonBandData.FrontierPressure` 新增 int 字段
- `WorldSettlementsModule.ModuleSchemaVersion` 从 7 → 8
- `SeasonBandSnapshot.FrontierPressure` 新增 int 字段（Contracts，向后兼容）
- `WorldSettlementsState.LastDeclaredFrontierStrainBand` 新增 int 字段
- save 迁移：旧存档 FrontierPressure = 0（默认值安全），`LastDeclaredFrontierStrainBand = 0`

## 确定性风险

- FrontierPressure 推进使用 deterministic random
- Settlement 遍历顺序稳定
- EntityKey 使用 typed-id 字符串
