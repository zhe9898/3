# Chain 3 Thin Slice: ExamPassed → ClanPrestigeAdjusted

> 规格：`RENZONG_PRESSURE_CHAIN_SPEC.md` §链3（科举-教育-家户-官员-公共生活）
> 原则：最薄可行，不批量实现所有事件，只打通一条主骨
> Skills: `pressure-chains-and-causality.md` 15 guardrails

---

## 链定义（精简版）

```
EducationAndExams (monthly, exam window)
  └── ExamPassed { EntityKey: personId }
      └──→ FamilyCore (HandleEvents)
            解析 personId → 找到所属 clan
            clan.Prestige += 5
            clan.MarriageAllianceValue += 3
            发出 ClanPrestigeAdjusted { EntityKey: clanId }
```

**Cadence**: 同月解决。EducationAndExams 在 month pass 中发出 `ExamPassed`，
FamilyCore 在 `HandleMonthEndEvents` Round 0 中消费并发出 `ClanPrestigeAdjusted`。

**Scope**: 
- `ExamPassed` — person-scoped（`EntityKey = personId`）
- `ClanPrestigeAdjusted` — clan-scoped（`EntityKey = clanId`）
- FamilyCore mutation — clan-scoped（filter by person→clan lookup）

**Player Window**: 暂无。本链是 ambient prestige pressure，M2 阶段只产生宗族声望。
Post-MVP 可接入 `FundStudy`、`RecommendSomeone`。

**Ignore Consequence**: 若玩家不介入，clan prestige 持续累积，
影响婚议价值、宗族议价能力和公共合法性。

---

## 模块边界检查

| 模块 | 拥有状态 | 行为 |
|------|----------|------|
| EducationAndExams | EducationPersonState.StudyProgress/ExamResult | 发出 ExamPassed（带 personId EntityKey） |
| FamilyCore | ClanStateData.Prestige/MarriageAllianceValue | 消费 ExamPassed，更新 clan 声望，发出 ClanPrestigeAdjusted |

**禁止**: 
- EducationAndExams 不得直接写 FamilyCore 状态
- FamilyCore 不得直接写 EducationAndExams 状态
- UI / NarrativeProjection 不得产生权威状态变化

---

## 实施步骤

### Milestone 1: EducationAndExams 加 EntityKey ✅

1. 修改 `RunMonth` 中 `scope.Emit(ExamPassed, ...)`：加上 `EntityKey = student.PersonId.Value.ToString()`
2. 同样修改 `ExamFailed` 和 `StudyAbandoned`（为后续扩展留接口）

**文件**: `src/Zongzu.Modules.EducationAndExams/EducationAndExamsModule.cs`

### Milestone 2: FamilyCore 消费 ExamPassed ✅

1. `ConsumedEvents` 添加 `EducationAndExamsEventNames.ExamPassed`
2. `HandleEvents` 中添加 `DispatchExamResultEvents` 方法
3. 实现 `ApplyExamPassPrestige`：
   - 解析 `EntityKey` → `PersonId`
   - 从 `scope.State.People` 中找到该 person
   - 从 `scope.State.Clans` 中找到该 person 的 clan
   - `clan.Prestige = Math.Clamp(clan.Prestige + 5, 0, 100)`
   - `clan.MarriageAllianceValue = Math.Clamp(clan.MarriageAllianceValue + 3, 0, 100)`
   - 发出 `ClanPrestigeAdjusted`
   - `EntityKey = clanId.Value.ToString()`

**文件**: `src/Zongzu.Modules.FamilyCore/FamilyCoreModule.cs`

### Milestone 3: 测试 ✅

1. FamilyCore focused handler tests：`ExamResultHandlerTests.cs`
   - `ExamPassed_RaisesClanPrestigeAndMarriageValue`
   - `ExamPassed_OffScopeClan_DoesNotChange`（off-scope negative assertion）
   - `ExamPassed_UnknownPerson_IsNoOp`
2. Scheduler end-to-end test：`ExamPrestigeChainTests.cs`
   - `ExamPass_ThinChain_RealScheduler_DrainsIntoClanPrestige`
   - 配置 PersonRegistry + WorldSettlements + PopulationAndHouseholds + FamilyCore + SocialMemoryAndRelations + EducationAndExams
   - 运行 scheduler 到 exam month (3)
   - 断言 `ExamPassed` 事件存在（带 personId EntityKey）
   - 断言 `ClanPrestigeAdjusted` 事件存在
   - 断言 clan prestige 从 50 升至 55

### Milestone 4: NarrativeProjection 注册 ✅

1. `DetermineTier`: `ClanPrestigeAdjusted` → `NotificationTier.Consequential`
2. `BuildTitle`: `ClanPrestigeAdjusted` → `"门望有变"`
3. `BuildWhatNext`: `"回看宗族声势与婚议去留，再定是借势扩张还是稳住根基。"`
4. `DetermineSurface`: `FamilyCore` 已是 `NarrativeSurface.AncestralHall`，无需修改

**文件**: `src/Zongzu.Modules.NarrativeProjection/NarrativeProjectionModule/NarrativeProjectionModule.Surfaces.cs`
**文件**: `src/Zongzu.Modules.NarrativeProjection/NarrativeProjectionModule/NarrativeProjectionModule.NextSteps.cs`

---

## 保存/模式影响

- 无新增 state 字段
- 无 save 格式变更
- 仅事件 EntityKey 补充

## 确定性风险

- `ExamPassed` EntityKey 使用 `PersonId.Value`，稳定整数键
- Clan 查找顺序按 `ClanId.Value` 排序（若需遍历）
- 无新增随机数调用

## 规范检查清单

- [x] Cadence 明确：同月解决（bounded drain）
- [x] 每轮只处理 fresh events
- [x] 发出的事件在 PublishedEvents 中
- [x] 处理的事件在 ConsumedEvents 中
- [x] 事件名使用 Contracts 常量
- [x] Chain test 使用 real scheduler
- [x] Scope 分类明确（person → clan）
- [x] EntityKey 包含稳定 metadata
- [x] Consumer 在 mutation 前 filter（person→clan lookup）
- [x] Off-scope negative assertion（unaffected clan unchanged）
- [x] 可见结果从最终 state / DomainEvent 断言
