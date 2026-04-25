using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        var forbidden = refs.Except(new[] { "Zongzu.Contracts", "Zongzu.Presentation.Unity.ViewModels" }).ToList();
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

    [Test]
    public void Source_must_not_define_forbidden_manager_or_god_controller_types()
    {
        var forbiddenTypePattern = new Regex(
            @"\b(class|record|record\s+class|record\s+struct|struct|interface)\s+(WorldManager|PersonManager|CharacterManager|GodController|WorldController)\b",
            RegexOptions.Compiled);

        var offenders = EnumerateSourceFiles(SrcDir)
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { file, line, lineNumber = index + 1 }))
            .Where(entry => forbiddenTypePattern.IsMatch(entry.line))
            .Select(entry => $"{Path.GetRelativePath(RepoRoot, entry.file)}:{entry.lineNumber}: {entry.line.Trim()}")
            .ToArray();

        Assert.That(
            offenders,
            Is.Empty,
            "World/Person/Character manager or god-controller types are forbidden. " +
            string.Join(Environment.NewLine, offenders));
    }

    [Test]
    public void Presentation_Unity_sources_must_not_hold_simulation_authority()
    {
        var presentationRoots = new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels"),
        };
        var forbiddenTokens = new[]
        {
            "Zongzu.Application",
            "Zongzu.Modules.",
            "PlayerCommandService",
            "GetMutableModuleState",
            "HandlePublicLifeCommand",
        };

        var offenders = FindTokenOccurrences(EnumerateSourceFiles(presentationRoots), forbiddenTokens);

        Assert.That(
            offenders,
            Is.Empty,
            "Presentation Unity sources must stay read-model/ViewModel adapters, not simulation authority. " +
            string.Join(Environment.NewLine, offenders));
    }

    [Test]
    public void Simulation_modules_must_not_use_application_or_presentation_mutation_paths()
    {
        var moduleRoots = ModuleProjectNames
            .Select(module => Path.Combine(SrcDir, module))
            .ToArray();
        var forbiddenTokens = new[]
        {
            "GetMutableModuleState",
            "PlayerCommandService",
            "FirstPassPresentationShell",
            "PresentationShellViewModel",
        };

        var offenders = FindTokenOccurrences(EnumerateSourceFiles(moduleRoots), forbiddenTokens);

        Assert.That(
            offenders,
            Is.Empty,
            "Simulation modules must not call application mutation escape hatches or presentation surfaces. " +
            string.Join(Environment.NewLine, offenders));
    }

    [Test]
    public void Social_memory_residue_writes_must_stay_inside_social_memory_module()
    {
        var forbiddenTokens = new[]
        {
            "new MemoryRecordState",
            ".Memories.Add",
            "ClanEmotionalClimates.Add",
            "PersonTemperings.Add",
        };

        var offenders = FindTokenOccurrences(
            EnumerateSourceFiles(SrcDir)
                .Where(file => file.IndexOf($"{Path.DirectorySeparatorChar}Zongzu.Modules.SocialMemoryAndRelations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) < 0),
            forbiddenTokens);

        Assert.That(
            offenders,
            Is.Empty,
            "Only SocialMemoryAndRelations may write social-memory residue/climate/tempering state. " +
            string.Join(Environment.NewLine, offenders));
    }

    [Test]
    public void Social_memory_public_life_order_residue_must_not_parse_order_receipt_summary()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.PublicLifeOrderResidue.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Contain("LastInterventionOutcomeCode"));
        Assert.That(
            source.Contains("LastInterventionRefusalCode", StringComparison.Ordinal)
            || source.Contains("LastInterventionPartialCode", StringComparison.Ordinal),
            Is.True);
    }

    [Test]
    public void Social_memory_public_life_order_response_residue_must_not_parse_response_or_event_summary()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.PublicLifeOrderResponseResidue.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Contain("LastRefusalResponseOutcomeCode"));
        Assert.That(source, Does.Contain("LastRefusalResponseTraceCode"));
        Assert.That(source, Does.Contain("LastRefusalResponseCommandCode"));
    }

    [Test]
    public void Social_memory_public_life_order_response_drift_must_not_parse_response_or_event_summary()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.PublicLifeOrderResponseDrift.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Contain("CauseKey.StartsWith"));
        Assert.That(source, Does.Contain("TryReadPublicLifeResponseCause"));
    }

    [Test]
    public void Ordinary_household_order_residue_projection_must_use_structured_after_account_fields()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.LivingSociety.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Contain("HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue"));
        Assert.That(source, Does.Contain("LastRefusalResponseTraceCode"));
        Assert.That(source, Does.Contain("BuildOrderResponseAftermathSummary"));
        Assert.That(source, Does.Contain("BuildOfficeResponseAftermathSummary"));
        Assert.That(source, Does.Contain("BuildFamilyResponseAftermathSummary"));
    }

    [Test]
    public void Ordinary_household_response_play_surface_must_use_projected_household_pressures_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.OrdinaryHouseholdSurface.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Not.Contain("PopulationAndHouseholdsState"));
        Assert.That(source, Does.Not.Contain(".Memories.Add"));
        Assert.That(source, Does.Contain("HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue"));
        Assert.That(source, Does.Contain("PlayerCommandAffordanceSnapshot"));
        Assert.That(source, Does.Contain("PlayerCommandReceiptSnapshot"));
    }

    [Test]
    public void Home_household_local_response_commands_must_stay_population_owned()
    {
        string[] sourcePaths =
        [
            Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds", "PopulationAndHouseholdsCommandResolver.cs"),
            Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds", "PopulationAndHouseholdsModule.Commands.cs"),
        ];
        string source = string.Join(Environment.NewLine, sourcePaths.Select(File.ReadAllText));

        Assert.That(source, Does.Not.Contain("OrderAndBanditryState"));
        Assert.That(source, Does.Not.Contain("OfficeAndCareerState"));
        Assert.That(source, Does.Not.Contain("FamilyCoreState"));
        Assert.That(source, Does.Not.Contain("SocialMemoryAndRelationsState"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Contain("KnownModuleKeys.PopulationAndHouseholds"));
        Assert.That(source, Does.Contain("LastLocalResponseCommandCode"));
        Assert.That(source, Does.Contain("LastLocalResponseOutcomeCode"));
        Assert.That(source, Does.Contain("LastLocalResponseTraceCode"));
        Assert.That(source, Does.Contain("LocalResponseCarryoverMonths"));
    }

    [Test]
    public void Home_household_local_response_projection_must_copy_projected_fields_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("PopulationAndHouseholdsState"));
        Assert.That(source, Does.Not.Contain(".Memories.Add"));
        Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(source, Does.Not.Contain("CooldownLedger"));
        Assert.That(source, Does.Not.Contain("HouseholdTarget"));
        Assert.That(source, Does.Contain("HouseholdPressureSnapshot"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseAffordanceCapacity"));
        Assert.That(source, Does.Contain("BuildHomeHouseholdLocalResponseAffordanceCapacity"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseTradeoffForecast"));
        Assert.That(source, Does.Contain("BuildHomeHouseholdLocalResponseTradeoffForecast"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseShortTermConsequenceReadback"));
        Assert.That(source, Does.Contain("BuildHomeHouseholdLocalResponseShortTermConsequenceReadback"));
        Assert.That(source, Does.Contain("HouseholdExternalOwnerLaneReturnGuidance"));
        Assert.That(source, Does.Contain("BuildHomeHouseholdExternalOwnerLaneReturnGuidance"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseFollowUpHint"));
        Assert.That(source, Does.Contain("BuildHomeHouseholdLocalResponseFollowUpHint"));
        Assert.That(source, Does.Contain("外部后账归位"));
        Assert.That(source, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(source, Does.Contain("该走县门/文移 lane"));
        Assert.That(source, Does.Contain("该走族老/担保 lane"));
        Assert.That(source, Does.Contain("本户不能代修"));
        Assert.That(source, Does.Contain("IsEnabled"));
        Assert.That(source, Does.Contain("LastLocalResponseCommandCode"));
        Assert.That(source, Does.Contain("PlayerCommandAffordanceSnapshot"));
        Assert.That(source, Does.Contain("PlayerCommandReceiptSnapshot"));
    }

    [Test]
    public void Owner_lane_return_surface_readback_must_stay_projection_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.OwnerLaneReturnSurface.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("PopulationAndHouseholdsState"));
        Assert.That(source, Does.Not.Contain("SocialMemoryAndRelationsState"));
        Assert.That(source, Does.Not.Contain(".Memories.Add"));
        Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(source, Does.Not.Contain("CooldownLedger"));
        Assert.That(source, Does.Not.Contain("HouseholdTarget"));
        Assert.That(source, Does.Not.Contain("ModuleSchemaVersion"));
        Assert.That(source, Does.Not.Contain("UpgradeFromSchema"));
        Assert.That(source, Does.Not.Contain("Migration"));
        Assert.That(source, Does.Contain("HouseholdPressureSnapshot"));
        Assert.That(source, Does.Contain("LastLocalResponseCommandCode"));
        Assert.That(source, Does.Contain("LastLocalResponseOutcomeCode"));
        Assert.That(source, Does.Contain("LocalResponseCarryoverMonths"));
        Assert.That(source, Does.Contain("外部后账归位"));
        Assert.That(source, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(source, Does.Contain("该走县门/文移 lane"));
        Assert.That(source, Does.Contain("该走族老/担保 lane"));
        Assert.That(source, Does.Contain("本户不能代修"));
        Assert.That(source, Does.Contain("承接入口"));
        Assert.That(source, Does.Contain("添雇巡丁"));
        Assert.That(source, Does.Contain("押文催县门"));
        Assert.That(source, Does.Contain("请族老解释"));
    }

    [Test]
    public void Home_household_local_response_social_memory_reader_must_use_structured_population_aftermath_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.HomeHouseholdLocalResponseResidue.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("短期后果"));
        Assert.That(source, Does.Not.Contain("缓住项"));
        Assert.That(source, Does.Not.Contain("挤压项"));
        Assert.That(source, Does.Not.Contain("仍欠外部后账"));
        Assert.That(source, Does.Not.Contain("续接提示"));
        Assert.That(source, Does.Not.Contain("换招提示"));
        Assert.That(source, Does.Not.Contain("冷却提示"));
        Assert.That(source, Does.Not.Contain("续接读回"));
        Assert.That(source, Does.Not.Contain("外部后账归位"));
        Assert.That(source, Does.Not.Contain("该走巡丁"));
        Assert.That(source, Does.Not.Contain("该走县门"));
        Assert.That(source, Does.Not.Contain("该走族老"));
        Assert.That(source, Does.Not.Contain("本户不能代修"));
        Assert.That(source, Does.Not.Contain("OrderAndBanditryState"));
        Assert.That(source, Does.Not.Contain("OfficeAndCareerState"));
        Assert.That(source, Does.Not.Contain("FamilyCoreState"));
        Assert.That(source, Does.Not.Contain("PopulationAndHouseholdsState"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Contain("HouseholdPressureSnapshot"));
        Assert.That(source, Does.Contain("LastLocalResponseCommandCode"));
        Assert.That(source, Does.Contain("LastLocalResponseOutcomeCode"));
        Assert.That(source, Does.Contain("LastLocalResponseTraceCode"));
        Assert.That(source, Does.Contain("order.public_life.household_response."));
        Assert.That(source, Does.Contain("ClanNarrativeState"));
        Assert.That(source, Does.Contain("ClanEmotionalClimateState"));
        Assert.That(source, Does.Contain("AddMemory"));
    }

    [Test]
    public void Home_household_local_response_repeat_friction_must_read_structured_social_memory_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsCommandResolver.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("memory.Summary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("OrderAndBanditryState"));
        Assert.That(source, Does.Not.Contain("OfficeAndCareerState"));
        Assert.That(source, Does.Not.Contain("FamilyCoreState"));
        Assert.That(source, Does.Not.Contain("SocialMemoryAndRelationsState"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Not.Contain(".Memories.Add"));
        Assert.That(source, Does.Contain("ISocialMemoryAndRelationsQueries"));
        Assert.That(source, Does.Contain("GetMemoriesByClan"));
        Assert.That(source, Does.Contain("order.public_life.household_response."));
        Assert.That(source, Does.Contain("CauseKey.Contains"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseOutcomeCodes.Relieved"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseOutcomeCodes.Strained"));
    }

    [Test]
    public void Home_household_local_response_texture_must_use_existing_population_household_fields_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsCommandResolver.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Not.Contain("OrderAndBanditryState"));
        Assert.That(source, Does.Not.Contain("OfficeAndCareerState"));
        Assert.That(source, Does.Not.Contain("FamilyCoreState"));
        Assert.That(source, Does.Not.Contain("SocialMemoryAndRelationsState"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Not.Contain(".Memories.Add"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("memory.Summary"));
        Assert.That(source, Does.Contain("ResolveHomeHouseholdLocalResponseTextureProfile"));
        Assert.That(source, Does.Contain("BuildHouseholdCapacitySummaryTail"));
        Assert.That(source, Does.Contain("BuildHouseholdTradeoffSummaryTail"));
        Assert.That(source, Does.Contain("HouseholdLocalResponseTextureProfile"));
        Assert.That(source, Does.Contain("DebtCapacityBroken"));
        Assert.That(source, Does.Contain("LaborCapacityBroken"));
        Assert.That(source, Does.Contain("DebtPressure"));
        Assert.That(source, Does.Contain("LaborCapacity"));
        Assert.That(source, Does.Contain("Distress"));
        Assert.That(source, Does.Contain("MigrationRisk"));
        Assert.That(source, Does.Contain("DependentCount"));
        Assert.That(source, Does.Contain("LaborerCount"));
        Assert.That(source, Does.Contain("LivelihoodType"));
    }

    [Test]
    public void Public_life_response_friction_readers_must_not_parse_social_memory_summary()
    {
        string[] sourcePaths =
        [
            Path.Combine(SrcDir, "Zongzu.Modules.OrderAndBanditry", "OrderAndBanditryCommandResolver.cs"),
            Path.Combine(SrcDir, "Zongzu.Modules.OfficeAndCareer", "OfficeAndCareerCommandResolver.cs"),
            Path.Combine(SrcDir, "Zongzu.Modules.FamilyCore", "FamilyCoreCommandResolver.cs"),
        ];

        foreach (string sourcePath in sourcePaths)
        {
            string source = File.ReadAllText(sourcePath);
            Assert.That(source, Does.Not.Contain("memory.Summary"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Contain("order.public_life.response."), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Contain("CauseKey.Contains"), Path.GetFileName(sourcePath));
        }
    }

    [Test]
    public void Public_life_actor_countermoves_must_read_structured_social_memory_only()
    {
        string[] sourcePaths =
        [
            Path.Combine(SrcDir, "Zongzu.Modules.OrderAndBanditry", "OrderAndBanditryModule", "OrderAndBanditryModule.PublicLifeActorCountermove.cs"),
            Path.Combine(SrcDir, "Zongzu.Modules.OfficeAndCareer", "OfficeAndCareerModule", "OfficeAndCareerModule.PublicLifeActorCountermove.cs"),
            Path.Combine(SrcDir, "Zongzu.Modules.FamilyCore", "FamilyCoreModule.PublicLifeActorCountermove.cs"),
        ];

        foreach (string sourcePath in sourcePaths)
        {
            string source = File.ReadAllText(sourcePath);
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Not.Contain("LastInterventionSummary"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Not.Contain("memory.Summary"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Contain("CauseKey.StartsWith"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Contain("CauseKey.Contains"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Contain("OriginDate"), Path.GetFileName(sourcePath));
        }
    }

    [Test]
    public void PersonRecord_must_remain_identity_only()
    {
        string personTypesPath = Path.Combine(SrcDir, "Zongzu.Contracts", "PersonRegistryTypes.cs");
        string source = File.ReadAllText(personTypesPath);
        Match match = Regex.Match(
            source,
            @"public sealed class PersonRecord\s*\{(?<body>.*?)\n\}",
            RegexOptions.Singleline);

        Assert.That(match.Success, Is.True, "Could not locate PersonRecord.");

        string[] properties = Regex.Matches(
                match.Groups["body"].Value,
                @"public\s+[^\r\n{]+\s+(?<name>\w+)\s*\{\s*get;\s*set;\s*\}")
            .Select(static property => property.Groups["name"].Value)
            .ToArray();

        Assert.That(
            properties,
            Is.EquivalentTo(new[]
            {
                "Id",
                "DisplayName",
                "BirthDate",
                "Gender",
                "LifeStage",
                "IsAlive",
                "FidelityRing",
            }),
            "PersonRegistry must remain an identity anchor, not a clan/household/office relationship table.");
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

    private static IEnumerable<string> EnumerateSourceFiles(params string[] roots)
    {
        return roots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            .Where(static file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static string[] FindTokenOccurrences(IEnumerable<string> files, IReadOnlyCollection<string> forbiddenTokens)
    {
        return files
            .SelectMany(file => File.ReadLines(file)
                .Select((line, index) => new { file, line, lineNumber = index + 1 }))
            .Where(entry => forbiddenTokens.Any(token => entry.line.Contains(token, StringComparison.Ordinal)))
            .Select(entry => $"{Path.GetRelativePath(RepoRoot, entry.file)}:{entry.lineNumber}: {entry.line.Trim()}")
            .ToArray();
    }
}
