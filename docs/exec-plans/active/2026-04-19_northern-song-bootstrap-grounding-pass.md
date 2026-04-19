## Goal
把 `SimulationBootstrapper` 里的起盘 seed / fixture 文本收成 `Northern Song-inspired` 的县域、埠口、漕路语感，避免玩家一进世界就看到英文 fixture 或现代测试口气。

## In Scope
- `SimulationBootstrapper` 里的 settlement / academy / market / route / household 命名
- bootstrap 自带的初始 explanation / pressure / conflict trace 文本
- 依赖 bootstrap 命名的集成测试断言

## Out of Scope
- 新增 authority 规则
- 新增 schema 或 save namespace
- 全仓库所有手写测试夹具的统一改名
- 继续扩 `WarfareCampaign` / `OfficeAndCareer` 机制

## Touched Modules
- `Zongzu.Application`
- `Zongzu.Integration.Tests`

## Schema / Save Impact
- 无 schema 变更
- save 结构不变
- 仅影响 bootstrap 生成的玩家可见初始文本和值班名目

## Determinism Risk
- 低
- 只改 seed 文本，不改数值和调度顺序

## Milestones
1. 清最小 bootstrap 的县城、书院、埠口、佃户命名
2. 清 stress bootstrap 的县城、漕路、山路、渡口和初始冲突描写
3. 更新依赖 bootstrap 命名的集成断言
4. 跑 build + integration tests

## Tests
- `dotnet build Zongzu.sln -c Debug`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj -c Debug`
- `dotnet test Zongzu.sln -c Debug --no-build`

## Rollback Path
- 回退本次 bootstrap 命名和说明文本
- 保留已有数值 seed，不动模块状态形状
