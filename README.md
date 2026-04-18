# 3

Modular monolith lineage simulation prototype for Windows, built with a deterministic pure-C# authority layer and a Unity-facing shell.

## Verification

Local verification uses the solution-wide .NET build and test commands:

```powershell
dotnet build .\Zongzu.sln -c Debug
dotnet test .\Zongzu.sln -c Debug --no-build
```

## GitHub Actions

The repository now includes two GitHub Actions workflows:

- `dotnet-ci.yml`
  Builds the solution on push and pull request, runs `Zongzu.Integration.Tests` as an explicit gate, then runs the remaining test projects and uploads `.trx` results.
- `release-artifacts.yml`
  Runs on `workflow_dispatch` and `v*` tags, builds the solution in `Release`, and uploads a zipped binary artifact bundle from `src/*/bin/Release/net8.0`.
