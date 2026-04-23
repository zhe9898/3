# Chain 7/8/9 Thin Slice: Office-Court-Regime Pressure Chains

> 规格：`RENZONG_PRESSURE_CHAIN_SPEC.md` §链7/8/9
> 日期：2026-04-23
> 范围：M2-lite，最薄可行。避开 WarfareCampaign（MVP 排除）。

---

## 链7：ClerkCaptureDeepened → PublicLife

```
OfficeAndCareer (monthly)
  └── ClerkCaptureDeepened { EntityKey: settlementId }
      └──→ PublicLifeAndRumor
            StreetTalkHeat += 12
            LastPublicTrace = "书吏坐大，街谈渐热。"
```

**Trigger**: `RunMonth` 检查 jurisdiction：`ClerkDependence >= 30 && PetitionBacklog >= 15`

---

## 链8：CourtAgendaPressureAccumulated → PolicyWindowOpened

```
WorldSettlements (monthly)
  └── CourtAgendaPressureAccumulated { EntityKey: "court" }
      └──→ OfficeAndCareer
            发出 PolicyWindowOpened { EntityKey: settlementId }
```

**Trigger**: `RunMonth` 检查 `Imperial.MandateConfidence < 40`

---

## 链9：RegimeLegitimacyShifted → OfficeDefected

```
WorldSettlements (monthly)
  └── RegimeLegitimacyShifted { EntityKey: "regime" }
      └──→ OfficeAndCareer
            发出 OfficeDefected { EntityKey: personId }
```

**Trigger**: `RunMonth` 检查 `Imperial.MandateConfidence < 25`

---

## 实施步骤

### Milestone 1: 链7 — OfficeAndCareer 发射 ClerkCaptureDeepened ✅
- `OfficeAndCareerModule.PublishedEvents` 添加 `ClerkCaptureDeepened`
- `RunMonth` 中检查 jurisdiction 条件，发射事件

### Milestone 2: 链7 — PublicLifeAndRumor 消费 ClerkCaptureDeepened ✅
- `PublicLifeAndRumorModule.ConsumedEvents` 添加 `ClerkCaptureDeepened`
- `HandleEvents` 中增加 case，增加 StreetTalkHeat

### Milestone 3: 链8 — WorldSettlements 发射 CourtAgendaPressureAccumulated ✅
- `WorldSettlementsModule.PublishedEvents` 添加 `CourtAgendaPressureAccumulated`
- `RunMonth` 中检查 `MandateConfidence`，发射事件
- 含 edge-detection watermark (`LastCourtAgendaPressureDeclared`)

### Milestone 4: 链8 — OfficeAndCareer 消费 CourtAgendaPressureAccumulated ✅
- `OfficeAndCareerModule.ConsumedEvents` 添加 `CourtAgendaPressureAccumulated`
- `HandleEvents` 中发出 `PolicyWindowOpened`
- `PublishedEvents` 添加 `PolicyWindowOpened`

### Milestone 5: 链9 — WorldSettlements 发射 RegimeLegitimacyShifted ✅
- `WorldSettlementsModule.PublishedEvents` 添加 `RegimeLegitimacyShifted`
- `RunMonth` 中检查 `MandateConfidence`，发射事件
- 含 edge-detection watermark (`LastRegimeLegitimacyShiftDeclared`)

### Milestone 6: 链9 — OfficeAndCareer 消费 RegimeLegitimacyShifted ✅
- `OfficeAndCareerModule.ConsumedEvents` 添加 `RegimeLegitimacyShifted`
- `HandleEvents` 中发出 `OfficeDefected`
- `PublishedEvents` 添加 `OfficeDefected`

### Milestone 7: 测试 ✅
- `WorldSettlementsModuleTests` / `Chain789EmitterTests`: 8 个发射端测试（阈值触发 / 不触发 / 水印防重 / 水印清除 × 链8/9）
- `PublicLifeAndRumorModuleTests` / `Chain7ClerkCaptureTests`: 4 个消费端测试（热度增加 / clamp / off-scope 不受影响 / 非法 EntityKey 容错）
- `OfficeAndCareer` Chain789 测试: 13 个测试全部通过
