using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zongzu.Architecture.Tests;

[TestFixture]
public sealed class ApplicationCommandBoundaryTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ApplicationDir = Path.Combine(RepoRoot, "src", "Zongzu.Application");
    private static readonly string[] AllowedMutableStateFiles =
    [
        Path.Combine("src", "Zongzu.Application", "GameSimulation.cs"),
        Path.Combine("src", "Zongzu.Application", "SimulationBootstrapper", "SimulationBootstrapper.Bootstraps.cs"),
        Path.Combine("src", "Zongzu.Application", "SimulationBootstrapper", "SimulationBootstrapper.Seeding.cs"),
    ];

    private static readonly Regex MutableStateUsagePattern = new(
        @"\bGetMutableModuleState\s*<",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Test]
    public void GetMutableModuleState_usage_must_stay_inside_bootstrap_and_core_simulation()
    {
        string[] actualFiles = Directory.GetFiles(ApplicationDir, "*.cs", SearchOption.AllDirectories)
            .Where(static path => path.IndexOf($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) < 0)
            .Where(path => MutableStateUsagePattern.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(RepoRoot, path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.That(actualFiles, Is.EquivalentTo(AllowedMutableStateFiles),
            "Application-owned mutable module state access must remain confined to GameSimulation and SimulationBootstrapper bootstrap/seeding paths.");
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Zongzu.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not locate repo root from {AppContext.BaseDirectory}.");
    }
}
