# 3

Modular monolith lineage simulation prototype for Windows, built with a deterministic pure-C# authority layer and a Unity-facing shell.

## Verification

Local verification uses the solution-wide .NET build and test commands:

```powershell
dotnet build .\Zongzu.sln -c Debug
dotnet test .\Zongzu.sln -c Debug --no-build
```

## Preview

You can generate the current MVP family-lifecycle snapshots with:

```powershell
dotnet run --project .\tools\Zongzu.MvpPreviewRunner\Zongzu.MvpPreviewRunner.csproj
```

The runner writes two preview artifacts against the explicit default MVP bootstrap, not the optional public-life / conflict / governance / warfare paths:

- [content/generated/mvp-family-lifecycle-preview.md](content/generated/mvp-family-lifecycle-preview.md)
- [content/generated/mvp-family-lifecycle-ten-year-preview.md](content/generated/mvp-family-lifecycle-ten-year-preview.md)

- The second artifact stretches the same MVP family-lifecycle slice across ten in-world years and audits whether hall / family council / notification guidance stays aligned on the same next family action.

## GitHub Actions

The repository now includes two GitHub Actions workflows:

- `dotnet-ci.yml`
  Builds the solution on push and pull request, runs `Zongzu.Integration.Tests` as an explicit gate, then runs the remaining test projects and uploads `.trx` results.
- `release-artifacts.yml`
  Runs on `workflow_dispatch` and `v*` tags, builds the solution in `Release`, and uploads a zipped binary artifact bundle from `src/*/bin/Release/net8.0`.
