using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Zongzu.Architecture.Tests;

[TestFixture]
public class ProjectReferenceTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string SrcDir = Path.Combine(RepoRoot, "src");
    private static readonly string TestsDir = Path.Combine(RepoRoot, "tests");
    private static readonly string ToolsDir = Path.Combine(RepoRoot, "tools");

    private static readonly string[] ModuleProjectNames =
    {
        "Zongzu.Modules.ConflictAndForce",
        "Zongzu.Modules.EducationAndExams",
        "Zongzu.Modules.FamilyCore",
        "Zongzu.Modules.NarrativeProjection",
        "Zongzu.Modules.OfficeAndCareer",
        "Zongzu.Modules.OrderAndBanditry",
        "Zongzu.Modules.PersonRegistry",
        "Zongzu.Modules.PopulationAndHouseholds",
        "Zongzu.Modules.PublicLifeAndRumor",
        "Zongzu.Modules.SocialMemoryAndRelations",
        "Zongzu.Modules.TradeAndIndustry",
        "Zongzu.Modules.WarfareCampaign",
        "Zongzu.Modules.WorldSettlements",
    };

    private static readonly string[] InfrastructureProjectNames =
    {
        "Zongzu.Persistence",
        "Zongzu.Scheduler",
    };

    private static readonly string[] AllSimulationProjectNames;

    static ProjectReferenceTests()
    {
        AllSimulationProjectNames = ModuleProjectNames
            .Concat(InfrastructureProjectNames)
            .Concat(new[] { "Zongzu.Kernel", "Zongzu.Contracts" })
            .ToArray();
    }

    [Test]
    public void Kernel_must_not_reference_any_project()
    {
        var refs = GetProjectReferences("Zongzu.Kernel");
        Assert.That(refs, Is.Empty,
            "Kernel must remain the innermost layer with zero project references.");
    }

    [Test]
    public void Contracts_must_only_reference_Kernel()
    {
        var refs = GetProjectReferences("Zongzu.Contracts");
        Assert.That(refs, Is.EquivalentTo(new[] { "Zongzu.Kernel" }),
            "Contracts may only reference Kernel.");
    }

    [Test]
    public void Modules_must_only_reference_Kernel_and_Contracts()
    {
        var allowed = new[] { "Zongzu.Kernel", "Zongzu.Contracts" };
        foreach (var module in ModuleProjectNames)
        {
            var refs = GetProjectReferences(module);
            var forbidden = refs.Except(allowed).ToList();
            Assert.That(forbidden, Is.Empty,
                $"Module {module} illegally references: {string.Join(", ", forbidden)}. " +
                "Modules may only reference Kernel and Contracts.");
        }
    }

    [Test]
    public void Modules_must_not_reference_each_other()
    {
        foreach (var module in ModuleProjectNames)
        {
            var refs = GetProjectReferences(module);
            var crossModuleRefs = refs.Intersect(ModuleProjectNames).ToList();
            Assert.That(crossModuleRefs, Is.Empty,
                $"Module {module} illegally references other modules: {string.Join(", ", crossModuleRefs)}");
        }
    }

    [Test]
    public void Infrastructure_must_only_reference_Kernel_and_Contracts()
    {
        var allowed = new[] { "Zongzu.Kernel", "Zongzu.Contracts" };
        foreach (var infra in InfrastructureProjectNames)
        {
            var refs = GetProjectReferences(infra);
            var forbidden = refs.Except(allowed).ToList();
            Assert.That(forbidden, Is.Empty,
                $"Infrastructure {infra} illegally references: {string.Join(", ", forbidden)}. " +
                "Infrastructure projects may only reference Kernel and Contracts.");
        }
    }

    [Test]
    public void Presentation_Unity_must_not_reference_simulation_modules()
    {
        var refs = GetProjectReferences("Zongzu.Presentation.Unity");
        var forbidden = refs.Except(new[] { "Zongzu.Contracts" }).ToList();
        Assert.That(forbidden, Is.Empty,
            $"Presentation.Unity may only reference Contracts. Found: {string.Join(", ", forbidden)}");
    }

    [Test]
    public void Application_may_reference_all_simulation_and_infrastructure()
    {
        var allowed = AllSimulationProjectNames
            .Concat(InfrastructureProjectNames)
            .ToHashSet();

        var refs = GetProjectReferences("Zongzu.Application");
        var forbidden = refs.Where(r => !allowed.Contains(r)).ToList();
        Assert.That(forbidden, Is.Empty,
            $"Application illegally references: {string.Join(", ", forbidden)}");
    }

    [Test]
    public void Test_projects_must_not_reference_other_test_projects()
    {
        var testCsprojs = Directory.GetFiles(TestsDir, "*.csproj", SearchOption.AllDirectories);

        var testProjectNames = testCsprojs
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet();

        foreach (var csproj in testCsprojs)
        {
            var name = Path.GetFileNameWithoutExtension(csproj);
            var refs = ParseProjectReferences(csproj);
            var forbidden = refs.Intersect(testProjectNames).ToList();
            Assert.That(forbidden, Is.Empty,
                $"Test project {name} illegally references other test projects: {string.Join(", ", forbidden)}");
        }
    }

    [Test]
    public void No_simulation_project_may_reference_Unity()
    {
        var simulationProjects = AllSimulationProjectNames
            .Concat(InfrastructureProjectNames)
            .Concat(new[] { "Zongzu.Application" })
            .ToArray();

        foreach (var projName in simulationProjects)
        {
            var csproj = FindCsproj(projName);
            var unityRefs = ParseUnityReferences(csproj);
            Assert.That(unityRefs, Is.Empty,
                $"Project {projName} illegally references Unity: {string.Join(", ", unityRefs)}");
        }
    }

    private static string[] GetProjectReferences(string projectName)
    {
        var csproj = FindCsproj(projectName);
        return ParseProjectReferences(csproj);
    }

    private static string FindCsproj(string projectName)
    {
        foreach (var dir in new[] { SrcDir, TestsDir, ToolsDir })
        {
            var path = Path.Combine(dir, projectName, $"{projectName}.csproj");
            if (File.Exists(path))
                return path;
        }
        Assert.Fail($"Could not find csproj for {projectName}");
        return null!; // unreachable
    }

    private static string[] ParseProjectReferences(string csprojPath)
    {
        var doc = XDocument.Load(csprojPath);

        var refs = doc.Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(path => Path.GetFileNameWithoutExtension(path!))
            .ToArray();

        return refs;
    }

    private static string[] ParseUnityReferences(string csprojPath)
    {
        var doc = XDocument.Load(csprojPath);

        var allRefs = new List<string?>();

        allRefs.AddRange(doc.Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value));

        allRefs.AddRange(doc.Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value));

        allRefs.AddRange(doc.Descendants("Reference")
            .Select(e => e.Attribute("Include")?.Value));

        return allRefs
            .Where(r => !string.IsNullOrEmpty(r))
            .Where(r => r!.IndexOf("Unity", StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray()!;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Zongzu.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            "Could not locate repo root from " + AppContext.BaseDirectory);
    }
}
