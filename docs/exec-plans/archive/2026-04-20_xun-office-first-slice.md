## Goal
- move the first documented `xun` behavior into real runtime ownership for `OfficeAndCareer`
- let paper delay, petition queue drift, and local yamen pressure breathe inside the month while keeping appointments, office transfer, and reputation movement month-bound

## Scope in
- add bounded `RunXun` behavior for `OfficeAndCareer`
- let serving officials drift `AdministrativeTaskLoad`, `PetitionPressure`, `PetitionBacklog`, `ClerkDependence`, and `JurisdictionLeverage`
- let pre-appointment candidates drift `AppointmentPressure` and `ClerkDependence` without granting office at `xun`
- read only from existing `Education`, `SocialMemory`, `OrderAndBanditry`, `BlackRoutePressure`, and optional `ConflictAndForce` seams
- rebuild jurisdiction projections after xun drift so application/query layers can see the updated pressure
- add focused module coverage for xun behavior

## Scope out
- no schema bump
- no new office commands
- no xun diff or xun event output
- no xun appointment grant, loss, promotion, or office transfer
- no new UI wording or hall projection changes

## Affected modules
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Modules.OfficeAndCareer.Tests`
- this exec-plan note

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains deterministic runtime evolution over existing office-owned state

## Determinism risk
- low to medium
- xun pulse should stay arithmetic and query-first
- no foreign writes
- month-end office review remains the authoritative readable cadence

## Milestones
1. define bounded `OfficeAndCareer` xun pulse over paper delay, queue drift, and yamen pressure
2. keep appointment grant/loss, authority-tier movement, and readable office outcomes month-bound
3. add focused xun module tests
4. run build/test verification

## Tests to add/update
- `Zongzu.Modules.OfficeAndCareer.Tests`
- `dotnet build / test` verification against full solution

## Rollback / fallback plan
- if xun office drift starts pre-granting too much momentum, keep `AppointmentPressure` month-bound and limit xun to serving-office queue pressure only
- if order/black-route pressure double-counts too hard against month aftermath, narrow xun office inputs to direct disorder and force activation only

## Open questions
- whether later xun office slices should read `PublicLifeAndRumor` venue heat directly, or whether order/conflict pressure is enough for local yamen temperature
- when to let multi-official settlements build intra-yamen load sharing instead of a simple lead-office jurisdiction average

## Result notes
- `OfficeAndCareer` now owns a bounded deterministic `RunXun` pulse over paper delay, petition queue drift, and local yamen pressure
- `shangxun` now nudges serving officials through task-load, paper pressure, and leverage drag while still keeping xun projection silent
- `zhongxun` now lets petition backlog and clerk dependence climb under disorder, black-route drag, and active force pressure without granting or removing office
- `xiaxun` now lets calm paper surfaces ease queue pressure while hot jurisdictions carry backlog and clerical drag into month-end review
- eligible but unappointed candidates can now drift in `AppointmentPressure` and `ClerkDependence` at `xun`, but appointment grant, loss, promotion, office transfer, and readable petition outcomes remain month-bound

## Verification
- `dotnet test .\tests\Zongzu.Modules.OfficeAndCareer.Tests\Zongzu.Modules.OfficeAndCareer.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
