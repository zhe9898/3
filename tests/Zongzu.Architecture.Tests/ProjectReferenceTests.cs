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
        Assert.That(source, Does.Not.Contain("Family承接入口"));
        Assert.That(source, Does.Not.Contain("族老解释读回"));
        Assert.That(source, Does.Not.Contain("本户担保读回"));
        Assert.That(source, Does.Not.Contain("宗房脸面读回"));
        Assert.That(source, Does.Not.Contain("Family闭环防回压"));
        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("receipt prose"));
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
    public void Event_contract_health_diagnostics_must_stay_runtime_only()
    {
        string diagnosticSourcePath = Path.Combine(
            TestsDir,
            "Zongzu.Integration.Tests",
            "M2LiteIntegrationTests",
            "TenYearSimulationHealthCheckTests.cs");
        string diagnosticSource = File.ReadAllText(diagnosticSourcePath);
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));

        Assert.That(diagnosticSource, Does.Contain("EventContractHealthClassifications"));
        Assert.That(diagnosticSource, Does.Contain("ProjectionOnlyReceipt"));
        Assert.That(diagnosticSource, Does.Contain("FutureContract"));
        Assert.That(diagnosticSource, Does.Contain("DormantSeededPath"));
        Assert.That(diagnosticSource, Does.Contain("AcceptanceTestGap"));
        Assert.That(diagnosticSource, Does.Contain("FormatEventContractKey"));
        Assert.That(diagnosticSource, Does.Contain("GetEventContractOwnerLane"));
        Assert.That(diagnosticSource, Does.Contain("EventContractEvidenceBacklinks"));
        Assert.That(diagnosticSource, Does.Contain("owner="));
        Assert.That(diagnosticSource, Does.Contain("evidence="));
        Assert.That(diagnosticSource, Does.Contain("CollectEventContractDebt"));
        Assert.That(diagnosticSource, Does.Contain("AssertNoUnclassifiedEventContractDebt"));
        Assert.That(diagnosticSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(diagnosticSource, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(diagnosticSource, Does.Not.Contain("EventHealthLedger"));
        Assert.That(diagnosticSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(diagnosticSource, Does.Not.Contain("PlayerCommandService"));
        Assert.That(schemaRules, Does.Contain("backend event contract health v32 adds no persisted fields"));
        Assert.That(schemaRules, Does.Contain("backend event contract health v33 adds no persisted fields"));
        Assert.That(schemaRules, Does.Contain("backend event contract health v34 adds no persisted fields"));
    }

    [Test]
    public void Office_policy_implementation_drag_must_use_structured_policy_window_metadata_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.OfficeAndCareer",
            "OfficeAndCareerModule",
            "OfficeAndCareerModule.PolicyImplementation.cs");
        string source = File.ReadAllText(sourcePath);
        string module = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.OfficeAndCareer",
            "OfficeAndCareerModule",
            "OfficeAndCareerModule.cs"));
        string contracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "Events",
            "OfficeAndCareerEventNames.cs"));

        Assert.That(source, Does.Contain("OfficeAndCareerEventNames.PolicyWindowOpened"));
        Assert.That(source, Does.Contain("OfficeAndCareerEventNames.PolicyImplemented"));
        Assert.That(source, Does.Contain("DomainEventMetadataKeys.PolicyWindowPressure"));
        Assert.That(source, Does.Contain("DomainEventMetadataKeys.PolicyImplementationOutcome"));
        Assert.That(source, Does.Contain("ReadMetadataInt(sourceEvent"));
        Assert.That(source, Does.Contain("ResolveSettlementMetadata"));
        Assert.That(source, Does.Contain("domainEvent.EntityKey"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("domainEvent.Summary"));
        Assert.That(source, Does.Not.Contain(".Summary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("PolicyImplementationLedger"));
        Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(source, Does.Not.Contain("CooldownLedger"));
        Assert.That(source, Does.Not.Contain("OfficeAndCareerStateProjection.UpgradeFromSchema"));
        Assert.That(module, Does.Contain("OfficeAndCareerEventNames.PolicyWindowOpened"));
        Assert.That(module, Does.Contain("OfficeAndCareerEventNames.PolicyImplemented"));
        Assert.That(contracts, Does.Contain("PolicyImplemented"));
    }

    [Test]
    public void Office_yamen_readback_spine_must_stay_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string playerCommandSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"),
        }.Select(File.ReadAllText));
        string publicLifeSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PublicLifeAndRumor",
            "PublicLifeAndRumorModule",
            "PublicLifeAndRumorModule.cs"));
        string contractsSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "GovernanceReadModels.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));

        Assert.That(governanceSource, Does.Contain("BuildOfficeImplementationReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildOfficeImplementationNextStepSummary"));
        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyEntryReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyDispatchReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyPublicReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyNoLoopGuardSummary"));
        Assert.That(governanceSource, Does.Contain("HasCourtPolicyProcessReadback"));
        Assert.That(governanceSource, Does.Contain("BuildRegimeOfficeReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildCanalRouteReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildResidueHealthSummary"));
        Assert.That(governanceSource, Does.Contain("BuildOfficeLaneEntryReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("BuildOfficeLaneReceiptClosureSummary"));
        Assert.That(governanceSource, Does.Contain("BuildOfficeLaneResidueFollowUpSummary"));
        Assert.That(governanceSource, Does.Contain("BuildOfficeLaneNoLoopGuardSummary"));
        Assert.That(governanceSource, Does.Contain("PetitionOutcomeCategory"));
        Assert.That(governanceSource, Does.Contain("OfficeImplementationReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("OfficeLaneEntryReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("OfficeLaneReceiptClosureSummary"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyEntryReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyDispatchReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyNoLoopGuardSummary"));
        Assert.That(governanceSource, Does.Contain("Office承接入口"));
        Assert.That(governanceSource, Does.Contain("Office后手收口读回"));
        Assert.That(governanceSource, Does.Contain("Office余味续接读回"));
        Assert.That(governanceSource, Does.Contain("Office闭环防回压"));
        Assert.That(governanceSource, Does.Contain("朝议压力读回"));
        Assert.That(governanceSource, Does.Contain("政策窗口读回"));
        Assert.That(governanceSource, Does.Contain("政策语气读回"));
        Assert.That(governanceSource, Does.Contain("文移到达读回"));
        Assert.That(governanceSource, Does.Contain("文移指向读回"));
        Assert.That(governanceSource, Does.Contain("县门执行承接读回"));
        Assert.That(governanceSource, Does.Contain("县门承接姿态"));
        Assert.That(governanceSource, Does.Contain("公议读法读回"));
        Assert.That(governanceSource, Does.Contain("公议承压读法"));
        Assert.That(governanceSource, Does.Contain("朝廷后手仍不直写地方"));
        Assert.That(governanceSource, Does.Contain("Office/PublicLife分读"));
        Assert.That(governanceSource, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(governanceSource, Does.Contain("Court-policy防回压"));
        Assert.That(governanceSource, Does.Contain("OfficeAndCareer lane"));
        Assert.That(governanceSource, Does.Contain("本户不能代修"));
        Assert.That(governanceSource, Does.Contain("本户不再代修"));
        Assert.That(governanceSource, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(governanceSource, Does.Not.Contain("OfficialNoticeLine"));
        Assert.That(governanceSource, Does.Not.Contain("PrefectureDispatchLine"));
        Assert.That(governanceSource, Does.Not.Contain("LastAdministrativeTrace"));
        Assert.That(governanceSource, Does.Not.Contain("LastExplanation"));
        Assert.That(governanceSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(governanceSource, Does.Not.Contain("domainEvent.Summary"));
        Assert.That(governanceSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(governanceSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(governanceSource, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(governanceSource, Does.Not.Contain("CooldownLedger"));
        Assert.That(governanceSource, Does.Not.Contain("PolicyImplementationLedger"));
        Assert.That(governanceSource, Does.Not.Contain("PolicyClosureLedger"));
        Assert.That(governanceSource, Does.Not.Contain("DispatchLedger"));

        Assert.That(contractsSource, Does.Contain("CourtPolicyEntryReadbackSummary"));
        Assert.That(contractsSource, Does.Contain("CourtPolicyDispatchReadbackSummary"));
        Assert.That(contractsSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(contractsSource, Does.Contain("CourtPolicyNoLoopGuardSummary"));

        Assert.That(playerCommandSource, Does.Contain("BuildOfficeImplementationAffordanceGuidance"));
        Assert.That(playerCommandSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(playerCommandSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(playerCommandSource, Does.Not.Contain("GetMutableModuleState"));

        Assert.That(publicLifeSource, Does.Contain("OfficeAndCareerEventNames.PolicyImplemented"));
        Assert.That(publicLifeSource, Does.Contain("ApplyPolicyImplementationHeat"));
        Assert.That(publicLifeSource, Does.Contain("DomainEventMetadataKeys.PolicyImplementationOutcome"));
        Assert.That(publicLifeSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(publicLifeSource, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(publicLifeSource, Does.Not.Contain("LastExplanation"));

        string officeModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.OfficeAndCareer",
            "OfficeAndCareerModule",
            "OfficeAndCareerModule.cs"));
        string publicLifeModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PublicLifeAndRumor",
            "PublicLifeAndRumorModule",
            "PublicLifeAndRumorModule.cs"));
        string socialModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.cs"));
        Assert.That(officeModule, Does.Contain("ModuleSchemaVersion => 7"));
        Assert.That(publicLifeModule, Does.Contain("ModuleSchemaVersion => 4"));
        Assert.That(socialModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(schemaRules, Does.Contain("court-policy process readback v93-v100 adds no persisted fields"));
    }

    [Test]
    public void Court_policy_process_thickening_v109_v116_must_stay_owner_lane_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string publicLifeSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PublicLifeAndRumor",
            "PublicLifeAndRumorModule",
            "PublicLifeAndRumorModule.cs"));
        string officeSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.OfficeAndCareer",
            "OfficeAndCareerModule",
            "OfficeAndCareerModule.PolicyImplementation.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "PublicLife", "PublicLifeShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine, EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry"))
            .Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-process-thickening-v109-v116.md"));

        foreach (string token in new[]
                 {
                     "政策语气读回",
                     "文移指向读回",
                     "县门承接姿态",
                     "公议承压读法",
                     "朝廷后手仍不直写地方",
                     "不是本户硬扛朝廷后账",
                 })
        {
            Assert.That(governanceSource, Does.Contain(token));
            Assert.That(publicLifeSource, Does.Contain(token));
        }

        Assert.That(officeSource, Does.Contain("PolicyImplementationProfile"));
        Assert.That(officeSource, Does.Contain("DomainEventMetadataKeys.PolicyImplementationOutcome"));
        Assert.That(officeSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(officeSource, Does.Not.Contain(".Summary"));
        Assert.That(governanceSource, Does.Not.Contain("OfficialNoticeLine"));
        Assert.That(governanceSource, Does.Not.Contain("PrefectureDispatchLine"));
        Assert.That(governanceSource, Does.Not.Contain("LastAdministrativeTrace"));
        Assert.That(governanceSource, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(publicLifeSource, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(publicLifeSource, Does.Not.Contain("LastAdministrativeTrace"));
        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("PolicyImplementationOutcome"));
        Assert.That(personRegistrySource, Does.Not.Contain("PolicyImplementation"));
        Assert.That(personRegistrySource, Does.Not.Contain("CourtAgenda"));
        string[] forbiddenCourtModuleDirectories = Directory
            .GetDirectories(SrcDir, "Zongzu.Modules.*", SearchOption.TopDirectoryOnly)
            .Where(static directory =>
            {
                string moduleName = Path.GetFileName(directory);
                return moduleName.Contains("Court", StringComparison.OrdinalIgnoreCase)
                    || moduleName.Contains("法院", StringComparison.Ordinal);
            })
            .ToArray();
        Assert.That(forbiddenCourtModuleDirectories, Is.Empty);

        foreach (string forbidden in new[]
                 {
                     "DispatchLedger",
                     "PolicyLedger",
                     "CourtProcessLedger",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "PolicyImplementationLedger",
                     "GetMutableModuleState",
                     "PlayerCommandService",
                 })
        {
            Assert.That(governanceSource, Does.Not.Contain(forbidden));
            Assert.That(publicLifeSource, Does.Not.Contain(forbidden));
            Assert.That(unitySource, Does.Not.Contain(forbidden));
        }

        Assert.That(schemaRules, Does.Contain("court-policy process thickening v109-v116 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy process thickening v109-v116 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No full Court module"));
    }

    [Test]
    public void Court_policy_local_response_v117_v124_must_reuse_office_lane_and_remain_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string playerCommandSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"),
        }.Select(File.ReadAllText));
        string officeCommandSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.OfficeAndCareer",
            "OfficeAndCareerCommandResolver.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "PublicLife", "PublicLifeShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-local-response-v117-v124.md"));

        Assert.That(playerCommandSource, Does.Contain("BuildCourtPolicyLocalResponseGuidance"));
        Assert.That(playerCommandSource, Does.Contain("政策回应入口"));
        Assert.That(playerCommandSource, Does.Contain("文移续接选择"));
        Assert.That(playerCommandSource, Does.Contain("公议降温只读回"));
        Assert.That(playerCommandSource, Does.Contain("PressCountyYamenDocument"));
        Assert.That(playerCommandSource, Does.Contain("RedirectRoadReport"));
        Assert.That(governanceSource, Does.Contain("hasCourtPolicyProcess"));
        Assert.That(governanceSource, Does.Contain("GetGovernanceAffordancePriority"));

        Assert.That(officeCommandSource, Does.Contain("HasCourtPolicyLocalResponsePressure"));
        Assert.That(officeCommandSource, Does.Contain("政策文移续接"));
        Assert.That(officeCommandSource, Does.Contain("政策递报改道"));
        Assert.That(officeCommandSource, Does.Contain("PetitionPressure"));
        Assert.That(officeCommandSource, Does.Contain("AdministrativeTaskLoad"));
        Assert.That(officeCommandSource, Does.Contain("PetitionBacklog"));
        Assert.That(officeCommandSource, Does.Contain("ClerkDependence"));

        foreach (string source in new[] { governanceSource, officeCommandSource })
        {
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("OfficialNoticeLine"));
            Assert.That(source, Does.Not.Contain("PrefectureDispatchLine"));
            Assert.That(source, Does.Not.Contain("LastAdministrativeTrace"));
            Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
            Assert.That(source, Does.Not.Contain("WorldManager"));
            Assert.That(source, Does.Not.Contain("PersonManager"));
            Assert.That(source, Does.Not.Contain("CharacterManager"));
            Assert.That(source, Does.Not.Contain("GodController"));
            Assert.That(source, Does.Not.Contain("PolicyLedger"));
            Assert.That(source, Does.Not.Contain("CourtProcessLedger"));
            Assert.That(source, Does.Not.Contain("DispatchLedger"));
            Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
            Assert.That(source, Does.Not.Contain("CooldownLedger"));
        }

        foreach (string source in new[] { governanceSource, officeCommandSource, playerCommandSource, unitySource })
        {
            Assert.That(source, Does.Not.Contain("WorldManager"));
            Assert.That(source, Does.Not.Contain("PersonManager"));
            Assert.That(source, Does.Not.Contain("CharacterManager"));
            Assert.That(source, Does.Not.Contain("GodController"));
            Assert.That(source, Does.Not.Contain("PolicyLedger"));
            Assert.That(source, Does.Not.Contain("CourtProcessLedger"));
            Assert.That(source, Does.Not.Contain("DispatchLedger"));
            Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
            Assert.That(source, Does.Not.Contain("CooldownLedger"));
        }

        Assert.That(playerCommandSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(playerCommandSource, Does.Not.Contain("OfficialNoticeLine"));
        Assert.That(playerCommandSource, Does.Not.Contain("PrefectureDispatchLine"));
        Assert.That(playerCommandSource, Does.Not.Contain("LastLocalResponseSummary"));

        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("PolicyImplementationOutcome"));
        Assert.That(unitySource, Does.Not.Contain("BuildCourtPolicyLocalResponseGuidance"));
        Assert.That(schemaRules, Does.Contain("court-policy local response v117-v124 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy local response v117-v124 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No full court engine"));
    }

    [Test]
    public void Court_policy_social_memory_echo_v125_v132_must_remain_structured_and_schema_neutral()
    {
        string socialSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.CourtPolicyLocalResponseResidue.cs"));
        string publicLifeOrderSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.PublicLifeOrderResponseResidue.cs"));
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string socialMemoryKinds = File.ReadAllText(Path.Combine(SrcDir, "Zongzu.Contracts", "SocialMemoryKinds.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-social-memory-echo-v125-v132.md"));

        Assert.That(socialSource, Does.Contain("ApplyCourtPolicyLocalResponseResidue"));
        Assert.That(socialSource, Does.Contain("office.policy_local_response"));
        Assert.That(socialSource, Does.Contain("SocialMemoryKinds.OfficePolicyLocalResponseResidue"));
        Assert.That(socialSource, Does.Contain("IsCourtPolicyLocalResponseCarryover"));
        Assert.That(socialSource, Does.Contain("JurisdictionAuthoritySnapshot"));
        Assert.That(socialSource, Does.Contain("LastRefusalResponseCommandCode"));
        Assert.That(socialSource, Does.Contain("LastRefusalResponseOutcomeCode"));
        Assert.That(socialSource, Does.Contain("LastRefusalResponseTraceCode"));
        Assert.That(publicLifeOrderSource, Does.Contain("!IsCourtPolicyLocalResponseCarryover(jurisdiction)"));
        Assert.That(governanceSource, Does.Contain("政策回应余味续接读回"));
        Assert.That(governanceSource, Does.Contain("office.policy_local_response"));
        Assert.That(governanceSource, Does.Contain("TryReadOfficePolicyLocalResponseResidueCause"));
        Assert.That(socialMemoryKinds, Does.Contain("OfficePolicyLocalResponseResidue"));

        foreach (string source in new[] { socialSource, publicLifeOrderSource, governanceSource })
        {
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("OfficialNoticeLine"));
            Assert.That(source, Does.Not.Contain("PrefectureDispatchLine"));
            Assert.That(source, Does.Not.Contain("LastAdministrativeTrace"));
            Assert.That(source, Does.Not.Contain("LastPetitionOutcome"));
            Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
            Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
            Assert.That(source, Does.Not.Contain("WorldManager"));
            Assert.That(source, Does.Not.Contain("PersonManager"));
            Assert.That(source, Does.Not.Contain("CharacterManager"));
            Assert.That(source, Does.Not.Contain("GodController"));
            Assert.That(source, Does.Not.Contain("PolicyLedger"));
            Assert.That(source, Does.Not.Contain("CourtProcessLedger"));
            Assert.That(source, Does.Not.Contain("DispatchLedger"));
            Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
            Assert.That(source, Does.Not.Contain("CooldownLedger"));
            Assert.That(source, Does.Not.Contain("SocialMemoryLedger"));
            Assert.That(source, Does.Not.Contain("PersonRegistry"));
        }

        Assert.That(schemaRules, Does.Contain("court-policy social-memory echo v125-v132 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy social-memory echo v125-v132 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No full court engine"));
        Assert.That(execPlan, Does.Contain("No social-memory ledger"));
    }

    [Test]
    public void Court_policy_memory_pressure_readback_v133_v140_must_remain_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-memory-pressure-readback-v133-v140.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));

        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyMemoryPressureReadbackSummary"));
        Assert.That(governanceSource, Does.Contain("政策旧账回压读回"));
        Assert.That(governanceSource, Does.Contain("旧文移余味"));
        Assert.That(governanceSource, Does.Contain("下一次政策窗口读法"));
        Assert.That(governanceSource, Does.Contain("SocialMemoryEntrySnapshot"));
        Assert.That(governanceSource, Does.Contain("OfficePolicyLocalResponseResidueCause"));
        Assert.That(governanceSource, Does.Contain("HasCourtPolicyProcessReadback"));
        Assert.That(governanceSource, Does.Contain("RenderCourtPolicyLocalResponseTraceLabel"));

        Assert.That(governanceSource, Does.Not.Contain("residue.Summary"));
        Assert.That(governanceSource, Does.Not.Contain("memory.Summary"));
        Assert.That(governanceSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(governanceSource, Does.Not.Contain("OfficialNoticeLine"));
        Assert.That(governanceSource, Does.Not.Contain("PrefectureDispatchLine"));
        Assert.That(governanceSource, Does.Not.Contain("LastAdministrativeTrace"));
        Assert.That(governanceSource, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(governanceSource, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(governanceSource, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(governanceSource, Does.Not.Contain("CourtProcessLedger"));
        Assert.That(governanceSource, Does.Not.Contain("PolicyLedger"));
        Assert.That(governanceSource, Does.Not.Contain("MemoryPressureLedger"));
        Assert.That(governanceSource, Does.Not.Contain("WorldManager"));
        Assert.That(governanceSource, Does.Not.Contain("PersonManager"));
        Assert.That(governanceSource, Does.Not.Contain("CharacterManager"));

        Assert.That(schemaRules, Does.Contain("court-policy memory-pressure readback v133-v140 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy memory-pressure readback v133-v140 note"));
        Assert.That(moduleBoundaries, Does.Contain("Court-policy memory-pressure readback v133-v140 boundary note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No full court engine"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No dispatch ledger"));
        Assert.That(execPlan, Does.Contain("memory-pressure ledger"));
    }

    [Test]
    public void Court_policy_public_reading_echo_v141_v148_must_remain_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string playerCommandSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "PublicLife", "PublicLifeShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-public-reading-echo-v141-v148.md"));

        Assert.That(playerCommandSource, Does.Contain("BuildCourtPolicyPublicReadingEchoGuidance"));
        Assert.That(playerCommandSource, Does.Contain("政策公议旧读回"));
        Assert.That(playerCommandSource, Does.Contain("公议旧账回声"));
        Assert.That(playerCommandSource, Does.Contain("下一次榜示/递报旧读法"));
        Assert.That(playerCommandSource, Does.Contain("SelectLocalOfficePolicySocialMemories"));
        Assert.That(playerCommandSource, Does.Contain("SelectOfficePolicyResidue"));
        Assert.That(playerCommandSource, Does.Contain("TryReadOfficePolicyLocalResponseResidueCause"));
        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyPublicReadingEchoGuidance"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(unitySource, Does.Contain("LeverageSummary = command.LeverageSummary"));
        Assert.That(unitySource, Does.Contain("ReadbackSummary = command.ReadbackSummary"));

        foreach (string source in new[] { governanceSource, playerCommandSource })
        {
            Assert.That(source, Does.Not.Contain("residue.Summary"));
            Assert.That(source, Does.Not.Contain("memory.Summary"));
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("OfficialNoticeLine"));
            Assert.That(source, Does.Not.Contain("PrefectureDispatchLine"));
            Assert.That(source, Does.Not.Contain("LastAdministrativeTrace"));
            Assert.That(source, Does.Not.Contain("LastPetitionOutcome"));
            Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
            Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
            Assert.That(source, Does.Not.Contain("CourtProcessLedger"));
            Assert.That(source, Does.Not.Contain("PolicyLedger"));
            Assert.That(source, Does.Not.Contain("PublicReadingLedger"));
            Assert.That(source, Does.Not.Contain("MemoryPressureLedger"));
            Assert.That(source, Does.Not.Contain("WorldManager"));
            Assert.That(source, Does.Not.Contain("PersonManager"));
            Assert.That(source, Does.Not.Contain("CharacterManager"));
            Assert.That(source, Does.Not.Contain("GodController"));
        }

        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("PolicyImplementationOutcome"));
        Assert.That(schemaRules, Does.Contain("court-policy public-reading echo v141-v148 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy public-reading echo v141-v148 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy public-reading echo v141-v148 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No full court engine"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("public-reading ledger"));
    }

    [Test]
    public void Court_policy_public_follow_up_cue_v149_v156_must_remain_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string playerCommandSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "PublicLife", "PublicLifeShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-public-follow-up-cue-v149-v156.md"));

        Assert.That(playerCommandSource, Does.Contain("BuildCourtPolicyPublicFollowUpCue"));
        Assert.That(playerCommandSource, Does.Contain("政策公议后手提示"));
        Assert.That(playerCommandSource, Does.Contain("公议轻续提示"));
        Assert.That(playerCommandSource, Does.Contain("下一步仍看榜示/递报承口"));
        Assert.That(playerCommandSource, Does.Contain("不是冷却账本"));
        Assert.That(playerCommandSource, Does.Contain("PublicLifeOrderResponseOutcomeCodes"));
        Assert.That(playerCommandSource, Does.Contain("BuildCourtPolicyPublicReadingEchoGuidance"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(unitySource, Does.Contain("LeverageSummary = command.LeverageSummary"));
        Assert.That(unitySource, Does.Contain("ReadbackSummary = command.ReadbackSummary"));

        foreach (string source in new[] { governanceSource, playerCommandSource })
        {
            Assert.That(source, Does.Not.Contain("residue.Summary"));
            Assert.That(source, Does.Not.Contain("memory.Summary"));
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("OfficialNoticeLine"));
            Assert.That(source, Does.Not.Contain("PrefectureDispatchLine"));
            Assert.That(source, Does.Not.Contain("LastAdministrativeTrace"));
            Assert.That(source, Does.Not.Contain("LastPetitionOutcome"));
            Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
            Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
            Assert.That(source, Does.Not.Contain("CourtProcessLedger"));
            Assert.That(source, Does.Not.Contain("PolicyLedger"));
            Assert.That(source, Does.Not.Contain("PublicFollowUpLedger"));
            Assert.That(source, Does.Not.Contain("CooldownLedger"));
            Assert.That(source, Does.Not.Contain("WorldManager"));
            Assert.That(source, Does.Not.Contain("PersonManager"));
            Assert.That(source, Does.Not.Contain("CharacterManager"));
            Assert.That(source, Does.Not.Contain("GodController"));
        }

        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("PolicyImplementationOutcome"));
        Assert.That(schemaRules, Does.Contain("court-policy public follow-up cue v149-v156 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy public follow-up cue v149-v156 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy public follow-up cue v149-v156 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No cooldown ledger"));
    }

    [Test]
    public void Court_policy_follow_up_docket_guard_v157_v164_must_remain_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-follow-up-docket-guard-v157-v164.md"));

        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyPublicFollowUpDocketGuard"));
        Assert.That(governanceSource, Does.Contain("政策后手案牍防误读"));
        Assert.That(governanceSource, Does.Contain("公议后手只作案牍提示"));
        Assert.That(governanceSource, Does.Contain("不是Order后账"));
        Assert.That(governanceSource, Does.Contain("不是Office成败"));
        Assert.That(governanceSource, Does.Contain("仍等Office/PublicLife/SocialMemory分读"));
        Assert.That(governanceSource, Does.Contain("SelectOfficePolicyResidue"));
        Assert.That(governanceSource, Does.Contain("TryReadOfficePolicyLocalResponseResidueCause"));
        Assert.That(governanceSource, Does.Contain("CourtPolicyNoLoopGuardSummary"));
        Assert.That(unitySource, Does.Contain("CourtPolicyNoLoopGuardSummary = governance?.CourtPolicyNoLoopGuardSummary"));

        foreach (string forbidden in new[]
                 {
                     "residue.Summary",
                     "memory.Summary",
                     "DomainEvent.Summary",
                     "OfficialNoticeLine",
                     "PrefectureDispatchLine",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "CourtProcessLedger",
                     "PolicyLedger",
                     "PublicFollowUpLedger",
                     "CooldownLedger",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                 })
        {
            Assert.That(governanceSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(unitySource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(schemaRules, Does.Contain("court-policy follow-up docket guard v157-v164 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy follow-up docket guard v157-v164 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy follow-up docket guard v157-v164 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No cooldown ledger"));
    }

    [Test]
    public void Court_policy_suggested_action_guard_v165_v172_must_remain_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-suggested-action-guard-v165-v172.md"));

        Assert.That(governanceSource, Does.Contain("BuildGovernanceSuggestedActionGuard"));
        Assert.That(governanceSource, Does.Contain("HasCourtPolicyPublicFollowUpGuard"));
        Assert.That(governanceSource, Does.Contain("建议动作防误读"));
        Assert.That(governanceSource, Does.Contain("只承接已投影的政策公议后手"));
        Assert.That(governanceSource, Does.Contain("SelectPrimaryGovernanceAffordance"));
        Assert.That(governanceSource, Does.Contain("GetGovernanceAffordancePriority"));
        Assert.That(governanceSource, Does.Contain("BuildGovernanceSuggestedCommandPrompt("));
        Assert.That(unitySource, Does.Contain("governanceDocket?.GuidanceSummary"));
        Assert.That(unitySource, Does.Contain("ShellTextAdapter.CombineDistinct"));

        foreach (string forbidden in new[]
                 {
                     "LeverageSummary.Contains",
                     "ReadbackSummary.Contains",
                     "SuggestedCommandPrompt.Contains",
                     "residue.Summary",
                     "memory.Summary",
                     "DomainEvent.Summary",
                     "OfficialNoticeLine",
                     "PrefectureDispatchLine",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "CourtProcessLedger",
                     "PolicyLedger",
                     "PublicFollowUpLedger",
                     "CooldownLedger",
                     "DocketLedger",
                     "SuggestedActionLedger",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                 })
        {
            Assert.That(governanceSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(unitySource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(schemaRules, Does.Contain("court-policy suggested action guard v165-v172 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy suggested action guard v165-v172 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy suggested action guard v165-v172 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No new suggested-action ranking rule"));
    }

    [Test]
    public void Court_policy_suggested_receipt_guard_v173_v180_must_remain_projection_only_and_schema_neutral()
    {
        string receiptSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"));
        string unitySource = File.ReadAllText(Path.Combine(
            TestsDir,
            "Zongzu.Presentation.Unity.Tests",
            "FirstPassPresentationShellTests",
            "FirstPassPresentationShellTests.GovernanceAndPublicLife.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-suggested-receipt-guard-v173-v180.md"));
        Match helperMatch = Regex.Match(
            receiptSource,
            @"private static string BuildCourtPolicySuggestedReceiptGuard\((?<body>.*?)\r?\n    private static",
            RegexOptions.Singleline);

        Assert.That(helperMatch.Success, Is.True);
        string helperSource = helperMatch.Value;
        Assert.That(receiptSource, Does.Contain("BuildCourtPolicySuggestedReceiptGuard"));
        Assert.That(receiptSource, Does.Contain("建议回执防误读"));
        Assert.That(receiptSource, Does.Contain("只回收已投影的政策公议后手"));
        Assert.That(receiptSource, Does.Contain("回执不是新政策结果"));
        Assert.That(receiptSource, Does.Contain("仍等Office/PublicLife/SocialMemory分读"));
        Assert.That(unitySource, Does.Contain("建议回执防误读"));
        Assert.That(unitySource, Does.Contain("ReadbackSummary"));

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     ".Summary.Contains",
                     "LeverageSummary.Contains",
                     "ReadbackSummary.Contains",
                     "SuggestedCommandPrompt.Contains",
                     "OfficialNoticeLine",
                     "PrefectureDispatchLine",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "ReceiptLedger",
                     "SuggestedReceiptLedger",
                     "PolicyLedger",
                     "CourtProcessLedger",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "DocketLedger",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                 })
        {
            Assert.That(helperSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(schemaRules, Does.Contain("court-policy suggested receipt guard v173-v180 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy suggested receipt guard v173-v180 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy suggested receipt guard v173-v180 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No new suggested receipt ledger"));
    }

    [Test]
    public void Court_policy_receipt_docket_consistency_v181_v188_must_remain_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-receipt-docket-consistency-v181-v188.md"));
        Match helperMatch = Regex.Match(
            governanceSource,
            @"private static string BuildCourtPolicyReceiptDocketConsistencyGuard\((?<body>.*?)\r?\n    private static",
            RegexOptions.Singleline);

        Assert.That(helperMatch.Success, Is.True);
        string helperSource = helperMatch.Value;
        Assert.That(governanceSource, Does.Contain("BuildCourtPolicyReceiptDocketConsistencyGuard"));
        Assert.That(governanceSource, Does.Contain("回执案牍一致防误读"));
        Assert.That(governanceSource, Does.Contain("回执只回收已投影的政策公议后手"));
        Assert.That(governanceSource, Does.Contain("案牍不把回执读成新政策结果"));

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     ".Summary.Contains",
                     "LeverageSummary.Contains",
                     "ReadbackSummary.Contains",
                     "SuggestedCommandPrompt.Contains",
                     "OfficialNoticeLine",
                     "PrefectureDispatchLine",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "ReceiptLedger",
                     "DocketConsistencyLedger",
                     "PolicyLedger",
                     "CourtProcessLedger",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "DocketLedger",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                 })
        {
            Assert.That(helperSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(schemaRules, Does.Contain("court-policy receipt-docket consistency guard v181-v188 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy receipt-docket consistency guard v181-v188 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy receipt-docket consistency guard v181-v188 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No new receipt-docket ledger"));
    }

    [Test]
    public void Court_policy_public_life_receipt_echo_v189_v196_must_remain_projection_only_and_schema_neutral()
    {
        string playerCommandSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.cs"));
        string unityAdapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "PublicLife", "PublicLifeShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiDocs = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-public-life-receipt-echo-v189-v196.md"));
        Match helperMatch = Regex.Match(
            playerCommandSource,
            @"private static string BuildCourtPolicyPublicLifeReceiptEchoGuard\((?<body>.*?)\r?\n    private static",
            RegexOptions.Singleline);

        Assert.That(helperMatch.Success, Is.True);
        string helperSource = helperMatch.Value;
        Assert.That(playerCommandSource, Does.Contain("BuildCourtPolicyPublicLifeReceiptEchoGuard"));
        Assert.That(playerCommandSource, Does.Contain("公议回执回声防误读"));
        Assert.That(playerCommandSource, Does.Contain("街面只读已投影的政策公议后手"));
        Assert.That(playerCommandSource, Does.Contain("公议不把回执读成新政令"));
        Assert.That(playerCommandSource, Does.Contain("TryReadOfficePolicyLocalResponseResidueCause"));

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     ".Summary.Contains",
                     "LeverageSummary.Contains",
                     "ReadbackSummary.Contains",
                     "SuggestedCommandPrompt.Contains",
                     "OfficialNoticeLine",
                     "PrefectureDispatchLine",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "ReceiptLedger",
                     "PublicLifeReceiptEchoLedger",
                     "PolicyLedger",
                     "CourtProcessLedger",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "DocketLedger",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                 })
        {
            Assert.That(helperSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(unityAdapterSource, Does.Contain("ReadbackSummary"));
        Assert.That(unityAdapterSource, Does.Contain("LeverageSummary"));
        Assert.That(unityAdapterSource, Does.Not.Contain("BuildCourtPolicyPublicLifeReceiptEchoGuard"));
        Assert.That(unityAdapterSource, Does.Not.Contain("TryReadOfficePolicyLocalResponseResidueCause"));
        Assert.That(unityAdapterSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(unityAdapterSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(schemaRules, Does.Contain("court-policy public-life receipt echo v189-v196 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy public-life receipt echo v189-v196 note"));
        Assert.That(uiDocs, Does.Contain("Court-policy public-life receipt echo v189-v196 UI note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));
        Assert.That(execPlan, Does.Contain("No new public-life receipt echo ledger"));
    }

    [Test]
    public void Court_policy_first_rule_density_closeout_v197_v204_must_document_v109_v196_without_claiming_full_court_engine()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string ui = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string audit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-27_court-policy-first-rule-density-closeout-audit-v197-v204.md"));

        Assert.That(topologyIndex, Does.Contain("Chain 8 First Rule-Density Closeout Audit - v197-v204"));
        Assert.That(topologyIndex, Does.Contain("v109-v196 first rule-density closeout audit v197-v204"));
        Assert.That(topologyIndex, Does.Contain("not the full court engine"));
        Assert.That(topologyIndex, Does.Contain("Court process state, appointment slate, dispatch arrival, and downstream household/market/public consequences remain explicit full-chain debt"));

        foreach (string subpass in new[]
                 {
                     "v109-v116",
                     "v117-v124",
                     "v125-v132",
                     "v133-v140",
                     "v141-v148",
                     "v149-v156",
                     "v157-v164",
                     "v165-v172",
                     "v173-v180",
                     "v181-v188",
                     "v189-v196",
                 })
        {
            Assert.That(topologyIndex, Does.Contain(subpass), subpass);
            Assert.That(acceptance, Does.Contain(subpass), subpass);
        }

        Assert.That(integrationRules, Does.Contain("Chain 8 v197-v204 first rule-density closeout audit integration note"));
        Assert.That(moduleBoundaries, Does.Contain("Court-policy first rule-density closeout audit v197-v204 boundary note"));
        Assert.That(schemaRules, Does.Contain("court-policy first rule-density closeout audit v197-v204 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current court-policy first rule-density closeout audit v197-v204 note"));
        Assert.That(simulation, Does.Contain("Current court-policy first rule-density closeout audit v197-v204 note"));
        Assert.That(ui, Does.Contain("Court-policy first rule-density closeout audit v197-v204 UI note"));
        Assert.That(acceptance, Does.Contain("Backend Chain 8 first rule-density closeout audit v197-v204 acceptance"));
        Assert.That(audit, Does.Contain("v197-v204 court-policy first rule-density closeout audit"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V204"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No Court module"));
        Assert.That(execPlan, Does.Contain("No new persisted field"));

        Assert.That(Directory.Exists(Path.Combine(SrcDir, "Zongzu.Modules.Court")), Is.False);

        foreach (string text in new[]
                 {
                     topologyIndex,
                     integrationRules,
                     moduleBoundaries,
                     schemaRules,
                     dataSchema,
                     simulation,
                     ui,
                     acceptance,
                     audit,
                     skillMatrix,
                     execPlan,
                 })
        {
            Assert.That(text, Does.Not.Contain("full court engine is complete"));
            Assert.That(text, Does.Not.Contain("v197-v204 completes court-agenda / policy-dispatch"));
            Assert.That(text, Does.Not.Contain("UI may compute court-policy results"));
            Assert.That(text, Does.Not.Contain("Application calculates policy success"));
        }
    }

    [Test]
    public void Thin_chain_closeout_audit_must_document_v100_without_claiming_full_chain_completion()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string ui = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string relationships = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RELATIONSHIPS_AND_GRUDGES.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string audit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-26_thin-chain-closeout-audit-v101-v108.md"));

        Assert.That(topologyIndex, Does.Contain("Thin-Chain Closeout Status - v101-v108"));
        Assert.That(topologyIndex, Does.Contain("closed through v100"));
        Assert.That(topologyIndex, Does.Contain("This is not a full-chain completion claim"));
        Assert.That(topologyIndex, Does.Contain("Full-Chain Debt"));
        Assert.That(topologyIndex, Does.Contain("v3-v100 thin-chain evidence"));

        Assert.That(integrationRules, Does.Contain("Thin-chain closeout audit v101-v108 integration note"));
        Assert.That(integrationRules, Does.Contain("It does not mean the full historical or social formula is implemented"));
        Assert.That(moduleBoundaries, Does.Contain("thin-chain closeout audit v101-v108 note"));
        Assert.That(schemaRules, Does.Contain("thin-chain closeout audit v101-v108 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current thin-chain closeout audit v101-v108 note"));
        Assert.That(simulation, Does.Contain("Current thin-chain closeout audit v101-v108 note"));
        Assert.That(ui, Does.Contain("v101-v108 closes the thin-chain readback skeleton"));
        Assert.That(relationships, Does.Contain("Thin-chain closeout audit v101-v108 note"));
        Assert.That(acceptance, Does.Contain("Backend thin-chain closeout audit v101-v108 acceptance"));
        Assert.That(audit, Does.Contain("v101-v108 thin-chain closeout audit"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V108"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No schema or migration impact"));

        foreach (string text in new[]
                 {
                     topologyIndex,
                     integrationRules,
                     moduleBoundaries,
                     schemaRules,
                     dataSchema,
                     simulation,
                     ui,
                     relationships,
                     acceptance,
                     audit,
                     skillMatrix,
                     execPlan,
                 })
        {
            Assert.That(text, Does.Not.Contain("full-chain completion is done"));
            Assert.That(text, Does.Not.Contain("thin-chain closeout completes full-chain formulas"));
            Assert.That(text, Does.Not.Contain("UI may compute owner-lane results"));
        }
    }

    [Test]
    public void Social_memory_office_policy_residue_must_read_structured_office_snapshots_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.OfficePolicyResidue.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.That(source, Does.Contain("JurisdictionAuthoritySnapshot"));
        Assert.That(source, Does.Contain("PetitionOutcomeCategory"));
        Assert.That(source, Does.Contain("AdministrativeTaskLoad"));
        Assert.That(source, Does.Contain("ClerkDependence"));
        Assert.That(source, Does.Contain("office.policy_implementation"));
        Assert.That(source, Does.Contain("SocialMemoryKinds.OfficePolicyImplementationResidue"));
        Assert.That(source, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(source, Does.Not.Contain("LastExplanation"));
        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain("receipt prose"));
        Assert.That(source, Does.Not.Contain("外部后账归位"));
        Assert.That(source, Does.Not.Contain("该走巡丁"));
        Assert.That(source, Does.Not.Contain("该走县门"));
        Assert.That(source, Does.Not.Contain("该走族老"));
        Assert.That(source, Does.Not.Contain("Family承接入口"));
        Assert.That(source, Does.Not.Contain("族老解释读回"));
        Assert.That(source, Does.Not.Contain("本户担保读回"));
        Assert.That(source, Does.Not.Contain("宗房脸面读回"));
        Assert.That(source, Does.Not.Contain("Family后手收口读回"));
        Assert.That(source, Does.Not.Contain("Family余味续接读回"));
        Assert.That(source, Does.Not.Contain("Family闭环防回压"));
        Assert.That(source, Does.Not.Contain("朝议压力读回"));
        Assert.That(source, Does.Not.Contain("政策窗口读回"));
        Assert.That(source, Does.Not.Contain("文移到达读回"));
        Assert.That(source, Does.Not.Contain("县门执行承接读回"));
        Assert.That(source, Does.Not.Contain("公议读法读回"));
        Assert.That(source, Does.Not.Contain("Court-policy防回压"));
        Assert.That(source, Does.Not.Contain("政策语气读回"));
        Assert.That(source, Does.Not.Contain("文移指向读回"));
        Assert.That(source, Does.Not.Contain("县门承接姿态"));
        Assert.That(source, Does.Not.Contain("公议承压读法"));
        Assert.That(source, Does.Not.Contain("朝廷后手仍不直写地方"));
        Assert.That(source, Does.Not.Contain("不是本户硬扛朝廷后账"));
        Assert.That(source, Does.Not.Contain("不是普通家户再扛"));
        Assert.That(source, Does.Not.Contain("本户不能代修"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
    }

    [Test]
    public void Unity_office_yamen_readback_must_copy_projected_fields_only()
    {
        string adapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string viewModelSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "DeskSandbox", "SettlementNodeViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Office", "OfficeJurisdictionViewModel.cs"),
        }.Select(File.ReadAllText));
        string officeSurfaceAdapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "ProjectionContexts", "OfficeProjectionContext.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
        }.Select(File.ReadAllText));

        Assert.That(adapterSource, Does.Contain("OfficeImplementationReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("OfficeNextStepReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("OfficeLaneEntryReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("OfficeLaneReceiptClosureSummary"));
        Assert.That(adapterSource, Does.Contain("OfficeLaneResidueFollowUpSummary"));
        Assert.That(adapterSource, Does.Contain("OfficeLaneNoLoopGuardSummary"));
        Assert.That(adapterSource, Does.Contain("CourtPolicyEntryReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("CourtPolicyDispatchReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("CourtPolicyNoLoopGuardSummary"));
        Assert.That(adapterSource, Does.Contain("RegimeOfficeReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("CanalRouteReadbackSummary"));
        Assert.That(adapterSource, Does.Contain("ResidueHealthSummary"));
        Assert.That(adapterSource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(adapterSource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(adapterSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(adapterSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(adapterSource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(adapterSource, Does.Not.Contain("PetitionOutcomeCategory"));

        Assert.That(viewModelSource, Does.Contain("OfficeImplementationReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("OfficeNextStepReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("OfficeLaneEntryReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("OfficeLaneReceiptClosureSummary"));
        Assert.That(viewModelSource, Does.Contain("OfficeLaneResidueFollowUpSummary"));
        Assert.That(viewModelSource, Does.Contain("OfficeLaneNoLoopGuardSummary"));
        Assert.That(viewModelSource, Does.Contain("CourtPolicyEntryReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("CourtPolicyDispatchReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("CourtPolicyNoLoopGuardSummary"));
        Assert.That(viewModelSource, Does.Contain("RegimeOfficeReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("CanalRouteReadbackSummary"));
        Assert.That(viewModelSource, Does.Contain("ResidueHealthSummary"));

        Assert.That(officeSurfaceAdapterSource, Does.Contain("GovernanceBySettlement"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("OfficeLaneEntryReadbackSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("OfficeLaneReceiptClosureSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("OfficeLaneResidueFollowUpSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("OfficeLaneNoLoopGuardSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("CourtPolicyEntryReadbackSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("CourtPolicyDispatchReadbackSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("CourtPolicyPublicReadbackSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Contain("CourtPolicyNoLoopGuardSummary"));
        Assert.That(officeSurfaceAdapterSource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(officeSurfaceAdapterSource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(officeSurfaceAdapterSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(officeSurfaceAdapterSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(officeSurfaceAdapterSource, Does.Not.Contain("DomainEventMetadataKeys"));
    }

    [Test]
    public void Unity_family_lane_closure_readback_must_copy_projected_fields_only()
    {
        string adapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string viewModelSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "DeskSandbox", "SettlementNodeViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Shared", "CommandAffordanceViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Shared", "CommandReceiptViewModel.cs"),
        }.Select(File.ReadAllText));

        foreach (string field in new[]
                 {
                     "FamilyLaneEntryReadbackSummary",
                     "FamilyElderExplanationReadbackSummary",
                     "FamilyGuaranteeReadbackSummary",
                     "FamilyHouseFaceReadbackSummary",
                     "FamilyLaneReceiptClosureSummary",
                     "FamilyLaneResidueFollowUpSummary",
                     "FamilyLaneNoLoopGuardSummary",
                 })
        {
            Assert.That(adapterSource, Does.Contain(field), field);
            Assert.That(viewModelSource, Does.Contain(field), field);
        }

        Assert.That(adapterSource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(adapterSource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(adapterSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(adapterSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(adapterSource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(adapterSource, Does.Not.Contain("SponsorClanId"));
        Assert.That(adapterSource, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(adapterSource, Does.Not.Contain("LastRefusalResponseSummary"));
    }

    [Test]
    public void Force_campaign_owner_lane_readback_must_stay_projection_only_and_schema_neutral()
    {
        string helperSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.WarfareLaneClosure.cs"));
        string projectionSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.Governance.cs"),
            Path.Combine(SrcDir, "Zongzu.Contracts", "ReadModels", "PlayerCommandReadModels.cs"),
            Path.Combine(SrcDir, "Zongzu.Contracts", "ReadModels", "GovernanceReadModels.cs"),
        }.Select(File.ReadAllText));
        string unityAdapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Warfare", "WarfareCampaignShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string unityViewModelSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Shared", "CommandAffordanceViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Shared", "CommandReceiptViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "DeskSandbox", "SettlementNodeViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Office", "OfficeJurisdictionViewModel.cs"),
        }.Select(File.ReadAllText));
        string socialMemorySource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.cs"));

        foreach (string field in new[]
                 {
                     "WarfareLaneEntryReadbackSummary",
                     "ForceReadinessReadbackSummary",
                     "CampaignAftermathReadbackSummary",
                     "WarfareLaneReceiptClosureSummary",
                     "WarfareLaneResidueFollowUpSummary",
                     "WarfareLaneNoLoopGuardSummary",
                 })
        {
            Assert.That(projectionSource, Does.Contain(field), field);
            Assert.That(unityAdapterSource, Does.Contain(field), field);
            Assert.That(unityViewModelSource, Does.Contain(field), field);
        }

        Assert.That(helperSource, Does.Contain("BuildWarfareLaneClosureReadback"));
        Assert.That(helperSource, Does.Contain("BuildWarfareLaneEntryReadbackSummary"));
        Assert.That(helperSource, Does.Contain("BuildForceReadinessReadbackSummary"));
        Assert.That(helperSource, Does.Contain("BuildCampaignAftermathReadbackSummary"));
        Assert.That(helperSource, Does.Contain("BuildWarfareLaneReceiptClosureSummary"));
        Assert.That(helperSource, Does.Contain("BuildWarfareLaneResidueFollowUpSummary"));
        Assert.That(helperSource, Does.Contain("BuildWarfareLaneNoLoopGuardSummary"));
        Assert.That(helperSource, Does.Contain("SelectLocalCampaignSocialMemories"));
        Assert.That(helperSource, Does.Contain("CampaignMobilizationSignalSnapshot"));
        Assert.That(helperSource, Does.Contain("CampaignFrontSnapshot"));
        Assert.That(helperSource, Does.Contain("JurisdictionAuthoritySnapshot"));
        Assert.That(helperSource, Does.Contain("SocialMemoryEntrySnapshot"));
        Assert.That(helperSource, Does.Contain("CauseKey.StartsWith(\"campaign."));
        Assert.That(helperSource, Does.Contain("军务承接入口"));
        Assert.That(helperSource, Does.Contain("Force承接读回"));
        Assert.That(helperSource, Does.Contain("战后后账读回"));
        Assert.That(helperSource, Does.Contain("军务后手收口读回"));
        Assert.That(helperSource, Does.Contain("军务余味续接读回"));
        Assert.That(helperSource, Does.Contain("军务闭环防回压"));
        Assert.That(helperSource, Does.Contain("不是普通家户硬扛"));
        Assert.That(helperSource, Does.Contain("不是把军务后账误读成县门/Order后账"));

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     "domainEvent.Summary",
                     "LastInterventionSummary",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "memory.Summary",
                     "IssueModuleCommand",
                     "GetMutableModuleState",
                     "OwnerLaneLedger",
                     "CampaignClosureLedger",
                     "ForceClosureLedger",
                     "HouseholdTarget",
                     "PersonRegistryState",
                     "ModuleSchemaVersion",
                     "UpgradeFromSchema",
                     "Migration",
                 })
        {
            Assert.That(helperSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(socialMemorySource, Does.Not.Contain("军务承接入口"));
        Assert.That(socialMemorySource, Does.Not.Contain("Force承接读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("战后后账读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("军务后手收口读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("军务闭环防回压"));

        Assert.That(unityAdapterSource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(unityAdapterSource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(unityAdapterSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(unityAdapterSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(unityAdapterSource, Does.Not.Contain("DomainEventMetadataKeys"));
    }

    [Test]
    public void Warfare_directive_choice_depth_must_stay_warfare_owned_and_schema_neutral()
    {
        string resolverSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.WarfareCampaign",
            "WarfareCampaignCommandResolver.cs"));
        string descriptorSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.WarfareCampaign",
            "WarfareCampaignDescriptors.cs"));
        string warfareModuleSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.WarfareCampaign",
            "WarfareCampaignModule",
            "WarfareCampaignModule.cs"));
        string choiceProjectionSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.WarfareDirectiveChoice.cs"));
        string compositionSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"),
        }.Select(File.ReadAllText));
        string unityAdapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Warfare", "WarfareCampaignShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));

        Assert.That(resolverSource, Does.Contain("BuildDirectiveChoiceReadbackSummary"));
        Assert.That(resolverSource, Does.Contain("ActiveDirectiveCode"));
        Assert.That(resolverSource, Does.Contain("LastDirectiveTrace"));
        Assert.That(descriptorSource, Does.Contain("BuildDirectiveChoiceReadbackSummary"));
        Assert.That(choiceProjectionSource, Does.Contain("BuildWarfareDirectiveChoiceReadback"));
        Assert.That(compositionSource, Does.Contain("BuildWarfareDirectiveChoiceReadback"));
        Assert.That(compositionSource, Does.Contain("JoinOwnerLaneReturnSurfaceText"));
        Assert.That(choiceProjectionSource, Does.Contain("CampaignMobilizationSignalSnapshot"));
        Assert.That(choiceProjectionSource, Does.Contain("CampaignFrontSnapshot"));

        foreach (string token in new[]
                 {
                     "军令选择读回",
                     "案头筹议选择",
                     "点兵加压选择",
                     "粮道护持选择",
                     "归营止损选择",
                     "WarfareCampaign拥有军令",
                     "军务选择不是县门文移代打",
                     "不是普通家户硬扛",
                 })
        {
            Assert.That(descriptorSource + choiceProjectionSource, Does.Contain(token), token);
        }

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     "domainEvent.Summary",
                     "LastInterventionSummary",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "memory.Summary",
                     "ConflictAndForceState",
                     "OfficeAndCareerState",
                     "PopulationAndHouseholdsState",
                     "SocialMemoryAndRelationsState",
                     "FamilyCoreState",
                     "IssueModuleCommand",
                     "GetMutableModuleState",
                     "OwnerLaneLedger",
                     "CampaignDirectiveLedger",
                     "CampaignClosureLedger",
                     "ForceClosureLedger",
                     "HouseholdTarget",
                     "PersonRegistryState",
                     "ModuleSchemaVersion => 5",
                     "UpgradeFromSchema",
                 })
        {
            Assert.That(resolverSource + choiceProjectionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(warfareModuleSource, Does.Contain("ModuleSchemaVersion => 4"));
        Assert.That(schemaRules, Does.Contain("warfare directive choice v77-v84 adds no persisted fields"));
        Assert.That(unityAdapterSource, Does.Not.Contain("Zongzu.Application"));
        Assert.That(unityAdapterSource, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(unityAdapterSource, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(unityAdapterSource, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(unityAdapterSource, Does.Not.Contain("DomainEventMetadataKeys"));
    }

    [Test]
    public void Warfare_aftermath_docket_readback_must_stay_projection_only_and_schema_neutral()
    {
        string bundleSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PresentationReadModelBundle.cs"));
        string builderSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.cs"));
        string helperSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.WarfareLaneClosure.cs"));
        string compositionSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"),
            Path.Combine(SrcDir, "Zongzu.Application", "PresentationReadModelBuilder", "PresentationReadModelBuilder.Governance.cs"),
        }.Select(File.ReadAllText));
        string unityAdapterSource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Warfare", "WarfareAftermathShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Warfare", "WarfareCampaignShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "ProjectionContexts", "WarfareProjectionContext.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "ProjectionContexts", "DeskSandboxProjectionContext.cs"),
        }.Select(File.ReadAllText));
        string warfareModuleSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.WarfareCampaign",
            "WarfareCampaignModule",
            "WarfareCampaignModule.cs"));
        string socialMemorySource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.SocialMemoryAndRelations",
            "SocialMemoryAndRelationsModule.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));

        Assert.That(bundleSource, Does.Contain("CampaignAftermathDockets"));
        Assert.That(builderSource, Does.Contain("GetAftermathDockets"));
        Assert.That(helperSource, Does.Contain("AftermathDocketSnapshot"));
        Assert.That(helperSource, Does.Contain("BuildCampaignAftermathDocketReadbackSummary"));
        Assert.That(helperSource, Does.Contain("BuildDocketCountReadback"));
        Assert.That(compositionSource, Does.Contain("CampaignAftermathDockets"));
        Assert.That(unityAdapterSource, Does.Contain("AftermathDocketSnapshot"));
        Assert.That(unityAdapterSource, Does.Contain("CampaignAftermathDockets"));
        Assert.That(unityAdapterSource, Does.Contain("ComposeDocketClauseText"));

        foreach (string token in new[]
                 {
                     "战后案卷读回",
                     "记功簿读回",
                     "劾责状读回",
                     "抚恤簿读回",
                     "清路札读回",
                     "WarfareCampaign拥有战后案卷",
                     "战后案卷不是县门/Order代算",
                     "不是普通家户补战后",
                     "军务案卷防回压",
                 })
        {
            Assert.That(helperSource, Does.Contain(token), token);
        }

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     "domainEvent.Summary",
                     "DocketSummary.Contains",
                     "DocketSummary.IndexOf",
                     "LastDirectiveTrace.Contains",
                     "LastDirectiveTrace.IndexOf",
                     "LastInterventionSummary.Contains",
                     "LastInterventionSummary.IndexOf",
                     "LastLocalResponseSummary.Contains",
                     "LastLocalResponseSummary.IndexOf",
                     "LastRefusalResponseSummary.Contains",
                     "LastRefusalResponseSummary.IndexOf",
                     "memory.Summary",
                     "IssueModuleCommand",
                     "GetMutableModuleState",
                     "AftermathLedger",
                     "ReliefLedger",
                     "RouteRepairLedger",
                     "OwnerLaneLedger",
                     "CampaignClosureLedger",
                     "ForceClosureLedger",
                     "HouseholdTarget",
                     "PersonRegistryState",
                     "ModuleSchemaVersion => 5",
                     "UpgradeFromSchema",
                 })
        {
            Assert.That(helperSource + compositionSource, Does.Not.Contain(forbidden), forbidden);
        }

        foreach (string forbidden in new[]
                 {
                     "MatchesSourceModule",
                     "HasTraceFromModule",
                     "DomainEventMetadataKeys",
                     "IssueModuleCommand",
                     "GetMutableModuleState",
                     "Zongzu.Application",
                     "Zongzu.Modules.",
                 })
        {
            Assert.That(unityAdapterSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(socialMemorySource, Does.Not.Contain("战后案卷读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("记功簿读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("劾责状读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("抚恤簿读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("清路札读回"));
        Assert.That(socialMemorySource, Does.Not.Contain("军务案卷防回压"));
        Assert.That(warfareModuleSource, Does.Contain("ModuleSchemaVersion => 4"));
        Assert.That(schemaRules, Does.Contain("warfare aftermath docket readback v85-v92 adds no persisted fields"));
    }

    [Test]
    public void Family_relief_choice_must_stay_family_owned_and_schema_neutral()
    {
        string resolverSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.FamilyCore",
            "FamilyCoreCommandResolver.cs"));
        string profileSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.FamilyCore",
            "FamilyCoreCommandResolver.Profiles.cs"));
        string familyModuleSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.FamilyCore",
            "FamilyCoreModule.cs"));
        string projectionSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.cs"));
        string receiptsSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.Receipts.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));

        Match grantBlock = Regex.Match(
            resolverSource,
            @"case PlayerCommandNames\.GrantClanRelief:(?<body>.*?)case PlayerCommandNames\.SuspendClanRelief:",
            RegexOptions.Singleline);
        Match readbackBuilder = Regex.Match(
            projectionSource,
            @"private static string BuildFamilyReliefChoiceReadback\(ClanSnapshot clan\)\s*\{(?<body>.*?)\n    \}",
            RegexOptions.Singleline);

        Assert.That(grantBlock.Success, Is.True, "Expected FamilyCore GrantClanRelief command block.");
        Assert.That(readbackBuilder.Success, Is.True, "Expected projection-only Family relief readback builder.");
        Assert.That(resolverSource, Does.Contain("PlayerCommandNames.GrantClanRelief"));
        Assert.That(profileSource, Does.Contain("ResolveGrantClanReliefProfile"));
        Assert.That(profileSource, Does.Contain("ApplyFamilyReliefProfile"));
        Assert.That(profileSource, Does.Contain("CharityObligation"));
        Assert.That(profileSource, Does.Contain("SupportReserve"));
        Assert.That(profileSource, Does.Contain("ReliefSanctionPressure"));
        Assert.That(familyModuleSource, Does.Contain("ModuleSchemaVersion => 8"));
        Assert.That(familyModuleSource, Does.Contain("PlayerCommandNames.GrantClanRelief"));
        Assert.That(projectionSource, Does.Contain("BuildFamilyReliefChoiceReadback"));
        Assert.That(projectionSource, Does.Contain("Family\u6551\u6d4e\u9009\u62e9\u8bfb\u56de"));
        Assert.That(projectionSource, Does.Contain("\u4e0d\u662f\u666e\u901a\u5bb6\u6237\u518d\u625b"));
        Assert.That(receiptsSource, Does.Contain("PlayerCommandNames.GrantClanRelief"));
        Assert.That(schemaRules, Does.Contain("family relief choice v61-v68 adds no persisted fields"));

        string[] forbiddenAuthorityTokens =
        [
            "PopulationAndHouseholdsState",
            "SocialMemoryAndRelationsState",
            ".Memories.Add",
            "DomainEvent.Summary",
            "LastLocalResponseSummary",
            "LastInterventionSummary",
            "ReliefLedger",
            "FamilyClosureLedger",
            "OwnerLaneLedger",
            "CooldownLedger",
            "HouseholdTarget",
            "PersonRegistry.Register",
            "GetMutableModuleState",
            "IssueModuleCommand",
        ];

        foreach (string token in forbiddenAuthorityTokens)
        {
            Assert.That(grantBlock.Groups["body"].Value, Does.Not.Contain(token), token);
            Assert.That(profileSource, Does.Not.Contain(token), token);
            Assert.That(readbackBuilder.Groups["body"].Value, Does.Not.Contain(token), token);
        }
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
    public void Family_household_burden_handoff_must_use_structured_population_queries_only()
    {
        string sourcePath = Path.Combine(
            SrcDir,
            "Zongzu.Modules.FamilyCore",
            "FamilyCoreModule.HouseholdBurdenEvents.cs");
        string source = File.ReadAllText(sourcePath);
        string familyModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.FamilyCore",
            "FamilyCoreModule.cs"));

        Assert.That(source, Does.Contain("IPopulationAndHouseholdsQueries"));
        Assert.That(source, Does.Contain("GetRequiredHousehold"));
        Assert.That(source, Does.Contain("SponsorClanId"));
        Assert.That(source, Does.Contain("PopulationEventNames.HouseholdDebtSpiked"));
        Assert.That(source, Does.Contain("PopulationEventNames.HouseholdSubsistencePressureChanged"));
        Assert.That(source, Does.Contain("PopulationEventNames.HouseholdBurdenIncreased"));
        Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(source, Does.Not.Contain(".Summary"));
        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        Assert.That(source, Does.Not.Contain("PopulationAndHouseholdsState"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(source, Does.Not.Contain("CooldownLedger"));
        Assert.That(familyModule, Does.Contain("PopulationEventNames.HouseholdDebtSpiked"));
        Assert.That(familyModule, Does.Contain("PopulationEventNames.HouseholdSubsistencePressureChanged"));
        Assert.That(familyModule, Does.Contain("PopulationEventNames.HouseholdBurdenIncreased"));
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
        Assert.That(source, Does.Not.Contain("LastRefusalResponseSummary"));
        Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
        Assert.That(source, Does.Not.Contain("社会其他人"));
        Assert.That(source, Does.Not.Contain("接手"));
        Assert.That(source, Does.Not.Contain("PlayerCommandService"));
        Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
        Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(source, Does.Not.Contain("PopulationAndHouseholdsState"));
        Assert.That(source, Does.Not.Contain("SocialMemoryAndRelationsState"));
        Assert.That(source, Does.Not.Contain(".Memories.Add"));
        Assert.That(source, Does.Not.Contain("memory.Summary"));
        Assert.That(source, Does.Not.Contain("OwnerLaneLedger"));
        Assert.That(source, Does.Not.Contain("FamilyClosureLedger"));
        Assert.That(source, Does.Not.Contain("CooldownLedger"));
        Assert.That(source, Does.Not.Contain("FollowUpLedger"));
        Assert.That(source, Does.Not.Contain("ReceiptStatusLedger"));
        Assert.That(source, Does.Not.Contain("OutcomeLedger"));
        Assert.That(source, Does.Not.Contain("HouseholdTarget"));
        Assert.That(source, Does.Not.Contain("ModuleSchemaVersion"));
        Assert.That(source, Does.Not.Contain("UpgradeFromSchema"));
        Assert.That(source, Does.Not.Contain("Migration"));
        Assert.That(source, Does.Contain("HouseholdPressureSnapshot"));
        Assert.That(source, Does.Contain("SettlementDisorderSnapshot"));
        Assert.That(source, Does.Contain("JurisdictionAuthoritySnapshot"));
        Assert.That(source, Does.Contain("ClanSnapshot"));
        Assert.That(source, Does.Contain("LastLocalResponseCommandCode"));
        Assert.That(source, Does.Contain("LastLocalResponseOutcomeCode"));
        Assert.That(source, Does.Contain("LastRefusalResponseCommandCode"));
        Assert.That(source, Does.Contain("LastRefusalResponseOutcomeCode"));
        Assert.That(source, Does.Contain("LocalResponseCarryoverMonths"));
        Assert.That(source, Does.Contain("SocialMemoryEntrySnapshot"));
        Assert.That(source, Does.Contain("MemoryLifecycleState.Active"));
        Assert.That(source, Does.Contain("CauseKey"));
        Assert.That(source, Does.Contain("外部后账归位"));
        Assert.That(source, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(source, Does.Contain("该走县门/文移 lane"));
        Assert.That(source, Does.Contain("该走族老/担保 lane"));
        Assert.That(source, Does.Contain("本户不能代修"));
        Assert.That(source, Does.Contain("BuildFamilyLaneClosureReadback"));
        Assert.That(source, Does.Contain("BuildFamilyLaneEntryReadbackSummary"));
        Assert.That(source, Does.Contain("BuildFamilyElderExplanationReadbackSummary"));
        Assert.That(source, Does.Contain("BuildFamilyGuaranteeReadbackSummary"));
        Assert.That(source, Does.Contain("BuildFamilyHouseFaceReadbackSummary"));
        Assert.That(source, Does.Contain("BuildFamilyLaneReceiptClosureSummary"));
        Assert.That(source, Does.Contain("BuildFamilyLaneResidueFollowUpSummary"));
        Assert.That(source, Does.Contain("BuildFamilyLaneNoLoopGuardSummary"));
        Assert.That(source, Does.Contain("Family承接入口"));
        Assert.That(source, Does.Contain("族老解释读回"));
        Assert.That(source, Does.Contain("本户担保读回"));
        Assert.That(source, Does.Contain("宗房脸面读回"));
        Assert.That(source, Does.Contain("Family后手收口读回"));
        Assert.That(source, Does.Contain("Family余味续接读回"));
        Assert.That(source, Does.Contain("Family闭环防回压"));
        Assert.That(source, Does.Contain("不是普通家户再扛"));
        Assert.That(source, Does.Contain("SponsorClanId"));
        Assert.That(source, Does.Contain("FamilyCore lane"));
        Assert.That(source, Does.Contain("承接入口"));
        Assert.That(source, Does.Contain("添雇巡丁"));
        Assert.That(source, Does.Contain("押文催县门"));
        Assert.That(source, Does.Contain("请族老解释"));
        Assert.That(source, Does.Contain("归口状态"));
        Assert.That(source, Does.Contain("已归口到巡丁/路匪 lane"));
        Assert.That(source, Does.Contain("已归口到县门/文移 lane"));
        Assert.That(source, Does.Contain("已归口到族老/担保 lane"));
        Assert.That(source, Does.Contain("归口不等于修好"));
        Assert.That(source, Does.Contain("仍看 owner lane 下月读回"));
        Assert.That(source, Does.Contain("BuildOwnerLaneOutcomeReading"));
        Assert.That(source, Does.Contain("归口后读法"));
        Assert.That(source, Does.Contain("已修复：先停本户加压"));
        Assert.That(source, Does.Contain("暂压留账：仍看本 lane 下月"));
        Assert.That(source, Does.Contain("恶化转硬：别让本户代扛"));
        Assert.That(source, Does.Contain("放置未接：仍回 owner lane"));
        Assert.That(source, Does.Contain("BuildOwnerLaneSocialResidueReadback"));
        Assert.That(source, Does.Contain("TryReadOwnerLaneSocialResidueCause"));
        Assert.That(source, Does.Contain("社会余味读回"));
        Assert.That(source, Does.Contain("后账渐平"));
        Assert.That(source, Does.Contain("后账暂压留账"));
        Assert.That(source, Does.Contain("后账转硬"));
        Assert.That(source, Does.Contain("后账放置发酸"));
        Assert.That(source, Does.Contain("仍由 SocialMemoryAndRelations 后续沉淀"));
        Assert.That(source, Does.Contain("不是本户再修"));
        Assert.That(source, Does.Contain("BuildOwnerLaneSocialResidueFollowUpGuidance"));
        Assert.That(source, Does.Contain("余味冷却提示"));
        Assert.That(source, Does.Contain("余味续接提示"));
        Assert.That(source, Does.Contain("余味换招提示"));
        Assert.That(source, Does.Contain("继续降温"));
        Assert.That(source, Does.Contain("别回压本户"));
        Assert.That(source, Does.Contain("不要从本户硬补"));
        Assert.That(source, Does.Contain("BuildOwnerLaneAffordanceEcho"));
        Assert.That(source, Does.Contain("现有入口读法"));
        Assert.That(source, Does.Contain("建议冷却"));
        Assert.That(source, Does.Contain("可轻续"));
        Assert.That(source, Does.Contain("建议换招"));
        Assert.That(source, Does.Contain("等待承接口"));
        Assert.That(source, Does.Contain("BuildOwnerLaneFollowUpReceiptClosure"));
        Assert.That(source, Does.Contain("后手收口读回"));
        Assert.That(source, Does.Contain("已收口：不回压本户"));
        Assert.That(source, Does.Contain("仍留账：轻续本 lane"));
        Assert.That(source, Does.Contain("转硬待换招"));
        Assert.That(source, Does.Contain("未接待承口"));
        Assert.That(source, Does.Contain("BuildOwnerLaneNoLoopGuard"));
        Assert.That(source, Does.Contain("闭环防回压"));
        Assert.That(source, Does.Contain("后账已收束"));
        Assert.That(source, Does.Contain("旧提示仅作读回"));
        Assert.That(source, Does.Contain("不重复追本户"));
        Assert.That(source, Does.Contain("PublicLifeOrderResponseOutcomeCodes.Repaired"));
        Assert.That(source, Does.Contain("PublicLifeOrderResponseOutcomeCodes.Contained"));
        Assert.That(source, Does.Contain("PublicLifeOrderResponseOutcomeCodes.Escalated"));
        Assert.That(source, Does.Contain("PublicLifeOrderResponseOutcomeCodes.Ignored"));
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
        Assert.That(source, Does.Not.Contain("归口后读法"));
        Assert.That(source, Does.Not.Contain("已修复：先停本户加压"));
        Assert.That(source, Does.Not.Contain("暂压留账"));
        Assert.That(source, Does.Not.Contain("恶化转硬"));
        Assert.That(source, Does.Not.Contain("放置未接"));
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
    public void Canal_window_owner_lane_handlers_must_use_structured_world_reads()
    {
        string tradeSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.TradeAndIndustry",
            "TradeAndIndustryModule",
            "TradeAndIndustryModule.CampaignEvents.cs"));
        string orderSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.OrderAndBanditry",
            "OrderAndBanditryModule",
            "OrderAndBanditryModule.cs"));

        foreach (string source in new[] { tradeSource, orderSource })
        {
            Assert.That(source, Does.Contain("WorldSettlementsEventNames.CanalWindowChanged"));
            Assert.That(source, Does.Contain("IWorldSettlementsQueries"));
            Assert.That(source, Does.Contain("DomainEventMetadataKeys.CanalWindowAfter"));
            Assert.That(source, Does.Not.Contain("domainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("LastLocalResponseSummary"));
            Assert.That(source, Does.Not.Contain("LastInterventionSummary"));
        }

        Assert.That(tradeSource, Does.Contain("TradeAndIndustryEventNames.RouteBusinessBlocked"));
        Assert.That(orderSource, Does.Contain("OrderAndBanditryEventNames.BlackRoutePressureRaised"));
        Assert.That(tradeSource, Does.Not.Contain("ModuleSchemaVersion => 5"));
        Assert.That(orderSource, Does.Not.Contain("ModuleSchemaVersion => 10"));
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

    [Test]
    public void Social_mobility_fidelity_ring_must_stay_owner_laned_projection_only_and_schema_neutral()
    {
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string personRegistryModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PersonRegistry",
            "PersonRegistryModule.cs"));
        string personRegistryCommands = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PersonRegistry",
            "PersonRegistryCommands.cs"));
        string mobilityProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Mobility.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));

        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(personRegistryModule, Does.Contain("ModuleSchemaVersion => 1"));
        Assert.That(personRegistryCommands, Does.Contain("ChangeFidelityRing"));
        Assert.That(personRegistryCommands, Does.Not.Contain("HouseholdId"));
        Assert.That(personRegistryCommands, Does.Not.Contain("LivelihoodType"));
        Assert.That(personRegistryCommands, Does.Not.Contain("PersonMovementLedger"));
        Assert.That(personRegistryCommands, Does.Not.Contain("SocialMobilityLedger"));

        Assert.That(populationModule, Does.Contain("TryApplyMonthlyLivelihoodDrift"));
        Assert.That(populationModule, Does.Contain("RebuildSettlementSummaries"));
        Assert.That(populationModule, Does.Contain("PromoteHotHouseholdMembers"));
        Assert.That(populationModule, Does.Contain("IPersonRegistryCommands"));
        Assert.That(populationModule, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(populationModule, Does.Not.Contain("PlayerCommandService"));
        Assert.That(populationModule, Does.Not.Contain("GetMutableModuleState"));

        Assert.That(mobilityProjection, Does.Contain("GetLaborPools"));
        Assert.That(mobilityProjection, Does.Contain("GetMarriagePools"));
        Assert.That(mobilityProjection, Does.Contain("GetMigrationPools"));
        Assert.That(mobilityProjection, Does.Contain("FidelityScaleSnapshot"));
        Assert.That(mobilityProjection, Does.Contain("SettlementMobilitySnapshot"));
        Assert.That(mobilityProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(mobilityProjection, Does.Not.Contain("PlayerCommandService"));
        Assert.That(mobilityProjection, Does.Not.Contain("GetMutableModuleState"));
        Assert.That(personDossierProjection, Does.Contain("MovementReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("FidelityRingReadbackSummary"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));

        string[] presentationSources =
        [
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "GreatHall", "GreatHallShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Debug", "DebugShellAdapter.cs"),
        ];
        foreach (string sourcePath in presentationSources)
        {
            string source = File.ReadAllText(sourcePath);
            Assert.That(source, Does.Not.Contain("Zongzu.Modules."), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Not.Contain("PlayerCommandService"), Path.GetFileName(sourcePath));
            Assert.That(source, Does.Not.Contain("GetMutableModuleState"), Path.GetFileName(sourcePath));
        }

        Assert.That(schemaRules, Does.Contain("v213-v244").Or.Contain("social mobility fidelity ring"));
        Assert.That(dataSchema, Does.Contain("v213-v244").Or.Contain("social mobility fidelity ring"));
    }

    [Test]
    public void Social_mobility_fidelity_ring_closeout_v245_v252_must_document_first_layer_only_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_social-mobility-fidelity-closeout-v245-v252.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_social-mobility-fidelity-closeout-v245-v252.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V245-V252 Social Mobility Fidelity Ring Closeout Audit"));
        Assert.That(topologyIndex, Does.Contain("first-layer substrate"));
        Assert.That(topologyIndex, Does.Contain("does not mean Zongzu has a full society engine"));
        Assert.That(designAudit, Does.Contain("v245-v252 social mobility fidelity-ring closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("social mobility fidelity ring closeout v245-v252 note"));
        Assert.That(integrationRules, Does.Contain("Social mobility fidelity ring closeout v245-v252 integration note"));
        Assert.That(acceptance, Does.Contain("Social mobility fidelity ring closeout v245-v252 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V252"));
        Assert.That(fidelityModel, Does.Contain("V245-V252 Closeout"));

        Assert.That(schemaRules, Does.Contain("social mobility fidelity ring closeout v245-v252 remains docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current social mobility fidelity ring closeout v245-v252 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("This pass must remain docs/tests only"));

        foreach (string forbidden in new[]
                 {
                     "MovementLedger",
                     "PersonMovementLedger",
                     "SocialMobilityLedger",
                     "FocusLedger",
                     "OwnerLaneLedger",
                     "SchedulerLedger",
                     "ProjectionCache",
                     "DormantStubStore",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
    }

    [Test]
    public void Social_mobility_scale_budget_guard_v269_v276_must_prevent_whole_world_person_simulation_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_social-mobility-scale-budget-guard-v269-v276.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_social-mobility-scale-budget-guard-v269-v276.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V269-V276 Social Mobility Scale Budget Guard"));
        Assert.That(designAudit, Does.Contain("v269-v276 social mobility scale-budget guard"));
        Assert.That(moduleBoundaries, Does.Contain("Social mobility scale-budget guard v269-v276 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social mobility scale-budget guard v269-v276 integration note"));
        Assert.That(simulation, Does.Contain("Current social mobility scale-budget guard v269-v276 note"));
        Assert.That(fidelityModel, Does.Contain("V269-V276 Scale Budget Guard"));
        Assert.That(uiPresentation, Does.Contain("v269-v276 scale-budget guard"));
        Assert.That(acceptance, Does.Contain("Social mobility scale budget guard v269-v276 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V276"));

        foreach (string band in new[]
                 {
                     "close-orbit named detail",
                     "influence/pressure selective detail",
                     "active-region structured pools",
                     "distant-world pressure summaries",
                 })
        {
            Assert.That(acceptance, Does.Contain(band), band);
        }

        Assert.That(schemaRules, Does.Contain("social mobility scale budget guard v269-v276 remains docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current social mobility scale budget guard v269-v276 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("This pass is docs/tests only"));
        Assert.That(execPlan, Does.Contain("No production rule change"));

        foreach (string forbidden in new[]
                 {
                     "WholeWorldPersonSimulation",
                     "GlobalPersonSimulation",
                     "AllWorldPersonSimulation",
                     "PerPersonWorldSimulation",
                     "EveryPersonEveryMonth",
                     "GlobalPersonTick",
                     "WorldPersonTick",
                     "WorldPopulationManager",
                     "PersonSimulationManager",
                     "MobilityManager",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "SocialMobilityLedger",
                     "FocusLedger",
                     "SchedulerLedger",
                     "v269-v276",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Social_mobility_influence_readback_v277_v284_must_stay_projection_only_and_schema_neutral()
    {
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "LivingSocietyReadModels.cs")) + Environment.NewLine + File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PersonDossierReadModels.cs"));
        string mobilityProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Mobility.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "GreatHall", "GreatHallShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Family", "PersonDossierViewModel.cs"),
        }.Select(File.ReadAllText));
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_social-mobility-influence-readback-v277-v284.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_social-mobility-influence-readback-v277-v284.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(readModelContracts, Does.Contain("InfluenceFootprintReadbackSummary"));
        Assert.That(readModelContracts, Does.Contain("ScaleBudgetReadbackSummary"));
        Assert.That(mobilityProjection, Does.Contain("BuildFidelityInfluenceFootprintReadback"));
        Assert.That(mobilityProjection, Does.Contain("BuildScaleBudgetReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("BuildPersonInfluenceFootprintReadbackSummary"));
        Assert.That(unitySource, Does.Contain("InfluenceFootprintReadbackSummary"));
        Assert.That(unitySource, Does.Contain("ScaleBudgetReadbackSummary"));

        foreach (string source in new[] { mobilityProjection, personDossierProjection, unitySource })
        {
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("domainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("PlayerCommandService"));
            Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
            Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        }

        foreach (string forbidden in new[]
                 {
                     "MovementLedger",
                     "PersonMovementLedger",
                     "SocialMobilityLedger",
                     "FocusLedger",
                     "SchedulerLedger",
                     "GlobalPersonSimulation",
                     "EveryPersonEveryMonth",
                     "WorldPersonTick",
                     "WorldPopulationManager",
                     "PersonSimulationManager",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(topologyIndex, Does.Contain("V277-V284 Social Mobility Influence Readback"));
        Assert.That(designAudit, Does.Contain("v277-v284 social mobility influence readback"));
        Assert.That(moduleBoundaries, Does.Contain("Social mobility influence readback v277-v284 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social mobility influence readback v277-v284 integration note"));
        Assert.That(simulation, Does.Contain("Current social mobility influence readback v277-v284 note"));
        Assert.That(fidelityModel, Does.Contain("V277-V284 Influence Readback"));
        Assert.That(uiPresentation, Does.Contain("v277-v284 influence readback"));
        Assert.That(acceptance, Does.Contain("Social mobility influence readback v277-v284 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V284"));
        Assert.That(schemaRules, Does.Contain("social mobility influence readback v277-v284 remains read-model/ViewModel only"));
        Assert.That(dataSchema, Does.Contain("Current social mobility influence readback v277-v284 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No new player command"));
    }

    [Test]
    public void Social_mobility_boundary_closeout_v285_v292_must_document_first_layer_only_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_social-mobility-boundary-closeout-v285-v292.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_social-mobility-boundary-closeout-v285-v292.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));
        string mobilityProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Mobility.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "GreatHall", "GreatHallShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Family", "PersonDossierViewModel.cs"),
        }.Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V285-V292 Social Mobility Boundary Closeout Audit"));
        Assert.That(designAudit, Does.Contain("v285-v292 social mobility boundary closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Social mobility boundary closeout v285-v292 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social mobility boundary closeout v285-v292 integration note"));
        Assert.That(simulation, Does.Contain("Current social mobility boundary closeout v285-v292 note"));
        Assert.That(fidelityModel, Does.Contain("V285-V292 Boundary Closeout"));
        Assert.That(uiPresentation, Does.Contain("v285-v292 boundary closeout"));
        Assert.That(acceptance, Does.Contain("Social mobility boundary closeout v285-v292 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V292"));
        Assert.That(schemaRules, Does.Contain("social mobility boundary closeout v285-v292 remains docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current social mobility boundary closeout v285-v292 note"));

        foreach (string expectedDebt in new[]
                 {
                     "full migration economy",
                     "direct personnel commands",
                     "dormant stubs",
                     "durable movement residue",
                     "cross-region personnel flow",
                 })
        {
            Assert.That(acceptance, Does.Contain(expectedDebt), expectedDebt);
            Assert.That(execPlan, Does.Contain(expectedDebt), expectedDebt);
        }

        foreach (string expectedBand in new[]
                 {
                     "near detail",
                     "pressure-selected local detail",
                     "active-region pools",
                     "distant pressure summary",
                 })
        {
            Assert.That(topologyIndex, Does.Contain(expectedBand), expectedBand);
            Assert.That(execPlan, Does.Contain(expectedBand), expectedBand);
        }

        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("This pass is docs/tests only"));
        Assert.That(execPlan, Does.Contain("No direct player command for moving people"));

        foreach (string source in new[] { mobilityProjection, personDossierProjection, unitySource })
        {
            Assert.That(source, Does.Not.Contain("DomainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("domainEvent.Summary"));
            Assert.That(source, Does.Not.Contain("IssueModuleCommand"));
            Assert.That(source, Does.Not.Contain("GetMutableModuleState"));
        }

        foreach (string forbidden in new[]
                 {
                     "WholeWorldPersonSimulation",
                     "GlobalPersonSimulation",
                     "AllWorldPersonSimulation",
                     "PerPersonWorldSimulation",
                     "EveryPersonEveryMonth",
                     "GlobalPersonTick",
                     "WorldPersonTick",
                     "WorldPopulationManager",
                     "PersonSimulationManager",
                     "MobilityManager",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "SocialMobilityLedger",
                     "FocusLedger",
                     "SchedulerLedger",
                     "PersonnelLedger",
                     "PersonCommandLedger",
                     "ProjectionCache",
                     "DormantStubStore",
                     "DirectPersonnelCommand",
                     "MovePersonCommand",
                     "TransferPersonCommand",
                     "v285-v292",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_command_preflight_v293_v300_must_block_direct_personnel_command_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-command-preflight-v293-v300.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-command-preflight-v293-v300.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string playerCommandNames = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string playerCommandCatalog = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PlayerCommandService",
            "PlayerCommandCatalog.cs"));
        string playerCommandService = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PlayerCommandService",
            "PlayerCommandService.cs"));
        string populationResolver = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsCommandResolver.cs"));
        string personRegistryCommands = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PersonRegistry",
            "PersonRegistryCommands.cs"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V293-V300 Personnel Command Preflight"));
        Assert.That(playerScope, Does.Contain("Personnel flow preflight"));
        Assert.That(designAudit, Does.Contain("v293-v300 personnel command preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel command preflight v293-v300 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel command preflight v293-v300 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel command preflight v293-v300 note"));
        Assert.That(fidelityModel, Does.Contain("V293-V300 Personnel Command Preflight"));
        Assert.That(uiPresentation, Does.Contain("v293-v300 personnel command preflight"));
        Assert.That(acceptance, Does.Contain("Personnel command preflight v293-v300 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V300"));
        Assert.That(schemaRules, Does.Contain("personnel command preflight v293-v300 remains docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current personnel command preflight v293-v300 note"));

        foreach (string expectedGate in new[]
                 {
                     "owner module",
                     "target scope",
                     "hot path",
                     "expected cardinality",
                     "deterministic cap/order",
                     "no-touch boundary",
                     "schema impact",
                     "validation lane",
                 })
        {
            Assert.That(acceptance, Does.Contain(expectedGate), expectedGate);
            Assert.That(execPlan, Does.Contain(expectedGate), expectedGate);
        }

        Assert.That(playerCommandCatalog, Does.Contain("KnownModuleKeys.PopulationAndHouseholds"));
        Assert.That(playerCommandCatalog, Does.Contain("RestrictNightTravel"));
        Assert.That(playerCommandCatalog, Does.Contain("PoolRunnerCompensation"));
        Assert.That(playerCommandCatalog, Does.Contain("SendHouseholdRoadMessage"));
        Assert.That(playerCommandService, Does.Contain("simulation.IssueModuleCommand(route.ModuleKey, command)"));
        Assert.That(populationResolver, Does.Contain("PopulationAndHouseholds does not handle player command"));
        Assert.That(personRegistryCommands, Does.Contain("ChangeFidelityRing"));

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonCommandLedger",
                     "PersonnelLedger",
                     "AssignmentLedger",
                     "PersonAssignmentLedger",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "v293-v300",
                 })
        {
            Assert.That(playerCommandNames, Does.Not.Contain(forbidden), forbidden);
            Assert.That(playerCommandCatalog, Does.Not.Contain(forbidden), forbidden);
            Assert.That(playerCommandService, Does.Not.Contain(forbidden), forbidden);
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        foreach (string forbiddenRegistryDomain in new[]
                 {
                     "PlayerCommandRequest",
                     "HouseholdId",
                     "SettlementId",
                     "MigrationRisk",
                     "Livelihood",
                     "OfficeCareer",
                     "Campaign",
                 })
        {
            Assert.That(personRegistryCommands, Does.Not.Contain(forbiddenRegistryDomain), forbiddenRegistryDomain);
        }

        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No new player command"));
        Assert.That(execPlan, Does.Contain("This pass is docs/tests only"));
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_command_readiness_v301_v308_must_stay_population_projection_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-command-readiness-v301-v308.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-command-readiness-v301-v308.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string localResponseProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse.cs"));
        string commandMetadata = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PlayerCommands.Metadata.cs"));
        string playerCommandNames = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string playerCommandCatalog = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PlayerCommandService",
            "PlayerCommandCatalog.cs"));
        string playerCommandService = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PlayerCommandService",
            "PlayerCommandService.cs"));
        string personRegistryCommands = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PersonRegistry",
            "PersonRegistryCommands.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Shared", "CommandShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Shared", "CommandAffordanceViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Shared", "CommandReceiptViewModel.cs"),
        }.Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V301-V308 Personnel Flow Command Readiness"));
        Assert.That(playerScope, Does.Contain("v301-v308 adds only `PersonnelFlowReadinessSummary`"));
        Assert.That(designAudit, Does.Contain("v301-v308 personnel flow command readiness audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow command readiness v301-v308 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow command readiness v301-v308 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow command readiness v301-v308 note"));
        Assert.That(fidelityModel, Does.Contain("V301-V308 Personnel Flow Command Readiness"));
        Assert.That(uiPresentation, Does.Contain("v301-v308 personnel flow command readiness"));
        Assert.That(acceptance, Does.Contain("Personnel flow command readiness v301-v308 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V308"));
        Assert.That(schemaRules, Does.Contain("personnel flow command readiness v301-v308 remains read-model/ViewModel only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow command readiness v301-v308 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(readModelContracts, Does.Contain("PersonnelFlowReadinessSummary"));
        Assert.That(commandMetadata, Does.Contain("PersonnelFlowReadinessSummary ="));
        Assert.That(localResponseProjection, Does.Contain("BuildHomeHouseholdPersonnelFlowReadinessSummary"));
        Assert.That(localResponseProjection, Does.Contain("RestrictNightTravel"));
        Assert.That(localResponseProjection, Does.Contain("PoolRunnerCompensation"));
        Assert.That(localResponseProjection, Does.Contain("SendHouseholdRoadMessage"));

        int projectionStart = localResponseProjection.IndexOf("private static string BuildHomeHouseholdPersonnelFlowReadinessSummary", StringComparison.Ordinal);
        int projectionEnd = localResponseProjection.IndexOf("private static HouseholdLocalResponseAffordanceCapacity", projectionStart, StringComparison.Ordinal);
        Assert.That(projectionStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(projectionEnd, Is.GreaterThan(projectionStart));
        string readinessMethod = localResponseProjection[projectionStart..projectionEnd];

        foreach (string token in new[]
                 {
                     "人员流动预备读回",
                     "近处细读",
                     "远处汇总",
                     "只影响本户生计/丁力/迁徙之念",
                     "不是直接调人、迁人、召人命令",
                     "PopulationAndHouseholds拥有本户回应",
                     "PersonRegistry只保身份/FidelityRing",
                     "UI/Unity只复制投影字段",
                 })
        {
            Assert.That(readinessMethod, Does.Contain(token), token);
            Assert.That(execPlan, Does.Contain(token), token);
        }

        foreach (string forbiddenParser in new[]
                 {
                     "DomainEvent.Summary",
                     "LastLocalResponseSummary",
                     ".Summary.Contains",
                     ".ReadbackSummary.Contains",
                 })
        {
            Assert.That(readinessMethod, Does.Not.Contain(forbiddenParser), forbiddenParser);
        }

        Assert.That(playerCommandCatalog, Does.Contain("KnownModuleKeys.PopulationAndHouseholds"));
        Assert.That(playerCommandService, Does.Contain("simulation.IssueModuleCommand(route.ModuleKey, command)"));
        Assert.That(personRegistryCommands, Does.Contain("ChangeFidelityRing"));
        Assert.That(unitySource, Does.Contain("PersonnelFlowReadinessSummary = command.PersonnelFlowReadinessSummary"));
        Assert.That(unitySource, Does.Contain("PersonnelFlowReadinessSummary = receipt.PersonnelFlowReadinessSummary"));
        Assert.That(unitySource, Does.Not.Contain("BuildHomeHouseholdPersonnelFlowReadinessSummary"));

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonCommandLedger",
                     "PersonnelLedger",
                     "AssignmentLedger",
                     "PersonAssignmentLedger",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "v301-v308",
                 })
        {
            Assert.That(playerCommandNames, Does.Not.Contain(forbidden), forbidden);
            Assert.That(playerCommandCatalog, Does.Not.Contain(forbidden), forbidden);
            Assert.That(playerCommandService, Does.Not.Contain(forbidden), forbidden);
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        foreach (string forbiddenRegistryDomain in new[]
                 {
                     "PlayerCommandRequest",
                     "HouseholdId",
                     "SettlementId",
                     "MigrationRisk",
                     "Livelihood",
                     "OfficeCareer",
                     "Campaign",
                 })
        {
            Assert.That(personRegistryCommands, Does.Not.Contain(forbiddenRegistryDomain), forbiddenRegistryDomain);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_surface_echo_v309_v316_must_stay_structured_projection_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-surface-echo-v309-v316.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-surface-echo-v309-v316.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string builder = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.cs"));
        string greatHallAdapter = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "GreatHall",
            "GreatHallShellAdapter.cs"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V309-V316 Personnel Flow Surface Echo"));
        Assert.That(designAudit, Does.Contain("v309-v316 personnel flow surface echo audit"));
        Assert.That(integrationRules, Does.Contain("Personnel flow surface echo v309-v316 integration note"));
        Assert.That(uiPresentation, Does.Contain("v309-v316 personnel flow surface echo"));
        Assert.That(acceptance, Does.Contain("Personnel flow surface echo v309-v316 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V316"));
        Assert.That(schemaRules, Does.Contain("personnel flow surface echo v309-v316 remains read-model/ViewModel only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow surface echo v309-v316 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(readModelContracts, Does.Contain("PlayerCommandSurfaceSnapshot"));
        Assert.That(readModelContracts, Does.Contain("public string PersonnelFlowReadinessSummary { get; init; } = string.Empty;"));
        Assert.That(builder, Does.Contain("BuildPlayerCommandSurfacePersonnelFlowReadinessSummary"));
        Assert.That(builder, Does.Contain("affordance.PersonnelFlowReadinessSummary"));
        Assert.That(builder, Does.Contain("receipt.PersonnelFlowReadinessSummary"));
        Assert.That(builder, Does.Contain("人员流动命令预备汇总"));
        Assert.That(builder, Does.Contain("只汇总已投影的人员流动预备读回"));
        Assert.That(builder, Does.Contain("不解析ReadbackSummary"));
        Assert.That(greatHallAdapter, Does.Contain("bundle.PlayerCommands.PersonnelFlowReadinessSummary"));
        Assert.That(greatHallAdapter, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(greatHallAdapter, Does.Not.Contain(".ReadbackSummary.Contains"));

        foreach (string forbiddenParser in new[]
                 {
                     "DomainEvent.Summary",
                     ".ReadbackSummary.Contains",
                     ".Summary.Contains",
                     "LastLocalResponseSummary",
                 })
        {
            Assert.That(builder, Does.Not.Contain(forbiddenParser), forbiddenParser);
        }

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonCommandLedger",
                     "PersonnelLedger",
                     "AssignmentLedger",
                     "PersonAssignmentLedger",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "SurfaceEchoLedger",
                     "v309-v316",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_readiness_closeout_v317_v324_must_stay_docs_tests_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-readiness-closeout-v317-v324.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-readiness-closeout-v317-v324.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string playerCommandCatalog = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PlayerCommandService",
            "PlayerCommandCatalog.cs"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V317-V324 Personnel Flow Readiness Closeout Audit"));
        Assert.That(playerScope, Does.Contain("v317-v324 closes that branch as first-layer readiness only"));
        Assert.That(designAudit, Does.Contain("v317-v324 personnel flow readiness closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow readiness closeout v317-v324 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow readiness closeout v317-v324 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow readiness closeout v317-v324 note"));
        Assert.That(fidelityModel, Does.Contain("V317-V324 Personnel Flow Readiness Closeout"));
        Assert.That(uiPresentation, Does.Contain("v317-v324 personnel flow readiness closeout"));
        Assert.That(acceptance, Does.Contain("Personnel flow readiness closeout v317-v324 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V324"));
        Assert.That(schemaRules, Does.Contain("personnel flow readiness closeout v317-v324 is docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow readiness closeout v317-v324 note"));
        Assert.That(execPlan, Does.Contain("Close v293-v316 as a first personnel-flow command-readiness layer"));
        Assert.That(execPlan, Does.Contain("No runtime rule change"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(readModelContracts, Does.Contain("PersonnelFlowReadinessSummary"));
        Assert.That(readModelContracts, Does.Contain("PlayerCommandSurfaceSnapshot"));
        Assert.That(playerCommandCatalog, Does.Contain("RestrictNightTravel"));
        Assert.That(playerCommandCatalog, Does.Contain("PoolRunnerCompensation"));
        Assert.That(playerCommandCatalog, Does.Contain("SendHouseholdRoadMessage"));

        foreach (string closeoutToken in new[]
                 {
                     "first personnel-flow command-readiness layer",
                     "不是完整迁徙系统",
                     "不是完整社会流动引擎",
                     "不是直接调人、迁人、召人命令",
                     "PopulationAndHouseholds owns household response",
                     "PersonRegistry owns identity/FidelityRing only",
                     "Application/Unity display projected fields only",
                     "schema/migration impact: none",
                 })
        {
            Assert.That(execPlan, Does.Contain(closeoutToken), closeoutToken);
        }

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonCommandLedger",
                     "PersonnelLedger",
                     "AssignmentLedger",
                     "PersonAssignmentLedger",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "SurfaceEchoLedger",
                     "CloseoutLedger",
                     "v317-v324",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_owner_lane_gate_v325_v332_must_stay_projection_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-owner-lane-gate-v325-v332.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-owner-lane-gate-v325-v332.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string builder = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.cs"));
        string greatHallAdapter = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "GreatHall",
            "GreatHallShellAdapter.cs"));
        string playerCommandCatalog = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PlayerCommandService",
            "PlayerCommandCatalog.cs"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V325-V332 Personnel Flow Owner-Lane Gate"));
        Assert.That(playerScope, Does.Contain("v325-v332 adds only an owner-lane gate readback"));
        Assert.That(designAudit, Does.Contain("v325-v332 personnel flow owner-lane gate audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow owner-lane gate v325-v332 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow owner-lane gate v325-v332 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow owner-lane gate v325-v332 note"));
        Assert.That(uiPresentation, Does.Contain("v325-v332 personnel flow owner-lane gate"));
        Assert.That(acceptance, Does.Contain("Personnel flow owner-lane gate v325-v332 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V332"));
        Assert.That(schemaRules, Does.Contain("personnel flow owner-lane gate v325-v332 remains read-model/ViewModel only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow owner-lane gate v325-v332 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(readModelContracts, Does.Contain("PersonnelFlowOwnerLaneGateSummary"));
        Assert.That(builder, Does.Contain("BuildPlayerCommandSurfacePersonnelFlowOwnerLaneGateSummary"));
        Assert.That(builder, Does.Contain("affordance.PersonnelFlowReadinessSummary"));
        Assert.That(builder, Does.Contain("receipt.PersonnelFlowReadinessSummary"));
        Assert.That(builder, Does.Contain("人员流动归口门槛"));
        Assert.That(builder, Does.Contain("当前可读归口为PopulationAndHouseholds本户回应"));
        Assert.That(builder, Does.Contain("FamilyCore亲族调处"));
        Assert.That(builder, Does.Contain("OfficeAndCareer文书役使"));
        Assert.That(builder, Does.Contain("WarfareCampaign军务人力"));
        Assert.That(builder, Does.Contain("另开owner-lane计划"));
        Assert.That(greatHallAdapter, Does.Contain("bundle.PlayerCommands.PersonnelFlowOwnerLaneGateSummary"));
        Assert.That(greatHallAdapter, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(playerCommandCatalog, Does.Contain("RestrictNightTravel"));
        Assert.That(playerCommandCatalog, Does.Contain("PoolRunnerCompensation"));
        Assert.That(playerCommandCatalog, Does.Contain("SendHouseholdRoadMessage"));

        foreach (string forbiddenParser in new[]
                 {
                     "DomainEvent.Summary",
                     ".ReadbackSummary.Contains",
                     ".Summary.Contains",
                     "LastLocalResponseSummary",
                 })
        {
            Assert.That(builder, Does.Not.Contain(forbiddenParser), forbiddenParser);
        }

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonCommandLedger",
                     "PersonnelLedger",
                     "AssignmentLedger",
                     "PersonAssignmentLedger",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "OwnerLaneGateLedger",
                     "v325-v332",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_desk_gate_echo_v333_v340_must_stay_local_projection_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-desk-gate-echo-v333-v340.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-desk-gate-echo-v333-v340.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string deskAdapter = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "DeskSandbox",
            "DeskSandboxShellAdapter.cs"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V333-V340 Personnel Flow Desk Gate Echo"));
        Assert.That(playerScope, Does.Contain("v333-v340 adds only a Desk Sandbox local echo"));
        Assert.That(designAudit, Does.Contain("v333-v340 personnel flow desk gate echo audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow desk gate echo v333-v340 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow desk gate echo v333-v340 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow desk gate echo v333-v340 note"));
        Assert.That(uiPresentation, Does.Contain("v333-v340 personnel flow desk gate echo"));
        Assert.That(acceptance, Does.Contain("Personnel flow desk gate echo v333-v340 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V340"));
        Assert.That(schemaRules, Does.Contain("personnel flow desk gate echo v333-v340 remains presentation-only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow desk gate echo v333-v340 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(deskAdapter, Does.Contain("BuildSettlementPersonnelFlowOwnerLaneGateEcho"));
        Assert.That(deskAdapter, Does.Contain("PlayerCommandSurfaceKeys.PublicLife"));
        Assert.That(deskAdapter, Does.Contain("EnumerateAffordances"));
        Assert.That(deskAdapter, Does.Contain("EnumerateReceipts"));
        Assert.That(deskAdapter, Does.Contain("PersonnelFlowReadinessSummary"));
        Assert.That(deskAdapter, Does.Contain("PersonnelFlowOwnerLaneGateSummary"));
        Assert.That(deskAdapter, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(deskAdapter, Does.Not.Contain(".ReadbackSummary.Contains"));
        Assert.That(deskAdapter, Does.Not.Contain(".Summary.Contains"));

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonCommandLedger",
                     "PersonnelLedger",
                     "AssignmentLedger",
                     "PersonAssignmentLedger",
                     "MovementLedger",
                     "PersonMovementLedger",
                     "DeskGateLedger",
                     "v333-v340",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_desk_gate_containment_v341_v348_must_stay_local_projection_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-desk-gate-containment-v341-v348.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-desk-gate-containment-v341-v348.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string deskAdapter = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "DeskSandbox",
            "DeskSandboxShellAdapter.cs"));
        string presentationTests = File.ReadAllText(Path.Combine(
            TestsDir,
            "Zongzu.Presentation.Unity.Tests",
            "FirstPassPresentationShellTests",
            "FirstPassPresentationShellTests.cs"));

        Assert.That(topologyIndex, Does.Contain("V341-V348 Personnel Flow Desk Gate Containment"));
        Assert.That(designAudit, Does.Contain("v341-v348 personnel flow desk gate containment audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow desk gate containment v341-v348 boundary note"));
        Assert.That(uiPresentation, Does.Contain("v341-v348 personnel flow desk gate containment"));
        Assert.That(acceptance, Does.Contain("Personnel flow desk gate containment v341-v348 acceptance"));
        Assert.That(schemaRules, Does.Contain("personnel flow desk gate containment v341-v348 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow desk gate containment v341-v348 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(deskAdapter, Does.Contain("BuildSettlementPersonnelFlowOwnerLaneGateEcho"));
        Assert.That(deskAdapter, Does.Contain("EnumerateAffordances(PlayerCommandSurfaceKeys.PublicLife, settlementId)"));
        Assert.That(deskAdapter, Does.Contain("EnumerateReceipts(PlayerCommandSurfaceKeys.PublicLife, settlementId)"));
        Assert.That(deskAdapter, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(deskAdapter, Does.Not.Contain(".ReadbackSummary.Contains"));
        Assert.That(deskAdapter, Does.Not.Contain(".Summary.Contains"));

        Assert.That(presentationTests, Does.Contain("Compose_DoesNotEchoPersonnelFlowGateToDeskSettlementWithoutLocalReadiness"));
        Assert.That(presentationTests, Does.Contain("Does.Not.Contain(\"personnel gate local marker\")"));
    }

    [Test]
    public void Personnel_flow_gate_closeout_v349_v356_must_document_first_gate_layer_without_new_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-gate-closeout-v349-v356.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-gate-closeout-v349-v356.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V325-V332 Personnel Flow Owner-Lane Gate"));
        Assert.That(topologyIndex, Does.Contain("V333-V340 Personnel Flow Desk Gate Echo"));
        Assert.That(topologyIndex, Does.Contain("V341-V348 Personnel Flow Desk Gate Containment"));
        Assert.That(topologyIndex, Does.Contain("V349-V356 Personnel Flow Gate Closeout Audit"));
        Assert.That(designAudit, Does.Contain("v349-v356 personnel flow gate closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow gate closeout v349-v356 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow gate closeout v349-v356 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow gate closeout v349-v356 note"));
        Assert.That(uiPresentation, Does.Contain("v349-v356 personnel flow gate closeout"));
        Assert.That(acceptance, Does.Contain("Personnel flow gate closeout v349-v356 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V349-V356 Personnel Flow Gate Closeout"));
        Assert.That(schemaRules, Does.Contain("personnel flow gate closeout v349-v356 is docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow gate closeout v349-v356 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "PersonnelCommandResolver",
                     "PersonnelFlowGateLedger",
                     "OwnerLaneGateLedger",
                     "DeskGateLedger",
                     "MovementResolver",
                     "v349-v356",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_future_owner_lane_preflight_v357_v364_must_block_unplanned_lane_expansion()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-future-owner-lane-preflight-v357-v364.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-future-owner-lane-preflight-v357-v364.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V357-V364 Personnel Flow Future Owner-Lane Preflight"));
        Assert.That(playerScope, Does.Contain("v357-v364 blocks future owner-lane expansion"));
        Assert.That(designAudit, Does.Contain("v357-v364 personnel flow future owner-lane preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow future owner-lane preflight v357-v364 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow future owner-lane preflight v357-v364 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow future owner-lane preflight v357-v364 note"));
        Assert.That(uiPresentation, Does.Contain("v357-v364 personnel flow future owner-lane preflight"));
        Assert.That(acceptance, Does.Contain("Personnel flow future owner-lane preflight v357-v364 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V357-V364 Personnel Flow Future Owner-Lane Preflight"));
        Assert.That(schemaRules, Does.Contain("personnel flow future owner-lane preflight v357-v364 is docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow future owner-lane preflight v357-v364 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("owner module and accepted command"));
        Assert.That(execPlan, Does.Contain("hot path and expected cardinality"));
        Assert.That(execPlan, Does.Contain("deterministic ordering and cap"));

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "OfficeServicePersonnelCommand",
                     "FamilyPersonnelCommand",
                     "CampaignManpowerCommand",
                     "PersonnelCommandResolver",
                     "PersonnelFutureOwnerLaneLedger",
                     "FutureOwnerLaneLedger",
                     "MovementResolver",
                     "v357-v364",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_future_owner_lane_surface_v365_v372_must_stay_projection_only_and_schema_neutral()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string contractsSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PlayerCommandReadModels.cs"));
        string builderSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.cs"));
        string greatHallSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "GreatHall",
            "GreatHallShellAdapter.cs"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-future-lane-surface-v365-v372.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-future-lane-surface-v365-v372.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        int helperStart = builderSource.IndexOf("private static string BuildPlayerCommandSurfacePersonnelFlowFutureOwnerLanePreflightSummary", StringComparison.Ordinal);
        int helperEnd = builderSource.IndexOf("\n}", helperStart, StringComparison.Ordinal);
        Assert.That(helperStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(helperEnd, Is.GreaterThan(helperStart));
        string helperSource = builderSource[helperStart..helperEnd];

        Assert.That(topologyIndex, Does.Contain("V365-V372 Personnel Flow Future Lane Surface"));
        Assert.That(playerScope, Does.Contain("v365-v372 surfaces future owner-lane preflight"));
        Assert.That(designAudit, Does.Contain("v365-v372 personnel flow future lane surface audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow future lane surface v365-v372 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow future lane surface v365-v372 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow future lane surface v365-v372 note"));
        Assert.That(uiPresentation, Does.Contain("v365-v372 personnel flow future lane surface"));
        Assert.That(acceptance, Does.Contain("Personnel flow future lane surface v365-v372 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V365-V372 Personnel Flow Future Lane Surface"));
        Assert.That(schemaRules, Does.Contain("personnel flow future lane surface v365-v372 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow future lane surface v365-v372 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));

        Assert.That(contractsSource, Does.Contain("PersonnelFlowFutureOwnerLanePreflightSummary"));
        Assert.That(builderSource, Does.Contain("BuildPlayerCommandSurfacePersonnelFlowFutureOwnerLanePreflightSummary"));
        Assert.That(helperSource, Does.Contain("PersonnelFlowReadinessSummary"));
        Assert.That(helperSource, Does.Contain("FamilyCore/OfficeAndCareer/WarfareCampaign"));
        Assert.That(helperSource, Does.Contain("owner module"));
        Assert.That(helperSource, Does.Contain("accepted command"));
        Assert.That(helperSource, Does.Contain("schema impact"));
        Assert.That(greatHallSource, Does.Contain("bundle.PlayerCommands.PersonnelFlowFutureOwnerLanePreflightSummary"));

        foreach (string forbidden in new[]
                 {
                     "ReadbackSummary",
                     "DomainEvent.Summary",
                     "domainEvent.Summary",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "GetMutableModuleState",
                     "PlayerCommandService",
                     "IssueModuleCommand",
                 })
        {
            Assert.That(helperSource, Does.Not.Contain(forbidden), forbidden);
        }

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     "domainEvent.Summary",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "GetMutableModuleState",
                     "PlayerCommandService",
                     "IssueModuleCommand",
                 })
        {
            Assert.That(greatHallSource, Does.Not.Contain(forbidden), forbidden);
        }

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "OfficeServicePersonnelCommand",
                     "FamilyPersonnelCommand",
                     "CampaignManpowerCommand",
                     "PersonnelCommandResolver",
                     "PersonnelFutureOwnerLaneLedger",
                     "FutureOwnerLaneLedger",
                     "FutureLaneSurfaceLedger",
                     "MovementResolver",
                     "v365-v372",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Personnel_flow_future_lane_closeout_v373_v380_must_document_surface_only_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string playerScope = File.ReadAllText(Path.Combine(RepoRoot, "docs", "PLAYER_SCOPE.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_personnel-flow-future-lane-closeout-v373-v380.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_personnel-flow-future-lane-closeout-v373-v380.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V373-V380 Personnel Flow Future Lane Closeout Audit"));
        Assert.That(playerScope, Does.Contain("v373-v380 closes that preflight/display layer as guidance only"));
        Assert.That(designAudit, Does.Contain("v373-v380 personnel flow future lane closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Personnel flow future lane closeout v373-v380 boundary note"));
        Assert.That(integrationRules, Does.Contain("Personnel flow future lane closeout v373-v380 integration note"));
        Assert.That(simulation, Does.Contain("Current personnel flow future lane closeout v373-v380 note"));
        Assert.That(uiPresentation, Does.Contain("v373-v380 personnel flow future lane closeout"));
        Assert.That(acceptance, Does.Contain("Personnel flow future lane closeout v373-v380 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V373-V380 Personnel Flow Future Lane Closeout"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V380"));
        Assert.That(schemaRules, Does.Contain("personnel flow future lane closeout v373-v380 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current personnel flow future lane closeout v373-v380 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("v357-v364: documents the future owner-lane contract"));
        Assert.That(execPlan, Does.Contain("v365-v372: surfaces that contract as Great Hall readback"));

        foreach (string futureDebt in new[]
                 {
                     "direct movement",
                     "office-service lane",
                     "kin-transfer lane",
                     "campaign-manpower lane",
                     "durable movement residue",
                     "full social mobility engine",
                 })
        {
            Assert.That(topologyIndex, Does.Contain(futureDebt), futureDebt);
            Assert.That(acceptance, Does.Contain(futureDebt), futureDebt);
        }

        foreach (string forbidden in new[]
                 {
                     "MovePerson",
                     "TransferPerson",
                     "SummonPerson",
                     "AssignPerson",
                     "RelocatePerson",
                     "DirectPersonnelCommand",
                     "OfficeServicePersonnelCommand",
                     "FamilyPersonnelCommand",
                     "CampaignManpowerCommand",
                     "PersonnelCommandResolver",
                     "PersonnelFutureOwnerLaneLedger",
                     "FutureOwnerLaneLedger",
                     "FutureLaneSurfaceLedger",
                     "FutureLaneCloseoutLedger",
                     "MovementResolver",
                     "v373-v380",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Migration*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Commoner_social_position_preflight_v381_v388_must_document_future_lane_without_class_engine_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_commoner-social-position-preflight-v381-v388.md"));
        string personDossierSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V381-V388 Commoner Social Position Preflight"));
        Assert.That(socialStrata, Does.Contain("Current implementation preflight: v381-v388"));
        Assert.That(designAudit, Does.Contain("v381-v388 commoner social position preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Commoner social position preflight v381-v388 boundary note"));
        Assert.That(integrationRules, Does.Contain("Commoner social position preflight v381-v388 integration note"));
        Assert.That(simulation, Does.Contain("Current commoner social position preflight v381-v388 note"));
        Assert.That(uiPresentation, Does.Contain("v381-v388 commoner social position preflight"));
        Assert.That(acceptance, Does.Contain("Commoner social position preflight v381-v388 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V381-V388 Commoner Social Position Preflight"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V388"));
        Assert.That(schemaRules, Does.Contain("commoner social position preflight v381-v388 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current commoner social position preflight v381-v388 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No complete class engine"));
        Assert.That(execPlan, Does.Contain("No direct promote/demote commoner command"));

        foreach (string futureRequirement in new[]
                 {
                     "owner module",
                     "pressure carrier",
                     "target scope",
                     "hot path",
                     "expected cardinality",
                     "deterministic order/cap",
                     "schema impact",
                     "projection fields",
                 })
        {
            Assert.That(acceptance, Does.Contain(futureRequirement), futureRequirement);
            Assert.That(execPlan, Does.Contain(futureRequirement), futureRequirement);
        }

        Assert.That(personDossierSource, Does.Contain("BuildSocialPositionLabel"));
        Assert.That(personDossierSource, Does.Contain("KnownModuleKeys.PopulationAndHouseholds"));
        Assert.That(personDossierSource, Does.Contain("KnownModuleKeys.EducationAndExams"));
        Assert.That(personDossierSource, Does.Contain("KnownModuleKeys.TradeAndIndustry"));
        Assert.That(personDossierSource, Does.Contain("KnownModuleKeys.OfficeAndCareer"));
        Assert.That(personDossierSource, Does.Not.Contain("Parse"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "PromotePerson",
                     "DemotePerson",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "CommonerClassResolver",
                     "SocialClassResolver",
                     "DirectPromoteCommand",
                     "DirectDemoteCommand",
                     "PerPersonCareerSimulationManager",
                     "v381-v388",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Commoner_social_position_readback_v389_v396_must_copy_structured_dossier_projection_without_class_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_commoner-social-position-readback-v389-v396.md"));
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PersonDossierReadModels.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(RepoRoot, "unity", "Zongzu.UnityShell", "Assets", "Scripts", "ReadModels", "ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V389-V396 Commoner Social Position Readback"));
        Assert.That(socialStrata, Does.Contain("Current implementation readback: v389-v396"));
        Assert.That(designAudit, Does.Contain("v389-v396 commoner social position readback audit"));
        Assert.That(moduleBoundaries, Does.Contain("Commoner social position readback v389-v396 boundary note"));
        Assert.That(integrationRules, Does.Contain("Commoner social position readback v389-v396 integration note"));
        Assert.That(simulation, Does.Contain("Current commoner social position readback v389-v396 note"));
        Assert.That(uiPresentation, Does.Contain("v389-v396 commoner social position readback"));
        Assert.That(acceptance, Does.Contain("Commoner social position readback v389-v396 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V389-V396 Commoner Social Position Readback"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V396"));
        Assert.That(schemaRules, Does.Contain("commoner social position readback v389-v396 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current commoner social position readback v389-v396 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No full class engine"));
        Assert.That(execPlan, Does.Contain("No direct promote/demote command"));

        Assert.That(readModelContracts, Does.Contain("SocialPositionReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("FamilyCore亲族位置"));
        Assert.That(personDossierProjection, Does.Contain("PopulationAndHouseholds生计活动"));
        Assert.That(personDossierProjection, Does.Contain("EducationAndExams读书考试"));
        Assert.That(personDossierProjection, Does.Contain("TradeAndIndustry商贸附着"));
        Assert.That(personDossierProjection, Does.Contain("OfficeAndCareer文书官身"));
        Assert.That(personDossierProjection, Does.Contain("SocialMemoryAndRelations旧忆压力"));
        Assert.That(personDossierProjection, Does.Contain("PersonRegistry只保身份/FidelityRing"));
        Assert.That(personDossierProjection, Does.Contain("不是升降阶级或zhuhu/kehu转换"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionLabel.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(unitySource, Does.Contain("SocialPositionReadbackSummary = dossier.SocialPositionReadbackSummary"));
        Assert.That(unitySource, Does.Contain("SocialPositionReadbackSummary { get; set; }"));
        Assert.That(unitySource, Does.Not.Contain("BuildSocialPositionReadbackSummary"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "PromotePerson",
                     "DemotePerson",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "CommonerClassResolver",
                     "SocialClassResolver",
                     "DirectPromoteCommand",
                     "DirectDemoteCommand",
                     "PerPersonCareerSimulationManager",
                     "v389-v396",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Social_position_owner_lane_keys_v397_v404_must_be_structured_projection_not_prose_parser()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_social-position-owner-lane-keys-v397-v404.md"));
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PersonDossierReadModels.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(RepoRoot, "unity", "Zongzu.UnityShell", "Assets", "Scripts", "ReadModels", "ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V397-V404 Social Position Owner-Lane Keys"));
        Assert.That(socialStrata, Does.Contain("Current implementation source keys: v397-v404"));
        Assert.That(designAudit, Does.Contain("v397-v404 social position owner-lane keys audit"));
        Assert.That(moduleBoundaries, Does.Contain("Social position owner-lane keys v397-v404 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social position owner-lane keys v397-v404 integration note"));
        Assert.That(simulation, Does.Contain("Current social position owner-lane keys v397-v404 note"));
        Assert.That(uiPresentation, Does.Contain("v397-v404 social position owner-lane keys"));
        Assert.That(acceptance, Does.Contain("Social position owner-lane keys v397-v404 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V397-V404 Social Position Owner-Lane Keys"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V404"));
        Assert.That(schemaRules, Does.Contain("social position owner-lane keys v397-v404 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current social position owner-lane keys v397-v404 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));

        Assert.That(readModelContracts, Does.Contain("SocialPositionSourceModuleKeys"));
        Assert.That(personDossierProjection, Does.Contain("IReadOnlyList<string> socialPositionSourceKeys = BuildSocialPositionSourceModuleKeys"));
        Assert.That(personDossierProjection, Does.Contain("SocialPositionSourceModuleKeys = socialPositionSourceKeys"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionSourceModuleKeys"));
        Assert.That(personDossierProjection, Does.Contain("List<string> keys = [KnownModuleKeys.PersonRegistry]"));
        Assert.That(personDossierProjection, Does.Contain("KnownModuleKeys.PopulationAndHouseholds"));
        Assert.That(personDossierProjection, Does.Contain("KnownModuleKeys.EducationAndExams"));
        Assert.That(personDossierProjection, Does.Contain("KnownModuleKeys.TradeAndIndustry"));
        Assert.That(personDossierProjection, Does.Contain("KnownModuleKeys.OfficeAndCareer"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionLabel.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionReadbackSummary.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(unitySource, Does.Contain("SocialPositionSourceModuleKeys = dossier.SocialPositionSourceModuleKeys.ToArray()"));
        Assert.That(unitySource, Does.Contain("SocialPositionSourceModuleKeys { get; set; }"));
        Assert.That(unitySource, Does.Not.Contain("BuildSocialPositionSourceModuleKeys"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionSourceModuleKeys"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "SocialPositionSourceLedger",
                     "CommonerClassResolver",
                     "SocialClassResolver",
                     "PerPersonCareerSimulationManager",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Social_position_readback_closeout_v405_v412_must_close_readback_layer_without_class_engine()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_social-position-readback-closeout-v405-v412.md"));
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PersonDossierReadModels.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V405-V412 Social Position Readback Closeout Audit"));
        Assert.That(socialStrata, Does.Contain("Current closeout: v405-v412"));
        Assert.That(designAudit, Does.Contain("v405-v412 social position readback closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Social position readback closeout v405-v412 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social position readback closeout v405-v412 integration note"));
        Assert.That(simulation, Does.Contain("Current social position readback closeout v405-v412 note"));
        Assert.That(uiPresentation, Does.Contain("v405-v412 social position readback closeout"));
        Assert.That(acceptance, Does.Contain("Social position readback closeout v405-v412 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V405-V412 Social Position Readback Closeout"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V412"));
        Assert.That(schemaRules, Does.Contain("social position readback closeout v405-v412 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current social position readback closeout v405-v412 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));

        Assert.That(readModelContracts, Does.Contain("SocialPositionReadbackSummary"));
        Assert.That(readModelContracts, Does.Contain("SocialPositionSourceModuleKeys"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionSourceModuleKeys"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionLabel.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionReadbackSummary.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionReadbackSummary"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionSourceModuleKeys"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "SocialPositionCloseoutLedger",
                     "CommonerClassResolver",
                     "SocialClassResolver",
                     "PerPersonCareerSimulationManager",
                     "CommonerCareerEngine",
                     "SocialClassEngine",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Social_position_scale_budget_v413_v420_must_read_existing_fidelity_without_precision_or_class_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_social-position-scale-budget-v413-v420.md"));
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "PersonDossierReadModels.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(RepoRoot, "unity", "Zongzu.UnityShell", "Assets", "Scripts", "ReadModels", "ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V413-V420 Social Position Scale Budget"));
        Assert.That(socialStrata, Does.Contain("Current scale-budget readback: v413-v420"));
        Assert.That(designAudit, Does.Contain("v413-v420 social position scale budget audit"));
        Assert.That(moduleBoundaries, Does.Contain("Social position scale budget v413-v420 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social position scale budget v413-v420 integration note"));
        Assert.That(simulation, Does.Contain("Current social position scale budget v413-v420 note"));
        Assert.That(uiPresentation, Does.Contain("v413-v420 social position scale budget"));
        Assert.That(acceptance, Does.Contain("Social position scale budget v413-v420 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V413-V420 Social Position Scale Budget"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V420"));
        Assert.That(schemaRules, Does.Contain("social position scale budget v413-v420 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current social position scale budget v413-v420 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No fidelity-ring mutation"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));

        Assert.That(readModelContracts, Does.Contain("SocialPositionScaleBudgetReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("SocialPositionScaleBudgetReadbackSummary = BuildSocialPositionScaleBudgetReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionScaleBudgetReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("FidelityRing.Core => \"close detail\""));
        Assert.That(personDossierProjection, Does.Contain("distant society remains pooled summary"));
        Assert.That(personDossierProjection, Does.Contain("no all-world per-person class simulation"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionReadbackSummary.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionSourceModuleKeys.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(unitySource, Does.Contain("SocialPositionScaleBudgetReadbackSummary = dossier.SocialPositionScaleBudgetReadbackSummary"));
        Assert.That(unitySource, Does.Contain("SocialPositionScaleBudgetReadbackSummary { get; set; }"));
        Assert.That(unitySource, Does.Not.Contain("BuildSocialPositionScaleBudgetReadbackSummary"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionScaleBudgetReadbackSummary"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "SocialPositionScaleBudgetLedger",
                     "CommonerClassResolver",
                     "SocialClassResolver",
                     "PerPersonCareerSimulationManager",
                     "CommonerCareerEngine",
                     "SocialClassEngine",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Social_position_regional_scale_guard_v421_v428_must_keep_far_summary_without_new_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_social-position-regional-scale-guard-v421-v428.md"));
        string integrationTestSource = File.ReadAllText(Path.Combine(
            TestsDir,
            "Zongzu.Integration.Tests",
            "PersonRegistryIntegrationTests.cs"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V421-V428 Social Position Regional Scale Guard"));
        Assert.That(socialStrata, Does.Contain("Current regional guard: v421-v428"));
        Assert.That(designAudit, Does.Contain("v421-v428 social position regional scale guard"));
        Assert.That(moduleBoundaries, Does.Contain("Social position regional scale guard v421-v428 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social position regional scale guard v421-v428 integration note"));
        Assert.That(simulation, Does.Contain("Current social position regional scale guard v421-v428 note"));
        Assert.That(uiPresentation, Does.Contain("v421-v428 social position regional scale guard"));
        Assert.That(acceptance, Does.Contain("Social position regional scale guard v421-v428 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V421-V428 Regional Scale Guard"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V428"));
        Assert.That(schemaRules, Does.Contain("social position regional scale guard v421-v428 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current social position regional scale guard v421-v428 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No fidelity-ring mutation"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        Assert.That(integrationTestSource, Does.Contain("RegistryOnlyBootstrap_BuildsRegionalScaleBudget_ForDistantPerson"));
        Assert.That(integrationTestSource, Does.Contain("FidelityRing = FidelityRing.Regional"));
        Assert.That(integrationTestSource, Does.Contain("regional summary"));
        Assert.That(integrationTestSource, Does.Contain("registry-only source"));
        Assert.That(integrationTestSource, Does.Contain("distant society remains pooled summary"));
        Assert.That(personDossierProjection, Does.Contain("FidelityRing.Regional => \"regional summary\""));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionReadbackSummary.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionSourceModuleKeys.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionScaleBudgetReadbackSummary"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "SocialPositionRegionalScaleLedger",
                     "RegionalScaleBudgetLedger",
                     "RegionalPersonSelectionManager",
                     "CommonerCareerEngine",
                     "SocialClassEngine",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Social_position_scale_closeout_v429_v436_must_not_become_class_or_person_browser_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_social-position-scale-closeout-v429-v436.md"));
        string personDossierProjection = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.PersonDossiers.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(RepoRoot, "unity", "Zongzu.UnityShell", "Assets", "Scripts", "ReadModels", "ViewModels", "Family", "PersonDossierViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Family", "LineageShellAdapter.cs"),
        }.Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V429-V436 Social Position Scale Closeout"));
        Assert.That(socialStrata, Does.Contain("Current closeout: v429-v436"));
        Assert.That(designAudit, Does.Contain("v429-v436 social position scale closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Social position scale closeout v429-v436 boundary note"));
        Assert.That(integrationRules, Does.Contain("Social position scale closeout v429-v436 integration note"));
        Assert.That(simulation, Does.Contain("Current social position scale closeout v429-v436 note"));
        Assert.That(uiPresentation, Does.Contain("v429-v436 social position scale closeout"));
        Assert.That(acceptance, Does.Contain("Social position scale closeout v429-v436 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V429-V436 Social Position Scale Closeout"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V436"));
        Assert.That(schemaRules, Does.Contain("social position scale closeout v429-v436 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current social position scale closeout v429-v436 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No global person browser"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionReadbackSummary"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionSourceModuleKeys"));
        Assert.That(personDossierProjection, Does.Contain("BuildSocialPositionScaleBudgetReadbackSummary"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionReadbackSummary.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionSourceModuleKeys.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("SocialPositionScaleBudgetReadbackSummary.Split"));
        Assert.That(personDossierProjection, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(unitySource, Does.Not.Contain("BuildSocialPositionReadbackSummary"));
        Assert.That(unitySource, Does.Not.Contain("BuildSocialPositionSourceModuleKeys"));
        Assert.That(unitySource, Does.Not.Contain("BuildSocialPositionScaleBudgetReadbackSummary"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionReadbackSummary"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionSourceModuleKeys"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialPositionScaleBudgetReadbackSummary"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "SocialPositionCloseoutLedger",
                     "RegionalPersonSelectionManager",
                     "GlobalPersonBrowser",
                     "CommonerCareerEngine",
                     "SocialClassEngine",
                     "ClassLadderController",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PersonnelFlow*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Commoner_status_owner_lane_preflight_v437_v444_must_point_to_population_without_new_status_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_commoner-status-owner-lane-preflight-v437-v444.md"));
        string populationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds")).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V437-V444 Commoner Status Owner-Lane Preflight"));
        Assert.That(socialStrata, Does.Contain("Current owner-lane preflight: v437-v444"));
        Assert.That(designAudit, Does.Contain("v437-v444 commoner status owner-lane preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Commoner status owner-lane preflight v437-v444 boundary note"));
        Assert.That(integrationRules, Does.Contain("Commoner status owner-lane preflight v437-v444 integration note"));
        Assert.That(simulation, Does.Contain("Current commoner status owner-lane preflight v437-v444 note"));
        Assert.That(uiPresentation, Does.Contain("v437-v444 commoner status owner-lane preflight"));
        Assert.That(acceptance, Does.Contain("Commoner status owner-lane preflight v437-v444 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V437-V444 Commoner Status Owner-Lane Preflight"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V444"));
        Assert.That(schemaRules, Does.Contain("commoner status owner-lane preflight v437-v444 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current commoner status owner-lane preflight v437-v444 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("PopulationAndHouseholds"));
        Assert.That(execPlan, Does.Contain("No new production rule"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        Assert.That(populationSource, Does.Contain("Livelihood"));
        Assert.That(populationSource, Does.Contain("Activity"));
        Assert.That(populationSource, Does.Contain("MigrationRisk"));
        Assert.That(populationSource, Does.Contain("LaborCapacity"));
        Assert.That(populationSource, Does.Contain("DebtPressure"));
        Assert.That(populationSource, Does.Contain("LandHolding"));
        Assert.That(populationSource, Does.Contain("GrainStore"));
        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("ZhuhuKehu"));
        Assert.That(personRegistrySource, Does.Not.Contain("OfficeService"));
        Assert.That(personRegistrySource, Does.Not.Contain("TradeAttachment"));
        Assert.That(personRegistrySource, Does.Not.Contain("DurableSocialPositionResidue"));

        foreach (string forbidden in new[]
                 {
                     "PromoteCommoner",
                     "DemoteCommoner",
                     "ConvertZhuhuKehu",
                     "ZhuhuKehuConversion",
                     "CommonerStatusLedger",
                     "CommonerMobilityLedger",
                     "SocialPositionLedger",
                     "ClassPositionLedger",
                     "SocialClassLedger",
                     "OwnerLanePreflightLedger",
                     "CommonerStatusResolver",
                     "CommonerCareerEngine",
                     "SocialClassEngine",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Fidelity_scale_budget_preflight_v445_v452_must_keep_near_detail_far_summary_without_global_person_scan()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_fidelity-scale-budget-preflight-v445-v452.md"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V445-V452 Fidelity Scale Budget Preflight"));
        Assert.That(socialStrata, Does.Contain("Current scale-budget preflight: v445-v452"));
        Assert.That(designAudit, Does.Contain("v445-v452 fidelity scale budget preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Fidelity scale budget preflight v445-v452 boundary note"));
        Assert.That(integrationRules, Does.Contain("Fidelity scale budget preflight v445-v452 integration note"));
        Assert.That(simulation, Does.Contain("Current fidelity scale budget preflight v445-v452 note"));
        Assert.That(uiPresentation, Does.Contain("v445-v452 fidelity scale budget preflight"));
        Assert.That(acceptance, Does.Contain("Fidelity scale budget preflight v445-v452 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V445-V452 Fidelity Scale Budget Preflight"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V452"));
        Assert.That(schemaRules, Does.Contain("fidelity scale budget preflight v445-v452 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current fidelity scale budget preflight v445-v452 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("near detail, far summary"));
        Assert.That(execPlan, Does.Contain("No fidelity-ring mutation"));
        Assert.That(execPlan, Does.Contain("No global person scan"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("FidelityScaleBudget"));
        Assert.That(personRegistrySource, Does.Not.Contain("TargetCardinality"));
        Assert.That(personRegistrySource, Does.Not.Contain("RegionalPersonSelector"));
        Assert.That(personRegistrySource, Does.Not.Contain("GlobalPersonScanner"));

        foreach (string forbidden in new[]
                 {
                     "GlobalPersonScanner",
                     "GlobalPersonBrowser",
                     "RegionalPersonSelector",
                     "RegionalPersonSelectionManager",
                     "AllWorldPersonSimulation",
                     "AllWorldPersonCareerSimulation",
                     "FidelityScaleBudgetLedger",
                     "FidelityBudgetLedger",
                     "ScaleBudgetLedger",
                     "TargetCardinalityLedger",
                     "PrecisionBandLedger",
                     "PopulationScaleBudgetLedger",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "CommonerCareerEngine",
                     "WorldPopulationManager",
                     "PersonManager",
                     "CharacterManager",
                     "WorldManager",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialPosition*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PopulationScaleBudget*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.FidelityScaleBudget*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Strata*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_dynamics_explanation_v453_v460_must_project_existing_dimensions_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-28_household-mobility-dynamics-explanation-v453-v460.md"));
        string readModelContracts = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Contracts",
            "ReadModels",
            "LivingSocietyReadModels.cs"));
        string builderSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.LivingSociety.cs"));
        string deskAdapter = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "DeskSandbox",
            "DeskSandboxShellAdapter.cs"));
        string settlementViewModel = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity.ViewModels",
            "DeskSandbox",
            "SettlementNodeViewModel.cs"));
        string populationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds")).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V453-V460 Household Mobility Dynamics Explanation"));
        Assert.That(socialStrata, Does.Contain("Current household dynamics explanation: v453-v460"));
        Assert.That(designAudit, Does.Contain("v453-v460 household mobility dynamics explanation audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility dynamics explanation v453-v460 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility dynamics explanation v453-v460 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility dynamics explanation v453-v460 note"));
        Assert.That(uiPresentation, Does.Contain("v453-v460 household mobility dynamics explanation"));
        Assert.That(acceptance, Does.Contain("Household mobility dynamics explanation v453-v460 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V453-V460 Household Mobility Dynamics Explanation"));
        Assert.That(schemaRules, Does.Contain("household mobility dynamics explanation v453-v460 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility dynamics explanation v453-v460 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("not a new social-class rule"));
        Assert.That(execPlan, Does.Contain("not a new mobility algorithm"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        Assert.That(readModelContracts, Does.Contain("MobilityDynamicsExplanationSummary"));
        Assert.That(readModelContracts, Does.Contain("MobilityDynamicsDimensionKeys"));
        Assert.That(builderSource, Does.Contain("BuildHouseholdMobilityDynamicsDimensionKeys"));
        Assert.That(builderSource, Does.Contain("BuildHouseholdMobilityDynamicsExplanationSummary"));
        Assert.That(builderSource, Does.Contain("HouseholdSocialPressureSignalKeys"));
        Assert.That(builderSource, Does.Contain("OrderByDescending"));
        Assert.That(builderSource, Does.Contain("StringComparer.Ordinal"));
        Assert.That(builderSource, Does.Contain(".Take(4)"));
        Assert.That(builderSource, Does.Contain("PopulationAndHouseholds owns household dynamics"));
        Assert.That(builderSource, Does.Contain("far summary stays pooled"));
        Assert.That(builderSource, Does.Contain("no PersonRegistry status authority"));
        Assert.That(deskAdapter, Does.Contain("HouseholdMobilityDynamicsSummary = BuildSettlementHouseholdMobilityDynamicsSummary"));
        Assert.That(settlementViewModel, Does.Contain("HouseholdMobilityDynamicsSummary"));

        foreach (string existingCarrier in new[]
                 {
                     "ComputeSubsistenceFragilityPressure",
                     "ComputeTaxSeasonBurdenProfile",
                     "ComputeOfficialSupplyBurdenProfile",
                     "TryApplyMonthlyLivelihoodDrift",
                     "MigrationRisk",
                     "DebtPressure",
                     "LaborCapacity",
                     "GrainStore",
                     "LandHolding",
                 })
        {
            Assert.That(populationSource, Does.Contain(existingCarrier), existingCarrier);
        }

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("MobilityDynamicsExplanationSummary"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityDynamics"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityDynamicsLedger",
                     "MobilityDynamicsLedger",
                     "CommonerStatusLedger",
                     "SocialClassLedger",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "WorldPopulationManager",
                     "PersonManager",
                     "CharacterManager",
                     "WorldManager",
                     "ParseDomainEventSummary",
                     ".Summary.Split",
                     "DomainEvent.Summary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobilityDynamics*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilityDynamics*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.PopulationScaleBudget*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.FidelityScaleBudget*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_dynamics_closeout_v461_v468_must_not_become_movement_or_status_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-29_household-mobility-closeout-v461-v468.md"));
        string builderSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.LivingSociety.cs"));
        string deskAdapter = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Presentation.Unity",
            "Adapters",
            "DeskSandbox",
            "DeskSandboxShellAdapter.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V461-V468 Household Mobility Dynamics Closeout"));
        Assert.That(socialStrata, Does.Contain("Current household dynamics closeout: v461-v468"));
        Assert.That(designAudit, Does.Contain("v461-v468 household mobility dynamics closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility dynamics closeout v461-v468 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility dynamics closeout v461-v468 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility dynamics closeout v461-v468 note"));
        Assert.That(uiPresentation, Does.Contain("v461-v468 household mobility dynamics closeout"));
        Assert.That(acceptance, Does.Contain("Household mobility dynamics closeout v461-v468 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V461-V468 Household Mobility Dynamics Closeout"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Closeout Through V468"));
        Assert.That(schemaRules, Does.Contain("household mobility dynamics closeout v461-v468 remains docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current household mobility dynamics closeout v461-v468 note"));
        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No direct move"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        Assert.That(builderSource, Does.Contain("BuildHouseholdMobilityDynamicsDimensionKeys"));
        Assert.That(builderSource, Does.Contain("BuildHouseholdMobilityDynamicsExplanationSummary"));
        Assert.That(builderSource, Does.Not.Contain("MobilityDynamicsExplanationSummary.Split"));
        Assert.That(builderSource, Does.Not.Contain("HouseholdMobilityDynamicsSummary.Split"));
        Assert.That(builderSource, Does.Not.Contain("DomainEvent.Summary"));
        Assert.That(deskAdapter, Does.Contain("HouseholdMobilityDynamicsSummary = BuildSettlementHouseholdMobilityDynamicsSummary"));
        Assert.That(deskAdapter, Does.Not.Contain("MobilityDynamicsDimensionKeys"));
        Assert.That(personRegistrySource, Does.Not.Contain("MobilityDynamicsExplanationSummary"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityDynamics"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityDynamicsCloseoutLedger",
                     "HouseholdMobilityDynamicsLedger",
                     "HouseholdMobilityLedger",
                     "HouseholdMovementLedger",
                     "MovementRouteHistory",
                     "HouseholdRouteHistoryLedger",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "DirectHouseholdMovementResolver",
                     "MobilitySelectorWatermark",
                     "HouseholdMovementEngine",
                     "HouseholdMobilityEngine",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "ParseMobilityDynamicsExplanation",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "DomainEvent.Summary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_owner_lane_preflight_v469_v476_must_gate_future_rule_depth_without_runtime_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-29_household-mobility-owner-lane-preflight-v469-v476.md"));
        string populationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds")).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V469-V476 Household Mobility Owner-Lane Preflight"));
        Assert.That(socialStrata, Does.Contain("Current household mobility owner-lane preflight: v469-v476"));
        Assert.That(designAudit, Does.Contain("v469-v476 household mobility owner-lane preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility owner-lane preflight v469-v476 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility owner-lane preflight v469-v476 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility owner-lane preflight v469-v476 note"));
        Assert.That(uiPresentation, Does.Contain("v469-v476 household mobility owner-lane preflight"));
        Assert.That(acceptance, Does.Contain("Household mobility owner-lane preflight v469-v476 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V469-V476 Household Mobility Owner-Lane Preflight"));
        Assert.That(skillMatrix, Does.Contain("V469-V476 contains household mobility owner-lane preflight"));
        Assert.That(schemaRules, Does.Contain("household mobility owner-lane preflight v469-v476 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility owner-lane preflight v469-v476 note"));

        foreach (string requiredGate in new[]
                 {
                     "PopulationAndHouseholds",
                     "owner state",
                     "cadence",
                     "target scope",
                     "hot path",
                     "touched counts",
                     "deterministic cap/order",
                     "no-touch boundary",
                     "schema impact",
                     "projection fields",
                     "validation",
                     "near detail, far summary",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredGate), requiredGate);
        }

        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No migration economy"));
        Assert.That(execPlan, Does.Contain("No route-history model"));
        Assert.That(execPlan, Does.Contain("No direct movement command"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        foreach (string existingCarrier in new[]
                 {
                     "MigrationRisk",
                     "DebtPressure",
                     "LaborCapacity",
                     "GrainStore",
                     "LandHolding",
                     "LaborPools",
                     "MigrationPools",
                 })
        {
            Assert.That(populationSource, Does.Contain(existingCarrier), existingCarrier);
        }

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityOwnerLane"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRoute"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityOwnerLaneLedger",
                     "HouseholdMobilityPreflightLedger",
                     "HouseholdMobilityRuleLedger",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MobilitySelectorWatermark",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "HouseholdMobilityRuleEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "ParseHouseholdMobilityOwnerLane",
                     "ParseMobilityDynamicsExplanation",
                     "DomainEvent.Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_preflight_closeout_v485_v492_must_close_gate_without_implementing_movement_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-29_household-mobility-preflight-closeout-v485-v492.md"));
        string populationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds")).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V485-V492 Household Mobility Preflight Closeout Audit"));
        Assert.That(socialStrata, Does.Contain("Current household mobility preflight closeout: v485-v492"));
        Assert.That(designAudit, Does.Contain("v485-v492 household mobility preflight closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility preflight closeout v485-v492 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility preflight closeout v485-v492 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility preflight closeout v485-v492 note"));
        Assert.That(uiPresentation, Does.Contain("v485-v492 household mobility preflight closeout"));
        Assert.That(acceptance, Does.Contain("Household mobility preflight closeout v485-v492 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V485-V492 Household Mobility Preflight Closeout"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Preflight Closeout Through V492"));
        Assert.That(schemaRules, Does.Contain("household mobility preflight closeout v485-v492 remains docs/tests only"));
        Assert.That(dataSchema, Does.Contain("Current household mobility preflight closeout v485-v492 note"));

        foreach (string requiredGate in new[]
                 {
                     "owner state",
                     "cadence",
                     "target scope",
                     "hot path",
                     "touched counts",
                     "deterministic cap/order",
                     "no-touch boundary",
                     "schema impact",
                     "projection fields",
                     "validation",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredGate), requiredGate);
        }

        Assert.That(execPlan, Does.Contain("Target schema/migration impact: none"));
        Assert.That(execPlan, Does.Contain("No production rule change"));
        Assert.That(execPlan, Does.Contain("No route-history model"));
        Assert.That(execPlan, Does.Contain("No direct movement command"));
        Assert.That(execPlan, Does.Contain("No Application, UI, or Unity authority"));
        Assert.That(execPlan, Does.Contain("No `PersonRegistry` expansion"));
        Assert.That(execPlan, Does.Contain("No parsing of `DomainEvent.Summary`"));

        foreach (string existingCarrier in new[]
                 {
                     "MigrationRisk",
                     "DebtPressure",
                     "LaborCapacity",
                     "GrainStore",
                     "LandHolding",
                     "LaborPools",
                     "MigrationPools",
                 })
        {
            Assert.That(populationSource, Does.Contain(existingCarrier), existingCarrier);
        }

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityCloseout"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRoute"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityPreflightCloseoutLedger",
                     "HouseholdMobilityCloseoutLedger",
                     "HouseholdMobilityOwnerLaneLedger",
                     "HouseholdMobilityRuleLedger",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MobilitySelectorWatermark",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "HouseholdMobilityRuleEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "ParseHouseholdMobilityCloseout",
                     "ParseMobilityDynamicsExplanation",
                     "DomainEvent.Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_rules_data_readiness_v501_v508_must_remain_readiness_only_without_movement_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_household-mobility-runtime-rules-data-readiness-v501-v508.md"));
        string populationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PopulationAndHouseholds")).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V501-V508 Household Mobility First Runtime Rule And Rules-Data Readiness"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime/rules-data readiness: v501-v508"));
        Assert.That(designAudit, Does.Contain("v501-v508 household mobility runtime rules-data readiness audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime rules-data readiness v501-v508 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime rules-data readiness v501-v508 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime rules-data readiness v501-v508 note"));
        Assert.That(uiPresentation, Does.Contain("v501-v508 household mobility runtime rules-data readiness"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime rules-data readiness v501-v508 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V501-V508 Household Mobility First Runtime Rule And Rules-Data Readiness"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Rules-Data Readiness Through V508"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime rules-data readiness v501-v508 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime rules-data readiness v501-v508 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "first runtime rule readiness map",
                     "hardcoded extraction map",
                     "monthly authority first",
                     "player-near households",
                     "pressure-hit local households",
                     "active-region",
                     "distant summaries",
                     "quiet households",
                     "off-scope settlements",
                     "distant pooled society",
                     "Application/UI/Unity",
                     "Threshold bands",
                     "Weights",
                     "Caps and limits",
                     "Recovery / decay",
                     "Deterministic ordering",
                     "Regional assumptions",
                     "Era/scenario assumptions",
                     "Pool limits",
                     "No `PersonRegistry` expansion",
                     "No parsing of `DomainEvent.Summary`",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        foreach (string existingCarrier in new[]
                 {
                     "Livelihood",
                     "Distress",
                     "DebtPressure",
                     "LaborCapacity",
                     "MigrationRisk",
                     "IsMigrating",
                     "LandHolding",
                     "GrainStore",
                     "LaborPools",
                     "MarriagePools",
                     "MigrationPools",
                     "ComputeDebtDelta",
                     "ComputeLaborDelta",
                     "ComputeMigrationDelta",
                     "ResolveMigrationStatus",
                     "ResolveMonthlyLivelihood",
                     "ResolveFocusPromotionReason",
                     "RebuildMigrationPools",
                     "RebuildLaborPools",
                     "RebuildMarriagePools",
                     "ComputeTaxSeasonBurdenProfile",
                     "ComputeSubsistencePressureProfile",
                     "ComputeOfficialSupplyBurdenProfile",
                     "DefaultFocusedMemberPromotionCap",
                     "GetFocusedMemberPromotionCapOrDefault",
                 })
        {
            Assert.That(populationSource, Does.Contain(existingCarrier), existingCarrier);
        }

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRuleReadiness"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRoute"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityReadinessLedger",
                     "HouseholdMobilityRulesDataLedger",
                     "HouseholdMobilityRuleLedger",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "HouseholdMobilityRuleEngine",
                     "HouseholdMobilityRulesDataLoader",
                     "RuntimePluginMarketplace",
                     "ReflectionRuleLoader",
                     "ArbitraryScriptRule",
                     "ApplicationHouseholdMobilityResolver",
                     "UnityHouseholdMovement",
                     "UiHouseholdMovement",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "ParseHouseholdMobilityReadiness",
                     "ParseMobilityDynamicsExplanation",
                     "DomainEvent.Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_rules_data_contract_v509_v516_must_stay_contract_preflight_not_runtime_plugin_system()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string engineeringRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ENGINEERING_RULES.md"));
        string livingWorldRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RULES_DRIVEN_LIVING_WORLD.md"));
        string extensibilityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "EXTENSIBILITY_MODEL.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_household-mobility-rules-data-contract-v509-v516.md"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V509-V516 Household Mobility Rules-Data Contract And Validator Preflight"));
        Assert.That(socialStrata, Does.Contain("Current household mobility rules-data contract preflight: v509-v516"));
        Assert.That(designAudit, Does.Contain("v509-v516 household mobility rules-data contract preflight audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility rules-data contract preflight v509-v516 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility rules-data contract preflight v509-v516 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility rules-data contract preflight v509-v516 note"));
        Assert.That(uiPresentation, Does.Contain("v509-v516 household mobility rules-data contract preflight"));
        Assert.That(acceptance, Does.Contain("Household mobility rules-data contract preflight v509-v516 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V509-V516 Household Mobility Rules-Data Contract And Validator Preflight"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Rules-Data Contract Preflight Through V516"));
        Assert.That(schemaRules, Does.Contain("household mobility rules-data contract preflight v509-v516 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility rules-data contract preflight v509-v516 note"));

        Assert.That(engineeringRules, Does.Contain("do not bury game rules in UI or config loaders"));
        Assert.That(engineeringRules, Does.Contain("do not bypass boundaries through reflection"));
        Assert.That(livingWorldRules, Does.Contain("data may configure a rule"));
        Assert.That(extensibilityModel, Does.Contain("must not become runtime plugin marketplaces"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "docs/tests-only contract preflight",
                     "No runtime behavior change",
                     "No rules-data loader",
                     "no reusable runtime rules-data/content/config pattern exists",
                     "stable ids",
                     "schema/version",
                     "deterministic ordering",
                     "default fallback",
                     "validation errors",
                     "owner-consumed only",
                     "no UI/Application authority",
                     "no arbitrary script/plugin execution",
                     "threshold bands",
                     "pressure weights",
                     "regional modifiers",
                     "era/scenario modifiers",
                     "recovery/decay rates",
                     "fanout caps",
                     "deterministic tie-break priorities",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "authoring")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "generated")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "rules-data")), Is.False);
        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "config")), Is.False);

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRulesData"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityContract"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRoute"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityRulesDataContract",
                     "HouseholdMobilityRulesDataValidator",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityDefaultRulesData",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityRulesDataLedger",
                     "HouseholdMobilityContractLedger",
                     "IRulesDataProvider",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "ReflectionRuleLoader",
                     "DynamicRuleAssembly",
                     "HouseholdMobilityScriptRule",
                     "ApplicationHouseholdMobilityRulesDataResolver",
                     "UiHouseholdMobilityRulesDataResolver",
                     "UnityHouseholdMobilityRulesDataResolver",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "CSScript",
                     "RoslynScript",
                     "JintEngine",
                     "MoonSharp",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "ParseHouseholdMobilityContract",
                     "ParseHouseholdMobilityReadiness",
                     "ParseMobilityDynamicsExplanation",
                     "DomainEvent.Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_default_rules_data_skeleton_v517_v524_must_remain_docs_tests_only_without_loader_when_no_pattern_exists()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_household-mobility-default-rules-data-skeleton-v517-v524.md"));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V517-V524 Household Mobility Default Rules-Data Skeleton"));
        Assert.That(socialStrata, Does.Contain("Current household mobility default rules-data skeleton gate: v517-v524"));
        Assert.That(designAudit, Does.Contain("v517-v524 household mobility default rules-data skeleton audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility default rules-data skeleton v517-v524 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility default rules-data skeleton v517-v524 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility default rules-data skeleton v517-v524 note"));
        Assert.That(uiPresentation, Does.Contain("v517-v524 household mobility default rules-data skeleton"));
        Assert.That(acceptance, Does.Contain("Household mobility default rules-data skeleton v517-v524 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V517-V524 Household Mobility Default Rules-Data Skeleton"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Default Rules-Data Skeleton Through V524"));
        Assert.That(schemaRules, Does.Contain("household mobility default rules-data skeleton v517-v524 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility default rules-data skeleton v517-v524 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "docs/tests-only skeleton contract",
                     "No runtime behavior change",
                     "No default rules-data file",
                     "No rules-data loader",
                     "No validator implementation",
                     "no reusable runtime rules-data/content/config pattern exists",
                     "ruleSetId",
                     "schemaVersion",
                     "ownerModule",
                     "defaultFallbackPolicy",
                     "parameterGroups",
                     "validationResult",
                     "deterministic declaration order",
                     "thresholdBands",
                     "pressureWeights",
                     "regionalModifiers",
                     "eraScenarioModifiers",
                     "recoveryDecayRates",
                     "fanoutCaps",
                     "tieBreakPriorities",
                     "The skeleton does not enter save",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "authoring")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "generated")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "rules-data")), Is.False);
        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "config")), Is.False);

        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityDefaultRulesData"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRulesData"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRoute"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityDefaultRulesData",
                     "DefaultHouseholdMobilityRulesData",
                     "HouseholdMobilityDefaultRulesDataValidator",
                     "HouseholdMobilityRulesDataContract",
                     "HouseholdMobilityRulesDataValidator",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityRulesDataLedger",
                     "HouseholdMobilityDefaultSkeletonLedger",
                     "IRulesDataProvider",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "ReflectionRuleLoader",
                     "DynamicRuleAssembly",
                     "ApplicationHouseholdMobilityRulesDataResolver",
                     "UiHouseholdMobilityRulesDataResolver",
                     "UnityHouseholdMobilityRulesDataResolver",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "ParseHouseholdMobilityDefaultSkeleton",
                     "ParseHouseholdMobilityContract",
                     "ParseMobilityDynamicsExplanation",
                     "DomainEvent.Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Population_households_first_hardcoded_rule_extraction_v525_v532_must_stay_owner_consumed_schema_neutral_and_ui_free()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_population-households-first-hardcoded-rule-extraction-v525-v532.md"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V525-V532 PopulationAndHouseholds First Hardcoded Rule Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility first hardcoded rule extraction: v525-v532"));
        Assert.That(designAudit, Does.Contain("v525-v532 population households first hardcoded rule extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("PopulationAndHouseholds first hardcoded rule extraction v525-v532 boundary note"));
        Assert.That(integrationRules, Does.Contain("PopulationAndHouseholds first hardcoded rule extraction v525-v532 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility first hardcoded rule extraction v525-v532 note"));
        Assert.That(uiPresentation, Does.Contain("v525-v532 population households first hardcoded rule extraction"));
        Assert.That(acceptance, Does.Contain("PopulationAndHouseholds first hardcoded rule extraction v525-v532 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V525-V532 PopulationAndHouseholds First Hardcoded Rule Extraction"));
        Assert.That(skillMatrix, Does.Contain("PopulationAndHouseholds First Hardcoded Rule Extraction Through V532"));
        Assert.That(schemaRules, Does.Contain("population households first hardcoded rule extraction v525-v532 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility first hardcoded rule extraction v525-v532 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "Extracted rule",
                     "focused member promotion fanout cap",
                     "DefaultFocusedMemberPromotionCap",
                     "default remains 2",
                     "deterministic household-id then person-id order",
                     "invalid cap falls back to default",
                     "No loader",
                     "No `content/rules-data`",
                     "Owner: `PopulationAndHouseholds`",
                     "Application/UI/Unity do not consume",
                     "No `PersonRegistry` expansion",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesDataSource, Does.Contain("PopulationHouseholdMobilityRulesData"));
        Assert.That(rulesDataSource, Does.Contain("DefaultFocusedMemberPromotionCap = 2"));
        Assert.That(rulesDataSource, Does.Contain("MaxFocusedMemberPromotionCap = 8"));
        Assert.That(rulesDataSource, Does.Contain("focused_member_promotion_cap"));
        Assert.That(rulesDataSource, Does.Contain("GetFocusedMemberPromotionCapOrDefault"));
        Assert.That(populationModule, Does.Contain("HouseholdMobilityRulesData"));
        Assert.That(populationModule, Does.Contain("PopulationHouseholdMobilityRulesData.Default"));
        Assert.That(populationModule, Does.Contain("focusedMemberPromotionCap"));
        Assert.That(populationModule, Does.Contain(".Take(focusedMemberPromotionCap)"));
        Assert.That(populationModule, Does.Not.Contain(".Take(2))"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));

        Assert.That(Directory.Exists(Path.Combine(RepoRoot, "content", "rules-data")), Is.False);
        Assert.That(personRegistrySource, Does.Contain("FidelityRing"));
        Assert.That(personRegistrySource, Does.Not.Contain("PopulationHouseholdMobilityRulesData"));
        Assert.That(personRegistrySource, Does.Not.Contain("FocusedMemberPromotionCap"));
        Assert.That(personRegistrySource, Does.Not.Contain("HouseholdMobilityRoute"));
        Assert.That(personRegistrySource, Does.Not.Contain("CommonerStatus"));
        Assert.That(personRegistrySource, Does.Not.Contain("SocialClass"));

        foreach (string authorityToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "FocusedMemberPromotionCap",
                     "DefaultFocusedMemberPromotionCap",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string forbidden in new[]
                 {
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityRulesDataLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "ReflectionRuleLoader",
                     "DynamicRuleAssembly",
                     "ApplicationHouseholdMobilityRulesDataResolver",
                     "UiHouseholdMobilityRulesDataResolver",
                     "UnityHouseholdMobilityRulesDataResolver",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "MigrationEconomyEngine",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GlobalPersonScanner",
                     "RegionalPersonSelector",
                     "WorldPopulationManager",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "ParseHouseholdMobilityRulesData",
                     "ParseMobilityDynamicsExplanation",
                     "DomainEvent.Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Population_households_first_mobility_runtime_rule_v533_v540_must_stay_population_owned_bounded_schema_neutral_and_ui_free()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_population-households-first-mobility-runtime-rule-v533-v540.md"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V533-V540 PopulationAndHouseholds First Household Mobility Runtime Rule"));
        Assert.That(socialStrata, Does.Contain("Current household mobility first runtime rule: v533-v540"));
        Assert.That(designAudit, Does.Contain("v533-v540 population households first mobility runtime rule audit"));
        Assert.That(moduleBoundaries, Does.Contain("PopulationAndHouseholds first mobility runtime rule v533-v540 boundary note"));
        Assert.That(integrationRules, Does.Contain("PopulationAndHouseholds first mobility runtime rule v533-v540 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility first runtime rule v533-v540 note"));
        Assert.That(uiPresentation, Does.Contain("v533-v540 population households first mobility runtime rule"));
        Assert.That(acceptance, Does.Contain("PopulationAndHouseholds first mobility runtime rule v533-v540 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V533-V540 PopulationAndHouseholds First Household Mobility Runtime Rule"));
        Assert.That(skillMatrix, Does.Contain("PopulationAndHouseholds First Mobility Runtime Rule Through V540"));
        Assert.That(schemaRules, Does.Contain("population households first mobility runtime rule v533-v540 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility first runtime rule v533-v540 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "first household mobility runtime rule",
                     "Owner: `PopulationAndHouseholds`",
                     "Cadence: monthly",
                     "deterministic target selection",
                     "bounded fanout",
                     "near detail far summary",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(populationModule, Does.Contain("ApplyMonthlyHouseholdMobilityRuntimeRule"));
        Assert.That(populationModule, Does.Contain("MigrationPools"));
        Assert.That(populationModule, Does.Contain("OrderByDescending(static pool => pool.OutflowPressure)"));
        Assert.That(populationModule, Does.Contain("ThenBy(static pool => pool.SettlementId.Value)"));
        Assert.That(populationModule, Does.Contain(".Take(settlementCap)"));
        Assert.That(populationModule, Does.Contain(".Take(householdCap)"));
        Assert.That(populationModule, Does.Contain("ComputeMonthlyHouseholdMobilityRuntimeScore"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeSettlementCapOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeHouseholdCapOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeRiskDeltaOrDefault"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));

        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeSettlementCap = 1"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeHouseholdCap = 2"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeRiskDelta = 1"));
        Assert.That(rulesDataSource, Does.Contain("monthly_runtime_household_cap"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));

        foreach (string authorityToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "MonthlyRuntimeHouseholdCap",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_first_runtime_rule_closeout_v541_v548_must_remain_closeout_only_without_second_mobility_authority()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_household-mobility-first-runtime-rule-closeout-v541-v548.md"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V541-V548 Household Mobility First Runtime Rule Closeout"));
        Assert.That(socialStrata, Does.Contain("Current household mobility first runtime rule closeout: v541-v548"));
        Assert.That(designAudit, Does.Contain("v541-v548 household mobility first runtime rule closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility first runtime rule closeout v541-v548 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility first runtime rule closeout v541-v548 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility first runtime rule closeout v541-v548 note"));
        Assert.That(uiPresentation, Does.Contain("v541-v548 household mobility first runtime rule closeout"));
        Assert.That(acceptance, Does.Contain("Household mobility first runtime rule closeout v541-v548 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V541-V548 Household Mobility First Runtime Rule Closeout"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility First Runtime Rule Closeout Through V548"));
        Assert.That(schemaRules, Does.Contain("household mobility first runtime rule closeout v541-v548 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility first runtime rule closeout v541-v548 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "docs/tests closeout",
                     "No runtime behavior change",
                     "No second household mobility runtime rule",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(populationModule, Does.Contain("ApplyMonthlyHouseholdMobilityRuntimeRule"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The closeout must not add a second monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeSettlementCapOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeHouseholdCapOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeRiskDeltaOrDefault"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeSettlementCap = 1"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeHouseholdCap = 2"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));

        foreach (string authorityToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "MonthlyRuntimeHouseholdCap",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_rule_health_evidence_v549_v556_must_remain_diagnostics_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-04-30_household-mobility-runtime-rule-health-evidence-v549-v556.md"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V549-V556 Household Mobility Runtime Rule Health Evidence"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime rule health evidence: v549-v556"));
        Assert.That(designAudit, Does.Contain("v549-v556 household mobility runtime rule health evidence audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime rule health evidence v549-v556 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime rule health evidence v549-v556 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime rule health evidence v549-v556 note"));
        Assert.That(uiPresentation, Does.Contain("v549-v556 household mobility runtime rule health evidence"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime rule health evidence v549-v556 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V549-V556 Household Mobility Runtime Rule Health Evidence"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Rule Health Evidence Through V556"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime rule health evidence v549-v556 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime rule health evidence v549-v556 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "No runtime behavior change",
                     "diagnostics/readiness evidence",
                     "No second household mobility runtime rule",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The health-evidence pass must not add a second monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeSettlementCap = 1"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeHouseholdCap = 2"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeRiskDelta = 1"));
        Assert.That(rulesDataSource, Does.Contain("monthly_runtime_household_cap"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));

        foreach (string authorityToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "MonthlyRuntimeHouseholdCap",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_widening_gate_v557_v564_must_remain_preflight_only_without_fanout_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-01_household-mobility-runtime-widening-gate-v557-v564.md"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V557-V564 Household Mobility Runtime Widening Gate"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime widening gate: v557-v564"));
        Assert.That(designAudit, Does.Contain("v557-v564 household mobility runtime widening gate audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime widening gate v557-v564 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime widening gate v557-v564 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime widening gate v557-v564 note"));
        Assert.That(uiPresentation, Does.Contain("v557-v564 household mobility runtime widening gate"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime widening gate v557-v564 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V557-V564 Household Mobility Runtime Widening Gate"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Widening Gate Through V564"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime widening gate v557-v564 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime widening gate v557-v564 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "docs/tests preflight",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No recovery/decay formula change",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The widening gate must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeSettlementCap = 1"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeHouseholdCap = 2"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeRiskDelta = 1"));
        Assert.That(rulesDataSource, Does.Contain("MaxMonthlyRuntimeSettlementCap = 8"));
        Assert.That(rulesDataSource, Does.Contain("MaxMonthlyRuntimeHouseholdCap = 16"));
        Assert.That(rulesDataSource, Does.Contain("MaxMonthlyRuntimeRiskDelta = 8"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));

        foreach (string authorityToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "MonthlyRuntimeHouseholdCap",
                     "MonthlyRuntimeSettlementCap",
                     "MonthlyRuntimeRiskDelta",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "TouchCount",
                     "MigrationPressureClass",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityFanoutExpander",
                     "HouseholdMobilityRecoveryTuner",
                     "HouseholdMobilityDecayTuner",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_touch_count_proof_v565_v572_must_stay_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-01_household-mobility-runtime-touch-count-proof-v565-v572.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V565-V572 Household Mobility Runtime Touch-Count Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime touch-count proof: v565-v572"));
        Assert.That(designAudit, Does.Contain("v565-v572 household mobility runtime touch-count proof audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime touch-count proof v565-v572 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime touch-count proof v565-v572 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime touch-count proof v565-v572 note"));
        Assert.That(uiPresentation, Does.Contain("v565-v572 household mobility runtime touch-count proof"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime touch-count proof v565-v572 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V565-V572 Household Mobility Runtime Touch-Count Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Touch-Count Proof Through V572"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime touch-count proof v565-v572 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime touch-count proof v565-v572 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "focused test evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No recovery/decay formula change",
                     "No persisted touch-count state",
                     "No diagnostic state",
                     "No performance cache",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultCapsTouchOnlyOnePoolAndTwoHouseholds"));
        Assert.That(householdModuleTests, Does.Contain("selectedPoolTouchedHouseholds"));
        Assert.That(householdModuleTests, Does.Contain("selectedPoolTouchedSettlements"));
        Assert.That(householdModuleTests, Does.Contain("PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 }"));

        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The touch-count proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeSettlementCap = 1"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeHouseholdCap = 2"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeRiskDelta = 1"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));

        foreach (string authorityToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "MonthlyRuntimeHouseholdCap",
                     "MonthlyRuntimeSettlementCap",
                     "MonthlyRuntimeRiskDelta",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "TouchCount",
                     "MigrationPressureClass",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "HouseholdMobilitySelector",
                     "HouseholdMovementEngine",
                     "DirectHouseholdMovementResolver",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityFanoutExpander",
                     "HouseholdMobilityRecoveryTuner",
                     "HouseholdMobilityDecayTuner",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_rules_data_fallback_matrix_v573_v580_must_remain_test_evidence_only_without_loader_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-01_household-mobility-rules-data-fallback-matrix-v573-v580.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesDataSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V573-V580 Household Mobility Rules-Data Fallback Matrix"));
        Assert.That(socialStrata, Does.Contain("Current household mobility rules-data fallback matrix: v573-v580"));
        Assert.That(designAudit, Does.Contain("v573-v580 household mobility rules-data fallback matrix audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility rules-data fallback matrix v573-v580 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility rules-data fallback matrix v573-v580 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility rules-data fallback matrix v573-v580 note"));
        Assert.That(uiPresentation, Does.Contain("v573-v580 household mobility rules-data fallback matrix"));
        Assert.That(acceptance, Does.Contain("Household mobility rules-data fallback matrix v573-v580 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V573-V580 Household Mobility Rules-Data Fallback Matrix"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Rules-Data Fallback Matrix Through V580"));
        Assert.That(schemaRules, Does.Contain("household mobility rules-data fallback matrix v573-v580 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility rules-data fallback matrix v573-v580 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "focused fallback evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No arbitrary script rules",
                     "No runtime assemblies",
                     "No reflection-heavy rule loading",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No recovery/decay formula change",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "Malformed runtime rules-data falls back deterministically to defaults",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults"));
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"));
        Assert.That(householdModuleTests, Does.Contain("MaxMonthlyRuntimeSettlementCap + 1"));
        Assert.That(householdModuleTests, Does.Contain("MaxMonthlyRuntimeHouseholdCap + 1"));
        Assert.That(householdModuleTests, Does.Contain("MaxMonthlyRuntimeRiskDelta + 1"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature(malformedResult)"));

        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The fallback matrix must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeSettlementCap = 1"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeHouseholdCap = 2"));
        Assert.That(rulesDataSource, Does.Contain("DefaultMonthlyRuntimeRiskDelta = 1"));
        Assert.That(rulesDataSource, Does.Contain("monthly_runtime_active_pool_outflow_threshold"));
        Assert.That(rulesDataSource, Does.Contain("monthly_runtime_settlement_cap"));
        Assert.That(rulesDataSource, Does.Contain("monthly_runtime_household_cap"));
        Assert.That(rulesDataSource, Does.Contain("monthly_runtime_risk_delta"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "MonthlyRuntimeHouseholdCap",
                     "MonthlyRuntimeSettlementCap",
                     "MonthlyRuntimeRiskDelta",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ValidationLedger",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityValidationLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_threshold_no_touch_v581_v588_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-01_household-mobility-runtime-threshold-no-touch-v581-v588.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V581-V588 Household Mobility Runtime Threshold No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime threshold no-touch proof: v581-v588"));
        Assert.That(designAudit, Does.Contain("v581-v588 household mobility runtime threshold no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime threshold no-touch v581-v588 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime threshold no-touch v581-v588 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime threshold no-touch v581-v588 note"));
        Assert.That(uiPresentation, Does.Contain("v581-v588 household mobility runtime threshold no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime threshold no-touch v581-v588 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V581-V588 Household Mobility Runtime Threshold No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Threshold No-Touch Through V588"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime threshold no-touch v581-v588 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime threshold no-touch v581-v588 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "threshold no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No arbitrary script rules",
                     "No runtime assemblies",
                     "No reflection-heavy rule loading",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No recovery/decay formula change",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "no `Household mobility pressure` diff",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleActivePoolThresholdNoTouchesHouseholdsOrPools"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeActivePoolOutflowThreshold = 100"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeRiskDelta = 0"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature(thresholdBlocked)"));
        Assert.That(householdModuleTests, Does.Contain("Household mobility pressure"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The threshold proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeActivePoolOutflowThreshold",
                     "HouseholdMobilityThreshold",
                     "HouseholdMobilityNoTouch",
                     "HouseholdMobilityTargetSelector",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ThresholdNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityValidationLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_zero_cap_no_touch_v589_v596_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-01_household-mobility-runtime-zero-cap-no-touch-v589-v596.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V589-V596 Household Mobility Runtime Zero-Cap No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime zero-cap no-touch proof: v589-v596"));
        Assert.That(designAudit, Does.Contain("v589-v596 household mobility runtime zero-cap no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime zero-cap no-touch v589-v596 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime zero-cap no-touch v589-v596 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime zero-cap no-touch v589-v596 note"));
        Assert.That(uiPresentation, Does.Contain("v589-v596 household mobility runtime zero-cap no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime zero-cap no-touch v589-v596 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V589-V596 Household Mobility Runtime Zero-Cap No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Zero-Cap No-Touch Through V596"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime zero-cap no-touch v589-v596 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime zero-cap no-touch v589-v596 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "zero-cap no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No arbitrary script rules",
                     "No runtime assemblies",
                     "No reflection-heavy rule loading",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No recovery/decay formula change",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "no `Household mobility pressure` diff",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleZeroCapsNoTouchHouseholdsOrPools"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeSettlementCap = 0"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeHouseholdCap = 0"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeRiskDelta = 0"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature(settlementCapBlocked)"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature(householdCapBlocked)"));
        Assert.That(householdModuleTests, Does.Contain("Household mobility pressure"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The zero-cap proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeSettlementCap",
                     "MonthlyRuntimeHouseholdCap",
                     "HouseholdMobilityZeroCap",
                     "HouseholdMobilityTargetSelector",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ZeroCapNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityValidationLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_zero_risk_delta_no_touch_v597_v604_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-zero-risk-delta-no-touch-v597-v604.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V597-V604 Household Mobility Runtime Zero-Risk-Delta No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime zero-risk-delta no-touch proof: v597-v604"));
        Assert.That(designAudit, Does.Contain("v597-v604 household mobility runtime zero-risk-delta no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime zero-risk-delta no-touch v597-v604 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime zero-risk-delta no-touch v597-v604 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime zero-risk-delta no-touch v597-v604 note"));
        Assert.That(uiPresentation, Does.Contain("v597-v604 household mobility runtime zero-risk-delta no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime zero-risk-delta no-touch v597-v604 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V597-V604 Household Mobility Runtime Zero-Risk-Delta No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Zero-Risk-Delta No-Touch Through V604"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime zero-risk-delta no-touch v597-v604 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime zero-risk-delta no-touch v597-v604 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "zero-risk-delta no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No arbitrary script rules",
                     "No runtime assemblies",
                     "No reflection-heavy rule loading",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No recovery/decay formula change",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "no `Household mobility pressure` diff",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleZeroRiskDeltaNoTouchHouseholdsOrPools"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeRiskDelta = 0"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeSettlementCap = 0"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature(riskDeltaBlocked)"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature(settlementCapBlocked)"));
        Assert.That(householdModuleTests, Does.Contain("Household mobility pressure"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The zero-risk-delta proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "MonthlyRuntimeRiskDelta",
                     "HouseholdMobilityZeroRiskDelta",
                     "HouseholdMobilityTargetSelector",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ZeroRiskDeltaNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "HouseholdMobilityValidationLedger",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "Assembly.LoadFrom(",
                     "Activator.CreateInstance(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     ".MobilityDynamicsExplanationSummary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                     "HouseholdMobilityLongRunOptimizer",
                     "HouseholdMobilitySaturationTuner",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MobilitySelector*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_candidate_filter_no_touch_v605_v612_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-candidate-filter-no-touch-v605-v612.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V605-V612 Household Mobility Runtime Candidate Filter No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime candidate-filter no-touch proof: v605-v612"));
        Assert.That(designAudit, Does.Contain("v605-v612 household mobility runtime candidate-filter no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime candidate-filter no-touch v605-v612 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime candidate-filter no-touch v605-v612 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime candidate-filter no-touch v605-v612 note"));
        Assert.That(uiPresentation, Does.Contain("v605-v612 household mobility runtime candidate-filter no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime candidate-filter no-touch v605-v612 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V605-V612 Household Mobility Runtime Candidate Filter No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Candidate Filter No-Touch Through V612"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime candidate-filter no-touch v605-v612 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime candidate-filter no-touch v605-v612 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "candidate-filter no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleCandidateFiltersNoTouchMigratingHighRiskOrBelowFloorHouseholds"));
        Assert.That(householdModuleTests, Does.Contain("alreadyMigrating.MigrationRisk = 82"));
        Assert.That(householdModuleTests, Does.Contain("alreadyMigrating.IsMigrating = true"));
        Assert.That(householdModuleTests, Does.Contain("belowFloor.MigrationRisk = 52"));
        Assert.That(householdModuleTests, Does.Contain("BuildFirstMobilityRuntimeSignature"));
        Assert.That(householdModuleTests, Does.Contain("Household mobility pressure"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The candidate-filter proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityCandidateFilter",
                     "HouseholdMobilityTargetSelector",
                     "HouseholdMobilityNoTouch",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "CandidateFilterNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_tiebreak_no_touch_v613_v620_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-tiebreak-proof-v613-v620.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V613-V620 Household Mobility Runtime Tie-Break No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime tie-break no-touch proof: v613-v620"));
        Assert.That(designAudit, Does.Contain("v613-v620 household mobility runtime tie-break no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime tie-break no-touch v613-v620 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime tie-break no-touch v613-v620 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime tie-break no-touch v613-v620 note"));
        Assert.That(uiPresentation, Does.Contain("v613-v620 household mobility runtime tie-break no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime tie-break no-touch v613-v620 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V613-V620 Household Mobility Runtime Tie-Break No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Tie-Break No-Touch Through V620"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime tie-break no-touch v613-v620 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime tie-break no-touch v613-v620 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "deterministic ordering / tie-break no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No ordering retune",
                     "No score formula retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleTieBreakTouchesLowerHouseholdIdWhenScoresMatch"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeHouseholdCap = 1"));
        Assert.That(householdModuleTests, Does.Contain("ComputeFirstMobilityRuntimeScoreForTest"));
        Assert.That(householdModuleTests, Does.Contain("higher household id remains no-touch"));
        Assert.That(householdModuleTests, Does.Contain("new[] { 1 }"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The tie-break proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain(".ThenBy(static household => household.Id.Value)"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("TieBreakLedger"));
        Assert.That(populationState, Does.Not.Contain("OrderingLedger"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityTieBreak",
                     "HouseholdMobilityTargetSelector",
                     "HouseholdMobilityOrdering",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "TieBreakNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityTieBreakLedger",
                     "HouseholdMobilityOrderingLedger",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_pool_tiebreak_no_touch_v621_v628_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-pool-tiebreak-proof-v621-v628.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V621-V628 Household Mobility Runtime Pool Tie-Break No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime pool tie-break no-touch proof: v621-v628"));
        Assert.That(designAudit, Does.Contain("v621-v628 household mobility runtime pool tie-break no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime pool tie-break no-touch v621-v628 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime pool tie-break no-touch v621-v628 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime pool tie-break no-touch v621-v628 note"));
        Assert.That(uiPresentation, Does.Contain("v621-v628 household mobility runtime pool tie-break no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime pool tie-break no-touch v621-v628 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V621-V628 Household Mobility Runtime Pool Tie-Break No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Pool Tie-Break No-Touch Through V628"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime pool tie-break no-touch v621-v628 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime pool tie-break no-touch v621-v628 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "active-pool deterministic ordering / settlement-id tie-break no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No pool ordering retune",
                     "No score formula retune",
                     "No threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRulePoolTieBreakTouchesLowerSettlementIdWhenOutflowsMatch"));
        Assert.That(householdModuleTests, Does.Contain("GetMigrationPool"));
        Assert.That(householdModuleTests, Does.Contain("higher settlement id remains no-touch"));
        Assert.That(householdModuleTests, Does.Contain("MonthlyRuntimeRiskDelta = 0"));
        Assert.That(householdModuleTests, Does.Contain("new[] { 1, 2 }"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The pool tie-break proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain(".OrderByDescending(static pool => pool.OutflowPressure)"));
        Assert.That(populationModule, Does.Contain(".ThenBy(static pool => pool.SettlementId.Value)"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("PoolTieBreakLedger"));
        Assert.That(populationState, Does.Not.Contain("SettlementOrderingLedger"));
        Assert.That(populationState, Does.Not.Contain("ActivePoolLedger"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityPoolTieBreak",
                     "HouseholdMobilityTargetSelector",
                     "HouseholdMobilityActivePoolOrdering",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "PoolTieBreakNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityPoolTieBreakLedger",
                     "HouseholdMobilitySettlementOrderingLedger",
                     "HouseholdMobilityActivePoolLedger",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_score_ordering_no_touch_v629_v636_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-score-order-proof-v629-v636.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V629-V636 Household Mobility Runtime Score-Ordering No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime score-ordering no-touch proof: v629-v636"));
        Assert.That(designAudit, Does.Contain("v629-v636 household mobility runtime score-ordering no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime score-ordering no-touch v629-v636 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime score-ordering no-touch v629-v636 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime score-ordering no-touch v629-v636 note"));
        Assert.That(uiPresentation, Does.Contain("v629-v636 household mobility runtime score-ordering no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime score-ordering no-touch v629-v636 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V629-V636 Household Mobility Runtime Score-Ordering No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Score-Ordering No-Touch Through V636"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime score-ordering no-touch v629-v636 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime score-ordering no-touch v629-v636 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "candidate score-ordering no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No score formula retune",
                     "No candidate ordering retune",
                     "No pool ordering retune",
                     "No threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleScoreOrderingTouchesHigherScoreBeforeLowerHouseholdId"));
        Assert.That(householdModuleTests, Does.Contain("ComputeFirstMobilityRuntimeScoreForTest"));
        Assert.That(householdModuleTests, Does.Contain("Is.GreaterThan"));
        Assert.That(householdModuleTests, Does.Contain("lower household id remains no-touch"));
        Assert.That(householdModuleTests, Does.Contain("new[] { 2 }"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The score-ordering proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            populationModule,
            Does.Contain(".OrderByDescending(household =>")
                .And.Contain("ComputeMonthlyHouseholdMobilityRuntimeScore(")
                .And.Contain("migrationRiskScoreWeight"));
        Assert.That(populationModule, Does.Contain(".ThenBy(static household => household.Id.Value)"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("ScoreOrderLedger"));
        Assert.That(populationState, Does.Not.Contain("CandidateRankLedger"));
        Assert.That(populationState, Does.Not.Contain("OrderingLedger"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityScoreOrdering",
                     "HouseholdMobilityCandidateScore",
                     "HouseholdMobilityTargetSelector",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ScoreOrderingNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityScoreOrderLedger",
                     "HouseholdMobilityCandidateRankLedger",
                     "HouseholdMobilityOrderingLedger",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_pool_priority_no_touch_v637_v644_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-pool-priority-proof-v637-v644.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V637-V644 Household Mobility Runtime Pool-Priority No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime pool-priority no-touch proof: v637-v644"));
        Assert.That(designAudit, Does.Contain("v637-v644 household mobility runtime pool-priority no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime pool-priority no-touch v637-v644 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime pool-priority no-touch v637-v644 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime pool-priority no-touch v637-v644 note"));
        Assert.That(uiPresentation, Does.Contain("v637-v644 household mobility runtime pool-priority no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime pool-priority no-touch v637-v644 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V637-V644 Household Mobility Runtime Pool-Priority No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Pool-Priority No-Touch Through V644"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime pool-priority no-touch v637-v644 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime pool-priority no-touch v637-v644 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "active-pool priority before cross-pool household score no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No pool ordering retune",
                     "No score formula retune",
                     "No candidate ordering retune",
                     "No threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRulePoolPriorityPrecedesCrossPoolHouseholdScore"));
        Assert.That(householdModuleTests, Does.Contain("active-pool priority before any cross-pool household score comparison"));
        Assert.That(householdModuleTests, Does.Contain("higher-scoring off-pool household remains no-touch"));
        Assert.That(householdModuleTests, Does.Contain("new[] { 2 }"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The pool-priority proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain(".OrderByDescending(static pool => pool.OutflowPressure)"));
        Assert.That(populationModule, Does.Contain(".ThenBy(static pool => pool.SettlementId.Value)"));
        Assert.That(
            populationModule,
            Does.Contain(".OrderByDescending(household =>")
                .And.Contain("ComputeMonthlyHouseholdMobilityRuntimeScore(")
                .And.Contain("migrationRiskScoreWeight"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("PoolPriorityLedger"));
        Assert.That(populationState, Does.Not.Contain("CrossPoolScoreLedger"));
        Assert.That(populationState, Does.Not.Contain("ActivePoolLedger"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityPoolPriority",
                     "HouseholdMobilityCrossPoolScore",
                     "HouseholdMobilityTargetSelector",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "PoolPriorityNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityPoolPriorityLedger",
                     "HouseholdMobilityCrossPoolScoreLedger",
                     "HouseholdMobilityActivePoolLedger",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_per_pool_cap_no_touch_v645_v652_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-per-pool-cap-proof-v645-v652.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V645-V652 Household Mobility Runtime Per-Pool Cap No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime per-pool cap no-touch proof: v645-v652"));
        Assert.That(designAudit, Does.Contain("v645-v652 household mobility runtime per-pool cap no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime per-pool cap no-touch v645-v652 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime per-pool cap no-touch v645-v652 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime per-pool cap no-touch v645-v652 note"));
        Assert.That(uiPresentation, Does.Contain("v645-v652 household mobility runtime per-pool cap no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime per-pool cap no-touch v645-v652 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V645-V652 Household Mobility Runtime Per-Pool Cap No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Per-Pool Cap No-Touch Through V652"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime per-pool cap no-touch v645-v652 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime per-pool cap no-touch v645-v652 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "per-selected-pool household cap evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No pool ordering retune",
                     "No score formula retune",
                     "No candidate ordering retune",
                     "No threshold retune",
                     "No cap semantics retune",
                     "No global household cap",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleHouseholdCapAppliesPerSelectedPool"));
        Assert.That(householdModuleTests, Does.Contain("The second selected pool also receives its own cap-one household touch"));
        Assert.That(householdModuleTests, Does.Contain("the household cap is not global"));
        Assert.That(householdModuleTests, Does.Contain("new[] { 2, 6 }"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The per-pool cap proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain(".Take(settlementCap)"));
        Assert.That(populationModule, Does.Contain(".Take(householdCap)"));
        Assert.That(populationModule, Does.Contain("foreach (MigrationPoolEntryState pool in activePools)"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("TouchCount"));
        Assert.That(populationState, Does.Not.Contain("PerPoolCapLedger"));
        Assert.That(populationState, Does.Not.Contain("GlobalHouseholdCapLedger"));
        Assert.That(populationState, Does.Not.Contain("ActivePoolLedger"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityPerPoolCap",
                     "HouseholdMobilityGlobalHouseholdCap",
                     "HouseholdMobilityTargetSelector",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "PerPoolCapNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityPerPoolCapLedger",
                     "HouseholdMobilityGlobalHouseholdCapLedger",
                     "HouseholdMobilityActivePoolLedger",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_threshold_event_no_touch_v653_v660_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-threshold-event-proof-v653-v660.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V653-V660 Household Mobility Runtime Threshold Event No-Touch Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime threshold-event no-touch proof: v653-v660"));
        Assert.That(designAudit, Does.Contain("v653-v660 household mobility runtime threshold-event no-touch audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime threshold-event no-touch v653-v660 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime threshold-event no-touch v653-v660 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime threshold-event no-touch v653-v660 note"));
        Assert.That(uiPresentation, Does.Contain("v653-v660 household mobility runtime threshold-event no-touch"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime threshold-event no-touch v653-v660 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V653-V660 Household Mobility Runtime Threshold Event No-Touch Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Threshold Event No-Touch Through V660"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime threshold-event no-touch v653-v660 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime threshold-event no-touch v653-v660 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "threshold event no-touch evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No new event type",
                     "No event routing change",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleEmitsThresholdEventOnlyForSelectedCrossingHousehold"));
        Assert.That(householdModuleTests, Does.Contain("PopulationEventNames.MigrationStarted"));
        Assert.That(householdModuleTests, Does.Contain("DomainEventMetadataKeys.Cause"));
        Assert.That(householdModuleTests, Does.Contain("DomainEventMetadataValues.CauseSocialPressure"));
        Assert.That(householdModuleTests, Does.Contain("new[] { \"2\" }"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The threshold event proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("PopulationEventNames.MigrationStarted"));
        Assert.That(populationModule, Does.Contain("oldMigrationRisk < 80 && household.MigrationRisk >= 80"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("ThresholdEventLedger"));
        Assert.That(populationState, Does.Not.Contain("RoutingLedger"));
        Assert.That(populationState, Does.Not.Contain("MigrationStartedSelector"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityThresholdEvent",
                     "HouseholdMobilityMigrationStartedSelector",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ThresholdEventNoTouch",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityThresholdEventLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_event_metadata_no_prose_v661_v668_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-event-metadata-proof-v661-v668.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V661-V668 Household Mobility Runtime Event Metadata No-Prose Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime event-metadata no-prose proof: v661-v668"));
        Assert.That(designAudit, Does.Contain("v661-v668 household mobility runtime event-metadata no-prose audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime event-metadata no-prose v661-v668 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime event-metadata no-prose v661-v668 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime event-metadata no-prose v661-v668 note"));
        Assert.That(uiPresentation, Does.Contain("v661-v668 household mobility runtime event-metadata no-prose"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime event-metadata no-prose v661-v668 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V661-V668 Household Mobility Runtime Event Metadata No-Prose Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Event Metadata No-Prose Through V668"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime event-metadata no-prose v661-v668 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime event-metadata no-prose v661-v668 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "event metadata no-prose evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No new event type",
                     "No event routing change",
                     "No `DomainEvent.Summary` parsing",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleThresholdEventCarriesMetadataWithoutSummaryParsing"));
        Assert.That(householdModuleTests, Does.Contain("metadataTuple"));
        Assert.That(householdModuleTests, Does.Contain("social-pressure:1:2"));
        Assert.That(householdModuleTests, Does.Contain("thresholdEvent.Summary"));
        Assert.That(householdModuleTests, Does.Contain("Does.Not.Contain(DomainEventMetadataKeys.SettlementId)"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The event metadata proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("PopulationEventNames.MigrationStarted"));
        Assert.That(populationModule, Does.Contain("DomainEventMetadataKeys.Cause"));
        Assert.That(populationModule, Does.Contain("DomainEventMetadataKeys.SettlementId"));
        Assert.That(populationModule, Does.Contain("DomainEventMetadataKeys.HouseholdId"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("EventMetadataLedger"));
        Assert.That(populationState, Does.Not.Contain("ProseParsingLedger"));
        Assert.That(populationState, Does.Not.Contain("RoutingLedger"));
        Assert.That(populationState, Does.Not.Contain("MigrationStartedSelector"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityEventMetadataInterpreter",
                     "HouseholdMobilitySummaryParser",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "EventMetadataNoProse",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityTouchCountState",
                     "HouseholdMobilityDiagnosticState",
                     "HouseholdMobilityPerformanceCache",
                     "HouseholdMobilityEventMetadataLedger",
                     "HouseholdMobilityProseParsingLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "DomainEventSummaryParser",
                     "MigrationStartedSummaryParser",
                     "HouseholdMobilitySummaryParser",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_event_metadata_replay_v669_v676_must_remain_test_evidence_only_without_runtime_or_schema_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-02_household-mobility-runtime-event-metadata-replay-proof-v669-v676.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V669-V676 Household Mobility Runtime Event Metadata Replay Proof"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime event-metadata replay proof: v669-v676"));
        Assert.That(designAudit, Does.Contain("v669-v676 household mobility runtime event-metadata replay audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime event-metadata replay v669-v676 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime event-metadata replay v669-v676 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime event-metadata replay v669-v676 note"));
        Assert.That(uiPresentation, Does.Contain("v669-v676 household mobility runtime event-metadata replay"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime event-metadata replay v669-v676 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V669-V676 Household Mobility Runtime Event Metadata Replay Proof"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Event Metadata Replay Through V676"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime event-metadata replay v669-v676 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime event-metadata replay v669-v676 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "same-seed replay evidence",
                     "No runtime behavior change",
                     "No fanout widening",
                     "No new event type",
                     "No event routing change",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleThresholdEventMetadataReplayStable"));
        Assert.That(householdModuleTests, Does.Contain("BuildThresholdEventMetadataSignature"));
        Assert.That(householdModuleTests, Does.Contain("thresholdEvent.Metadata[DomainEventMetadataKeys.Cause]"));
        Assert.That(householdModuleTests, Does.Contain("thresholdEvent.Summary"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The event metadata replay proof must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(populationModule, Does.Contain("PopulationEventNames.MigrationStarted"));
        Assert.That(populationModule, Does.Contain("DomainEventMetadataKeys.Cause"));
        Assert.That(populationModule, Does.Contain("DomainEventMetadataKeys.SettlementId"));
        Assert.That(populationModule, Does.Contain("DomainEventMetadataKeys.HouseholdId"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("EventReplayState"));
        Assert.That(populationState, Does.Not.Contain("EventMetadataLedger"));
        Assert.That(populationState, Does.Not.Contain("RoutingLedger"));
        Assert.That(populationState, Does.Not.Contain("MigrationStartedSelector"));
        Assert.That(populationState, Does.Not.Contain("DiagnosticState"));
        Assert.That(populationState, Does.Not.Contain("PerformanceCache"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityEventReplaySignature",
                     "HouseholdMobilityEventMetadataInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "EventMetadataReplay",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventReplayState",
                     "HouseholdMobilityEventReplayLedger",
                     "HouseholdMobilityEventMetadataLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_threshold_extraction_v677_v684_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-threshold-extraction-v677-v684.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V677-V684 Household Mobility Runtime Threshold Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime threshold extraction: v677-v684"));
        Assert.That(designAudit, Does.Contain("v677-v684 household mobility runtime threshold extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime threshold extraction v677-v684 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime threshold extraction v677-v684 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime threshold extraction v677-v684 note"));
        Assert.That(uiPresentation, Does.Contain("v677-v684 household mobility runtime threshold extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime threshold extraction v677-v684 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V677-V684 Household Mobility Runtime Threshold Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Threshold Extraction Through V684"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime threshold extraction v677-v684 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime threshold extraction v677-v684 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default event threshold remains 80",
                     "No runtime behavior change under default rules-data",
                     "No fanout widening",
                     "No candidate filter retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeMigrationStartedEventThreshold"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeMigrationStartedEventThreshold = 80"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_migration_started_event_threshold"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault"));
        Assert.That(populationModule, Does.Contain("PopulationEventNames.MigrationStarted"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The threshold extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultMigrationStartedEventThresholdPreservesPreviousEventBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationStartedEventThresholdFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeMigrationStartedEventThreshold"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("ThresholdExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeMigrationStartedEventThreshold",
                     "GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityThresholdInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ThresholdExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityThresholdExtractionState",
                     "HouseholdMobilityThresholdExtractionLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_candidate_floor_extraction_v685_v692_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-candidate-floor-extraction-v685-v692.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V685-V692 Household Mobility Runtime Candidate Floor Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime candidate-floor extraction: v685-v692"));
        Assert.That(designAudit, Does.Contain("v685-v692 household mobility runtime candidate-floor extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime candidate-floor extraction v685-v692 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime candidate-floor extraction v685-v692 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime candidate-floor extraction v685-v692 note"));
        Assert.That(uiPresentation, Does.Contain("v685-v692 household mobility runtime candidate-floor extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime candidate-floor extraction v685-v692 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V685-V692 Household Mobility Runtime Candidate Floor Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Candidate Floor Extraction Through V692"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime candidate-floor extraction v685-v692 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime candidate-floor extraction v685-v692 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default candidate floor remains 55",
                     "No runtime behavior change under default rules-data",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No general migration-state threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeCandidateMigrationRiskFloor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeCandidateMigrationRiskFloor = 55"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_candidate_migration_risk_floor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("candidateMigrationRiskFloor"));
        Assert.That(populationModule, Does.Contain("household.MigrationRisk < candidateMigrationRiskFloor"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The candidate-floor extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskFloorPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskFloorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeCandidateMigrationRiskFloor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("CandidateFloorExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeCandidateMigrationRiskFloor",
                     "GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityCandidateFloorInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "CandidateFloorExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityCandidateFloorState",
                     "HouseholdMobilityCandidateFloorLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_score_weight_extraction_v693_v700_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-score-weight-extraction-v693-v700.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V693-V700 Household Mobility Runtime Score Weight Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime score-weight extraction: v693-v700"));
        Assert.That(designAudit, Does.Contain("v693-v700 household mobility runtime score-weight extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime score-weight extraction v693-v700 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime score-weight extraction v693-v700 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime score-weight extraction v693-v700 note"));
        Assert.That(uiPresentation, Does.Contain("v693-v700 household mobility runtime score-weight extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime score-weight extraction v693-v700 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V693-V700 Household Mobility Runtime Score Weight Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Score Weight Extraction Through V700"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime score-weight extraction v693-v700 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime score-weight extraction v693-v700 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default migration-risk score weight remains 4",
                     "No runtime behavior change under default rules-data",
                     "No score formula retune",
                     "No livelihood/labor/grain/land weight extraction",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeMigrationRiskScoreWeight"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeMigrationRiskScoreWeight = 4"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_migration_risk_score_weight"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault"));
        Assert.That(populationModule, Does.Contain("household.MigrationRisk * migrationRiskScoreWeight"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The score-weight extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultMigrationRiskScoreWeightPreservesPreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationRiskScoreWeightFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeMigrationRiskScoreWeight"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("ScoreWeightExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeMigrationRiskScoreWeight",
                     "GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityScoreWeightInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "ScoreWeightExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityScoreWeightState",
                     "HouseholdMobilityScoreWeightLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_labor_floor_extraction_v701_v708_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-labor-floor-extraction-v701-v708.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V701-V708 Household Mobility Runtime Labor Floor Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime labor-floor extraction: v701-v708"));
        Assert.That(designAudit, Does.Contain("v701-v708 household mobility runtime labor-floor extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime labor-floor extraction v701-v708 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime labor-floor extraction v701-v708 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime labor-floor extraction v701-v708 note"));
        Assert.That(uiPresentation, Does.Contain("v701-v708 household mobility runtime labor-floor extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime labor-floor extraction v701-v708 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V701-V708 Household Mobility Runtime Labor Floor Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Labor Floor Extraction Through V708"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime labor-floor extraction v701-v708 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime labor-floor extraction v701-v708 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default labor-capacity pressure floor remains 60",
                     "No runtime behavior change under default rules-data",
                     "No labor model retune",
                     "No score formula retune beyond literal extraction",
                     "No migration-risk weight retune",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLaborCapacityPressureFloor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeLaborCapacityPressureFloor = 60"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_labor_capacity_pressure_floor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("laborCapacityPressureFloor - household.LaborCapacity"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The labor-floor extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultLaborCapacityPressureFloorPreservesPreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLaborCapacityPressureFloorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeLaborCapacityPressureFloor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LaborFloorExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeLaborCapacityPressureFloor",
                     "GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityLaborPressureInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LaborFloorExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityLaborFloorState",
                     "HouseholdMobilityLaborFloorLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_grain_floor_extraction_v709_v716_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-grain-floor-extraction-v709-v716.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V709-V716 Household Mobility Runtime Grain Floor Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime grain-floor extraction: v709-v716"));
        Assert.That(designAudit, Does.Contain("v709-v716 household mobility runtime grain-floor extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime grain-floor extraction v709-v716 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime grain-floor extraction v709-v716 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime grain-floor extraction v709-v716 note"));
        Assert.That(uiPresentation, Does.Contain("v709-v716 household mobility runtime grain-floor extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime grain-floor extraction v709-v716 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V709-V716 Household Mobility Runtime Grain Floor Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Grain Floor Extraction Through V716"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime grain-floor extraction v709-v716 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime grain-floor extraction v709-v716 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default grain-store pressure floor remains 25",
                     "No runtime behavior change under default rules-data",
                     "No grain economy retune",
                     "No grain pressure divisor extraction",
                     "No score formula retune beyond literal extraction",
                     "No migration-risk weight retune",
                     "No labor-floor retune",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeGrainStorePressureFloor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeGrainStorePressureFloor = 25"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeGrainStorePressureFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_grain_store_pressure_floor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeGrainStorePressureFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("grainStorePressureFloor - household.GrainStore"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The grain-floor extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultGrainStorePressureFloorPreservesPreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStorePressureFloorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeGrainStorePressureFloor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("GrainFloorExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeGrainStorePressureFloor",
                     "GetMonthlyRuntimeGrainStorePressureFloorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityGrainPressureInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "GrainFloorExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GrainEconomyEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityGrainFloorState",
                     "HouseholdMobilityGrainFloorLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_land_floor_extraction_v717_v724_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-land-floor-extraction-v717-v724.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V717-V724 Household Mobility Runtime Land Floor Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime land-floor extraction: v717-v724"));
        Assert.That(designAudit, Does.Contain("v717-v724 household mobility runtime land-floor extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime land-floor extraction v717-v724 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime land-floor extraction v717-v724 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime land-floor extraction v717-v724 note"));
        Assert.That(uiPresentation, Does.Contain("v717-v724 household mobility runtime land-floor extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime land-floor extraction v717-v724 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V717-V724 Household Mobility Runtime Land Floor Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Land Floor Extraction Through V724"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime land-floor extraction v717-v724 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime land-floor extraction v717-v724 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default land-holding pressure floor remains 20",
                     "No runtime behavior change under default rules-data",
                     "No land economy retune",
                     "No land pressure divisor extraction",
                     "No class/status engine",
                     "No score formula retune beyond literal extraction",
                     "No migration-risk weight retune",
                     "No labor-floor retune",
                     "No grain-floor retune",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLandHoldingPressureFloor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeLandHoldingPressureFloor = 20"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLandHoldingPressureFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_land_holding_pressure_floor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeLandHoldingPressureFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("landHoldingPressureFloor - household.LandHolding"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The land-floor extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultLandHoldingPressureFloorPreservesPreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLandHoldingPressureFloorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeLandHoldingPressureFloor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LandFloorExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeLandHoldingPressureFloor",
                     "GetMonthlyRuntimeLandHoldingPressureFloorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityLandPressureInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LandFloorExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "LandEconomyEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityLandFloorState",
                     "HouseholdMobilityLandFloorLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_grain_divisor_extraction_v725_v732_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-grain-divisor-extraction-v725-v732.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V725-V732 Household Mobility Runtime Grain Divisor Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime grain-divisor extraction: v725-v732"));
        Assert.That(designAudit, Does.Contain("v725-v732 household mobility runtime grain-divisor extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime grain-divisor extraction v725-v732 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime grain-divisor extraction v725-v732 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime grain-divisor extraction v725-v732 note"));
        Assert.That(uiPresentation, Does.Contain("v725-v732 household mobility runtime grain-divisor extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime grain-divisor extraction v725-v732 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V725-V732 Household Mobility Runtime Grain Divisor Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Grain Divisor Extraction Through V732"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime grain-divisor extraction v725-v732 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime grain-divisor extraction v725-v732 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default grain-store pressure divisor remains 2",
                     "No runtime behavior change under default rules-data",
                     "No grain economy retune",
                     "No grain floor retune",
                     "No land pressure divisor extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeGrainStorePressureDivisor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeGrainStorePressureDivisor = 2"));
        Assert.That(rulesData, Does.Contain("MaxMonthlyRuntimeGrainStorePressureDivisor = 16"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeGrainStorePressureDivisor is < 1"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeGrainStorePressureDivisorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_grain_store_pressure_divisor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeGrainStorePressureDivisorOrDefault"));
        Assert.That(populationModule, Does.Contain("grainStorePressureFloor - household.GrainStore"));
        Assert.That(populationModule, Does.Contain("/ grainStorePressureDivisor"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The grain-divisor extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultGrainStorePressureDivisorPreservesPreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStorePressureDivisorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeGrainStorePressureDivisor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("GrainDivisorExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeGrainStorePressureDivisor",
                     "GetMonthlyRuntimeGrainStorePressureDivisorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityGrainPressureInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "GrainDivisorExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "GrainEconomyEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityGrainDivisorState",
                     "HouseholdMobilityGrainDivisorLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_land_divisor_extraction_v733_v740_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-land-divisor-extraction-v733-v740.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V733-V740 Household Mobility Runtime Land Divisor Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime land-divisor extraction: v733-v740"));
        Assert.That(designAudit, Does.Contain("v733-v740 household mobility runtime land-divisor extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime land-divisor extraction v733-v740 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime land-divisor extraction v733-v740 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime land-divisor extraction v733-v740 note"));
        Assert.That(uiPresentation, Does.Contain("v733-v740 household mobility runtime land-divisor extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime land-divisor extraction v733-v740 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V733-V740 Household Mobility Runtime Land Divisor Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Land Divisor Extraction Through V740"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime land-divisor extraction v733-v740 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime land-divisor extraction v733-v740 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default land-holding pressure divisor remains 2",
                     "No runtime behavior change under default rules-data",
                     "No land economy retune",
                     "No land floor retune",
                     "No grain pressure divisor extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No high-risk filter retune",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLandHoldingPressureDivisor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeLandHoldingPressureDivisor = 2"));
        Assert.That(rulesData, Does.Contain("MaxMonthlyRuntimeLandHoldingPressureDivisor = 16"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLandHoldingPressureDivisor is < 1"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_land_holding_pressure_divisor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault"));
        Assert.That(populationModule, Does.Contain("landHoldingPressureFloor - household.LandHolding"));
        Assert.That(populationModule, Does.Contain("/ landHoldingPressureDivisor"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The land-divisor extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultLandHoldingPressureDivisorPreservesPreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLandHoldingPressureDivisorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeLandHoldingPressureDivisor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LandDivisorExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeLandHoldingPressureDivisor",
                     "GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityLandPressureInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LandDivisorExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "LandEconomyEngine",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityLandDivisorState",
                     "HouseholdMobilityLandDivisorLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_candidate_ceiling_extraction_v741_v748_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-candidate-ceiling-extraction-v741-v748.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V741-V748 Household Mobility Runtime Candidate Ceiling Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime candidate-ceiling extraction: v741-v748"));
        Assert.That(designAudit, Does.Contain("v741-v748 household mobility runtime candidate-ceiling extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime candidate-ceiling extraction v741-v748 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime candidate-ceiling extraction v741-v748 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime candidate-ceiling extraction v741-v748 note"));
        Assert.That(uiPresentation, Does.Contain("v741-v748 household mobility runtime candidate-ceiling extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime candidate-ceiling extraction v741-v748 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V741-V748 Household Mobility Runtime Candidate Ceiling Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Candidate Ceiling Extraction Through V748"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime candidate-ceiling extraction v741-v748 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime candidate-ceiling extraction v741-v748 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default candidate migration-risk ceiling remains 80",
                     "No runtime behavior change under default rules-data",
                     "No migration-started event threshold retune",
                     "No candidate floor retune",
                     "No active-pool threshold retune",
                     "No trigger threshold extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No event threshold retune",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeCandidateMigrationRiskCeiling"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeCandidateMigrationRiskCeiling = 80"));
        Assert.That(rulesData, Does.Contain("MaxMonthlyRuntimeCandidateMigrationRiskCeiling = 100"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeCandidateMigrationRiskCeiling is < 1"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_candidate_migration_risk_ceiling"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault"));
        Assert.That(populationModule, Does.Contain("candidateMigrationRiskCeiling"));
        Assert.That(populationModule, Does.Contain("household.MigrationRisk >= candidateMigrationRiskCeiling"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The candidate-ceiling extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskCeilingPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskCeilingFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeCandidateMigrationRiskCeiling"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("CandidateCeilingExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeCandidateMigrationRiskCeiling",
                     "GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityCandidateCeilingInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "CandidateCeilingExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "CandidateFloorRetune",
                     "MigrationStartedEventThresholdRetune",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityCandidateCeilingState",
                     "HouseholdMobilityCandidateCeilingLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_distress_trigger_extraction_v749_v756_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-distress-trigger-extraction-v749-v756.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V749-V756 Household Mobility Runtime Distress Trigger Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime distress-trigger extraction: v749-v756"));
        Assert.That(designAudit, Does.Contain("v749-v756 household mobility runtime distress-trigger extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime distress-trigger extraction v749-v756 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime distress-trigger extraction v749-v756 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime distress-trigger extraction v749-v756 note"));
        Assert.That(uiPresentation, Does.Contain("v749-v756 household mobility runtime distress-trigger extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime distress-trigger extraction v749-v756 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V749-V756 Household Mobility Runtime Distress Trigger Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Distress Trigger Extraction Through V756"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime distress-trigger extraction v749-v756 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime distress-trigger extraction v749-v756 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default distress trigger threshold remains 60",
                     "No runtime behavior change under default rules-data",
                     "No distress economy retune",
                     "No debt trigger extraction",
                     "No labor trigger extraction",
                     "No grain trigger extraction",
                     "No land trigger extraction",
                     "No livelihood trigger extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeDistressTriggerThreshold"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeDistressTriggerThreshold = 60"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeDistressTriggerThreshold is < 0 or > 100"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeDistressTriggerThresholdOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_distress_trigger_threshold"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeDistressTriggerThresholdOrDefault"));
        Assert.That(populationModule, Does.Contain("distressTriggerThreshold"));
        Assert.That(populationModule, Does.Contain("household.Distress >= distressTriggerThreshold"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The distress-trigger extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultDistressTriggerThresholdPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDistressTriggerThresholdFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeDistressTriggerThreshold"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("DistressTriggerExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeDistressTriggerThreshold",
                     "GetMonthlyRuntimeDistressTriggerThresholdOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityDistressTriggerInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "DistressTriggerExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "DebtTriggerExtractionState",
                     "LaborTriggerExtractionState",
                     "GrainTriggerExtractionState",
                     "LandTriggerExtractionState",
                     "LivelihoodTriggerExtractionState",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityDistressTriggerState",
                     "HouseholdMobilityDistressTriggerLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_debt_trigger_extraction_v757_v764_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-debt-trigger-extraction-v757-v764.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V757-V764 Household Mobility Runtime Debt Trigger Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime debt-trigger extraction: v757-v764"));
        Assert.That(designAudit, Does.Contain("v757-v764 household mobility runtime debt-trigger extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime debt-trigger extraction v757-v764 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime debt-trigger extraction v757-v764 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime debt-trigger extraction v757-v764 note"));
        Assert.That(uiPresentation, Does.Contain("v757-v764 household mobility runtime debt-trigger extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime debt-trigger extraction v757-v764 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V757-V764 Household Mobility Runtime Debt Trigger Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Debt Trigger Extraction Through V764"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime debt-trigger extraction v757-v764 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime debt-trigger extraction v757-v764 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default debt-pressure trigger threshold remains 60",
                     "No runtime behavior change under default rules-data",
                     "No debt economy retune",
                     "No distress trigger retune",
                     "No labor trigger extraction",
                     "No grain trigger extraction",
                     "No land trigger extraction",
                     "No livelihood trigger extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeDebtPressureTriggerThreshold"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeDebtPressureTriggerThreshold = 60"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeDebtPressureTriggerThreshold is < 0 or > 100"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_debt_pressure_trigger_threshold"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault"));
        Assert.That(populationModule, Does.Contain("debtPressureTriggerThreshold"));
        Assert.That(populationModule, Does.Contain("household.DebtPressure >= debtPressureTriggerThreshold"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The debt-trigger extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultDebtPressureTriggerThresholdPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDebtPressureTriggerThresholdFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeDebtPressureTriggerThreshold"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("DebtTriggerExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeDebtPressureTriggerThreshold",
                     "GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityDebtTriggerInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "DebtTriggerExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "LaborTriggerExtractionState",
                     "GrainTriggerExtractionState",
                     "LandTriggerExtractionState",
                     "LivelihoodTriggerExtractionState",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityDebtTriggerState",
                     "HouseholdMobilityDebtTriggerLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_labor_trigger_extraction_v765_v772_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-labor-trigger-extraction-v765-v772.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V765-V772 Household Mobility Runtime Labor Trigger Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime labor-trigger extraction: v765-v772"));
        Assert.That(designAudit, Does.Contain("v765-v772 household mobility runtime labor-trigger extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime labor-trigger extraction v765-v772 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime labor-trigger extraction v765-v772 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime labor-trigger extraction v765-v772 note"));
        Assert.That(uiPresentation, Does.Contain("v765-v772 household mobility runtime labor-trigger extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime labor-trigger extraction v765-v772 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V765-V772 Household Mobility Runtime Labor Trigger Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Labor Trigger Extraction Through V772"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime labor-trigger extraction v765-v772 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime labor-trigger extraction v765-v772 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default labor-capacity trigger ceiling remains 45",
                     "No runtime behavior change under default rules-data",
                     "No labor model retune",
                     "No debt trigger retune",
                     "No distress trigger retune",
                     "No grain trigger extraction",
                     "No land trigger extraction",
                     "No livelihood trigger extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLaborCapacityTriggerCeiling"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeLaborCapacityTriggerCeiling = 45"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLaborCapacityTriggerCeiling is < 0 or > 100"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_labor_capacity_trigger_ceiling"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault"));
        Assert.That(populationModule, Does.Contain("laborCapacityTriggerCeiling"));
        Assert.That(populationModule, Does.Contain("household.LaborCapacity < laborCapacityTriggerCeiling"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The labor-trigger extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultLaborCapacityTriggerCeilingPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLaborCapacityTriggerCeilingFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeLaborCapacityTriggerCeiling"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LaborTriggerExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeLaborCapacityTriggerCeiling",
                     "GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityLaborTriggerInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LaborTriggerExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityLaborTriggerState",
                     "HouseholdMobilityLaborTriggerLedger",
                     "GrainTriggerExtractionState",
                     "LandTriggerExtractionState",
                     "LivelihoodTriggerExtractionState",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_grain_trigger_extraction_v773_v780_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-grain-trigger-extraction-v773-v780.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V773-V780 Household Mobility Runtime Grain Trigger Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime grain-trigger extraction: v773-v780"));
        Assert.That(designAudit, Does.Contain("v773-v780 household mobility runtime grain-trigger extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime grain-trigger extraction v773-v780 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime grain-trigger extraction v773-v780 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime grain-trigger extraction v773-v780 note"));
        Assert.That(uiPresentation, Does.Contain("v773-v780 household mobility runtime grain-trigger extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime grain-trigger extraction v773-v780 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V773-V780 Household Mobility Runtime Grain Trigger Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Grain Trigger Extraction Through V780"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime grain-trigger extraction v773-v780 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime grain-trigger extraction v773-v780 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default grain-store trigger floor remains 25",
                     "No runtime behavior change under default rules-data",
                     "No grain economy retune",
                     "No labor trigger retune",
                     "No debt trigger retune",
                     "No distress trigger retune",
                     "No land trigger extraction",
                     "No livelihood trigger extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeGrainStoreTriggerFloor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeGrainStoreTriggerFloor = 25"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeGrainStoreTriggerFloor is < 0 or > 100"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_grain_store_trigger_floor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("grainStoreTriggerFloor"));
        Assert.That(populationModule, Does.Contain("household.GrainStore < grainStoreTriggerFloor"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The grain-trigger extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultGrainStoreTriggerFloorPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStoreTriggerFloorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeGrainStoreTriggerFloor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("GrainTriggerExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeGrainStoreTriggerFloor",
                     "GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityGrainTriggerInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "GrainTriggerExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityGrainTriggerState",
                     "HouseholdMobilityGrainTriggerLedger",
                     "LandTriggerExtractionState",
                     "LivelihoodTriggerExtractionState",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_land_trigger_extraction_v781_v788_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-land-trigger-extraction-v781-v788.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V781-V788 Household Mobility Runtime Land Trigger Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime land-trigger extraction: v781-v788"));
        Assert.That(designAudit, Does.Contain("v781-v788 household mobility runtime land-trigger extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime land-trigger extraction v781-v788 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime land-trigger extraction v781-v788 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime land-trigger extraction v781-v788 note"));
        Assert.That(uiPresentation, Does.Contain("v781-v788 household mobility runtime land-trigger extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime land-trigger extraction v781-v788 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V781-V788 Household Mobility Runtime Land Trigger Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Land Trigger Extraction Through V788"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime land-trigger extraction v781-v788 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime land-trigger extraction v781-v788 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default land-holding trigger floor remains 15",
                     "No runtime behavior change under default rules-data",
                     "No land economy retune",
                     "No grain trigger retune",
                     "No labor trigger retune",
                     "No debt trigger retune",
                     "No distress trigger retune",
                     "No livelihood trigger extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLandHoldingTriggerFloor"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeLandHoldingTriggerFloor = 15"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLandHoldingTriggerFloor is < 0 or > 100"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_land_holding_trigger_floor"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("landHoldingTriggerFloor"));
        Assert.That(populationModule, Does.Contain("household.LandHolding < landHoldingTriggerFloor"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The land-trigger extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultLandHoldingTriggerFloorPreservesPreviousNoTouchBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLandHoldingTriggerFloorFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeLandHoldingTriggerFloor"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LandTriggerExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeLandHoldingTriggerFloor",
                     "GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityLandTriggerInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LandTriggerExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityLandTriggerState",
                     "HouseholdMobilityLandTriggerLedger",
                     "LivelihoodTriggerExtractionState",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_livelihood_trigger_extraction_v789_v796_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-livelihood-trigger-extraction-v789-v796.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V789-V796 Household Mobility Runtime Livelihood Trigger Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime livelihood-trigger extraction: v789-v796"));
        Assert.That(designAudit, Does.Contain("v789-v796 household mobility runtime livelihood-trigger extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime livelihood-trigger extraction v789-v796 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime livelihood-trigger extraction v789-v796 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime livelihood-trigger extraction v789-v796 note"));
        Assert.That(uiPresentation, Does.Contain("v789-v796 household mobility runtime livelihood-trigger extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime livelihood-trigger extraction v789-v796 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V789-V796 Household Mobility Runtime Livelihood Trigger Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Livelihood Trigger Extraction Through V796"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime livelihood-trigger extraction v789-v796 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime livelihood-trigger extraction v789-v796 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default trigger livelihood list remains `[SeasonalMigrant, HiredLabor]`",
                     "No runtime behavior change under default rules-data",
                     "No livelihood engine retune",
                     "No livelihood score weight extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeTriggerLivelihoods"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeTriggerLivelihoods"));
        Assert.That(rulesData, Does.Contain("LivelihoodType.SeasonalMigrant, LivelihoodType.HiredLabor"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeTriggerLivelihoodsOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_trigger_livelihoods"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeTriggerLivelihoodsOrDefault"));
        Assert.That(populationModule, Does.Contain("triggerLivelihoods"));
        Assert.That(populationModule, Does.Contain("triggerLivelihoods.Contains(household.Livelihood)"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The livelihood-trigger extraction must not add or duplicate the monthly household mobility runtime rule path.");
        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultTriggerLivelihoodsPreservePreviousCandidateBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeTriggerLivelihoodsFallBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeTriggerLivelihoods"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LivelihoodTriggerExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeTriggerLivelihoods",
                     "GetMonthlyRuntimeTriggerLivelihoodsOrDefault",
                     "ApplyMonthlyHouseholdMobilityRuntimeRule",
                     "HouseholdMobilityLivelihoodTriggerInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LivelihoodTriggerExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityLivelihoodTriggerState",
                     "HouseholdMobilityLivelihoodTriggerLedger",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_livelihood_score_weight_extraction_v797_v804_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-livelihood-score-weight-extraction-v797-v804.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V797-V804 Household Mobility Runtime Livelihood Score Weight Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime livelihood-score extraction: v797-v804"));
        Assert.That(designAudit, Does.Contain("v797-v804 household mobility runtime livelihood-score extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime livelihood-score extraction v797-v804 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime livelihood-score extraction v797-v804 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime livelihood-score extraction v797-v804 note"));
        Assert.That(uiPresentation, Does.Contain("v797-v804 household mobility runtime livelihood-score extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime livelihood-score extraction v797-v804 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V797-V804 Household Mobility Runtime Livelihood Score Weight Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Livelihood Score Weight Extraction Through V804"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime livelihood-score extraction v797-v804 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime livelihood-score extraction v797-v804 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "SeasonalMigrant = 18",
                     "HiredLabor = 10",
                     "Tenant = 6",
                     "unmatched livelihoods score `0`",
                     "No runtime behavior change under default rules-data",
                     "No livelihood engine retune",
                     "No livelihood trigger extraction",
                     "No migration-risk score weight extraction",
                     "No pressure floor/divisor extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeLivelihoodScoreWeights"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeLivelihoodScoreWeights"));
        Assert.That(rulesData, Does.Contain("PopulationHouseholdMobilityLivelihoodScoreWeight"));
        Assert.That(rulesData, Does.Contain("LivelihoodType.SeasonalMigrant, 18"));
        Assert.That(rulesData, Does.Contain("LivelihoodType.HiredLabor, 10"));
        Assert.That(rulesData, Does.Contain("LivelihoodType.Tenant, 6"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeLivelihoodScoreWeightOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_livelihood_score_weights"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault"));
        Assert.That(populationModule, Does.Contain("ResolveMonthlyHouseholdMobilityLivelihoodScoreWeight"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The livelihood score-weight extraction must not add or duplicate the monthly household mobility runtime rule path.");

        int scoreStart = populationModule.IndexOf(
            "private static int ComputeMonthlyHouseholdMobilityRuntimeScore",
            StringComparison.Ordinal);
        int scoreEnd = populationModule.IndexOf(
            "private static int ResolveMonthlyHouseholdMobilityLivelihoodScoreWeight",
            scoreStart,
            StringComparison.Ordinal);
        Assert.That(scoreStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(scoreEnd, Is.GreaterThan(scoreStart));
        string scoreMethod = populationModule[scoreStart..scoreEnd];
        Assert.That(scoreMethod, Does.Not.Contain("LivelihoodType.SeasonalMigrant => 18"));
        Assert.That(scoreMethod, Does.Not.Contain("LivelihoodType.HiredLabor => 10"));
        Assert.That(scoreMethod, Does.Not.Contain("LivelihoodType.Tenant => 6"));

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultLivelihoodScoreWeightsPreservePreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLivelihoodScoreWeightsFallBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeLivelihoodScoreWeights"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("LivelihoodScoreExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeLivelihoodScoreWeights",
                     "GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault",
                     "ResolveMonthlyHouseholdMobilityLivelihoodScoreWeight",
                     "HouseholdMobilityLivelihoodScoreInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "LivelihoodScoreExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityLivelihoodScoreState",
                     "HouseholdMobilityLivelihoodScoreLedger",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_pressure_score_weight_extraction_v805_v812_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-pressure-score-weight-extraction-v805-v812.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V805-V812 Household Mobility Runtime Pressure Score Weight Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime pressure-score extraction: v805-v812"));
        Assert.That(designAudit, Does.Contain("v805-v812 household mobility runtime pressure-score extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime pressure-score extraction v805-v812 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime pressure-score extraction v805-v812 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime pressure-score extraction v805-v812 note"));
        Assert.That(uiPresentation, Does.Contain("v805-v812 household mobility runtime pressure-score extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime pressure-score extraction v805-v812 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V805-V812 Household Mobility Runtime Pressure Score Weight Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Pressure Score Weight Extraction Through V812"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime pressure-score extraction v805-v812 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime pressure-score extraction v805-v812 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "Distress = 1",
                     "DebtPressure = 1",
                     "No runtime behavior change under default rules-data",
                     "No pressure formula retune",
                     "No livelihood engine retune",
                     "No trigger extraction",
                     "No migration-risk score weight extraction",
                     "No livelihood score weight extraction",
                     "No pressure floor/divisor extraction",
                     "No score formula retune beyond literal extraction",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeDistressScoreWeight"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeDebtPressureScoreWeight"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeDistressScoreWeight = 1"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeDebtPressureScoreWeight = 1"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeDistressScoreWeightOrDefault"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeDebtPressureScoreWeightOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_distress_score_weight"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_debt_pressure_score_weight"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeDistressScoreWeightOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeDebtPressureScoreWeightOrDefault"));
        Assert.That(populationModule, Does.Contain("household.Distress * distressScoreWeight"));
        Assert.That(populationModule, Does.Contain("household.DebtPressure * debtPressureScoreWeight"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The pressure score-weight extraction must not add or duplicate the monthly household mobility runtime rule path.");

        int scoreStart = populationModule.IndexOf(
            "private static int ComputeMonthlyHouseholdMobilityRuntimeScore",
            StringComparison.Ordinal);
        int scoreEnd = populationModule.IndexOf(
            "private static int ResolveMonthlyHouseholdMobilityLivelihoodScoreWeight",
            scoreStart,
            StringComparison.Ordinal);
        Assert.That(scoreStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(scoreEnd, Is.GreaterThan(scoreStart));
        string scoreMethod = populationModule[scoreStart..scoreEnd];
        Assert.That(scoreMethod, Does.Not.Contain("+ household.Distress"));
        Assert.That(scoreMethod, Does.Not.Contain("+ household.DebtPressure"));

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultPressureScoreWeightsPreservePreviousScoreOrdering"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimePressureScoreWeightsFallBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeDistressScoreWeight"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeDebtPressureScoreWeight"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("PressureScoreExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeDistressScoreWeight",
                     "MonthlyRuntimeDebtPressureScoreWeight",
                     "GetMonthlyRuntimeDistressScoreWeightOrDefault",
                     "GetMonthlyRuntimeDebtPressureScoreWeightOrDefault",
                     "HouseholdMobilityPressureScoreInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "PressureScoreExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityPressureScoreState",
                     "HouseholdMobilityPressureScoreLedger",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_migration_status_threshold_extraction_v813_v820_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-migration-status-threshold-extraction-v813-v820.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V813-V820 Household Mobility Runtime Migration Status Threshold Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime migration-status extraction: v813-v820"));
        Assert.That(designAudit, Does.Contain("v813-v820 household mobility runtime migration-status extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime migration-status extraction v813-v820 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime migration-status extraction v813-v820 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime migration-status extraction v813-v820 note"));
        Assert.That(uiPresentation, Does.Contain("v813-v820 household mobility runtime migration-status extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime migration-status extraction v813-v820 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V813-V820 Household Mobility Runtime Migration Status Threshold Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Migration Status Threshold Extraction Through V820"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime migration-status extraction v813-v820 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime migration-status extraction v813-v820 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default status threshold remains `80`",
                     "No runtime behavior change under default rules-data",
                     "Other existing module paths continue to use the default threshold",
                     "No migration status retune",
                     "No migration-started event threshold retune",
                     "No candidate ceiling retune",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeMigrationStatusThreshold"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeMigrationStatusThreshold = 80"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeMigrationStatusThresholdOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_migration_status_threshold"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeMigrationStatusThresholdOrDefault"));
        Assert.That(populationModule, Does.Contain("ResolveMigrationStatus(household, migrationStatusThreshold)"));
        Assert.That(populationModule, Does.Contain("DefaultMonthlyRuntimeMigrationStatusThreshold"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The migration-status extraction must not add or duplicate the monthly household mobility runtime rule path.");

        int statusStart = populationModule.IndexOf(
            "private static bool ResolveMigrationStatus(PopulationHouseholdState household)",
            StringComparison.Ordinal);
        int statusEnd = populationModule.IndexOf(
            "private static bool TryApplyMonthlyLivelihoodDrift",
            statusStart,
            StringComparison.Ordinal);
        Assert.That(statusStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(statusEnd, Is.GreaterThan(statusStart));
        string statusMethods = populationModule[statusStart..statusEnd];
        Assert.That(statusMethods, Does.Not.Contain("MigrationRisk >= 80"));
        Assert.That(statusMethods, Does.Contain("MigrationRisk >= migrationStatusThreshold"));

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultMigrationStatusThresholdPreservesPreviousStatusBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationStatusThresholdFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeMigrationStatusThreshold"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("MigrationStatusExtractionState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeMigrationStatusThreshold",
                     "GetMonthlyRuntimeMigrationStatusThresholdOrDefault",
                     "HouseholdMobilityMigrationStatusInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "MigrationStatusExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityMigrationStatusState",
                     "HouseholdMobilityMigrationStatusLedger",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Household_mobility_runtime_migration_risk_clamp_extraction_v821_v828_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string socialStrata = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SOCIAL_STRATA_AND_PATHWAYS.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string fidelityModel = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION_FIDELITY_MODEL.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlan = File.ReadAllText(Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "active",
            "2026-05-03_household-mobility-runtime-risk-clamp-extraction-v821-v828.md"));
        string householdModuleTests = File.ReadAllText(Path.Combine(
            RepoRoot,
            "tests",
            "Zongzu.Modules.PopulationAndHouseholds.Tests",
            "PopulationAndHouseholdsModuleTests.cs"));
        string populationModule = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsModule.cs"));
        string rulesData = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationHouseholdMobilityRulesData.cs"));
        string populationState = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PopulationAndHouseholds",
            "PopulationAndHouseholdsState.cs"));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string applicationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Application")).Select(File.ReadAllText));
        string presentationSource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity"),
                Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels")).Select(File.ReadAllText));
        string unitySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(RepoRoot, "unity")).Select(File.ReadAllText));
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("V821-V828 Household Mobility Runtime Migration Risk Clamp Extraction"));
        Assert.That(socialStrata, Does.Contain("Current household mobility runtime migration-risk clamp extraction: v821-v828"));
        Assert.That(designAudit, Does.Contain("v821-v828 household mobility runtime migration-risk clamp extraction audit"));
        Assert.That(moduleBoundaries, Does.Contain("Household mobility runtime migration-risk clamp extraction v821-v828 boundary note"));
        Assert.That(integrationRules, Does.Contain("Household mobility runtime migration-risk clamp extraction v821-v828 integration note"));
        Assert.That(simulation, Does.Contain("Current household mobility runtime migration-risk clamp extraction v821-v828 note"));
        Assert.That(uiPresentation, Does.Contain("v821-v828 household mobility runtime migration-risk clamp extraction"));
        Assert.That(acceptance, Does.Contain("Household mobility runtime migration-risk clamp extraction v821-v828 acceptance"));
        Assert.That(fidelityModel, Does.Contain("V821-V828 Household Mobility Runtime Migration Risk Clamp Extraction"));
        Assert.That(skillMatrix, Does.Contain("Household Mobility Runtime Migration Risk Clamp Extraction Through V828"));
        Assert.That(schemaRules, Does.Contain("household mobility runtime migration-risk clamp extraction v821-v828 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current household mobility runtime migration-risk clamp extraction v821-v828 note"));

        foreach (string requiredPlanText in new[]
                 {
                     "Target schema/migration impact: none",
                     "owner-consumed rules-data extraction",
                     "default migration-risk clamp remains `0..100`",
                     "Runtime behavior under default rules-data: unchanged",
                     "Other module paths continue to use their existing default range behavior",
                     "No migration-risk retune",
                     "No risk-delta retune",
                     "No candidate floor or ceiling retune",
                     "No migration status threshold retune",
                     "No fanout widening",
                     "No second household mobility runtime rule",
                     "No rules-data loader",
                     "No rules-data file",
                     "No runtime plugin marketplace",
                     "No direct route-history",
                     "No household movement command",
                     "No migration economy",
                     "No class/status engine",
                     "No `PersonRegistry` expansion",
                     "Application/UI/Unity do not calculate household mobility outcomes",
                     "No long-run saturation tuning",
                     "No performance optimization claim",
                 })
        {
            Assert.That(execPlan, Does.Contain(requiredPlanText), requiredPlanText);
        }

        Assert.That(rulesData, Does.Contain("MonthlyRuntimeMigrationRiskClampFloor"));
        Assert.That(rulesData, Does.Contain("MonthlyRuntimeMigrationRiskClampCeiling"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeMigrationRiskClampFloor = 0"));
        Assert.That(rulesData, Does.Contain("DefaultMonthlyRuntimeMigrationRiskClampCeiling = 100"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeMigrationRiskClampFloorOrDefault"));
        Assert.That(rulesData, Does.Contain("GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_migration_risk_clamp_floor"));
        Assert.That(rulesData, Does.Contain("monthly_runtime_migration_risk_clamp_ceiling"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeMigrationRiskClampFloorOrDefault"));
        Assert.That(populationModule, Does.Contain("GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault"));
        Assert.That(populationModule, Does.Contain("migrationRiskClampFloor"));
        Assert.That(populationModule, Does.Contain("migrationRiskClampCeiling"));
        Assert.That(populationModule, Does.Contain("ModuleSchemaVersion => 3"));

        int runtimeStart = populationModule.IndexOf(
            "private bool ApplyMonthlyHouseholdMobilityRuntimeRule",
            StringComparison.Ordinal);
        int runtimeEnd = populationModule.IndexOf(
            "private static bool IsMonthlyHouseholdMobilityRuntimeCandidate",
            runtimeStart,
            StringComparison.Ordinal);
        Assert.That(runtimeStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(runtimeEnd, Is.GreaterThan(runtimeStart));
        string runtimeRule = populationModule[runtimeStart..runtimeEnd];
        Assert.That(runtimeRule, Does.Not.Contain("Math.Clamp(household.MigrationRisk + riskDelta, 0, 100)"));
        Assert.That(runtimeRule, Does.Contain("migrationRiskClampFloor"));
        Assert.That(runtimeRule, Does.Contain("migrationRiskClampCeiling"));
        Assert.That(
            Regex.Matches(populationModule, @"\bApplyMonthlyHouseholdMobilityRuntimeRule\s*\(").Count,
            Is.EqualTo(2),
            "The migration-risk clamp extraction must not add or duplicate the monthly household mobility runtime rule path.");

        Assert.That(
            householdModuleTests,
            Does.Contain("RunMonth_FirstMobilityRuntimeRuleDefaultMigrationRiskClampPreservesPreviousClampBehavior"));
        Assert.That(
            householdModuleTests,
            Does.Contain("PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationRiskClampFallsBackToDefault"));
        Assert.That(populationState, Does.Not.Contain("MonthlyRuntimeMigrationRiskClamp"));
        Assert.That(populationState, Does.Not.Contain("HouseholdMobility"));
        Assert.That(populationState, Does.Not.Contain("RouteHistory"));
        Assert.That(populationState, Does.Not.Contain("Cooldown"));
        Assert.That(populationState, Does.Not.Contain("RiskClampState"));
        Assert.That(populationState, Does.Not.Contain("ValidationLedger"));

        foreach (string authorityToken in new[]
                 {
                     "MonthlyRuntimeMigrationRiskClampFloor",
                     "MonthlyRuntimeMigrationRiskClampCeiling",
                     "GetMonthlyRuntimeMigrationRiskClampFloorOrDefault",
                     "GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault",
                     "HouseholdMobilityRiskClampInterpreter",
                     "HouseholdMobilityEventRouter",
                 })
        {
            Assert.That(applicationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(presentationSource, Does.Not.Contain(authorityToken), authorityToken);
            Assert.That(unitySource, Does.Not.Contain(authorityToken), authorityToken);
        }

        foreach (string personRegistryToken in new[]
                 {
                     "PopulationHouseholdMobilityRulesData",
                     "MonthlyRuntime",
                     "HouseholdMobilityRoute",
                     "CommonerStatus",
                     "SocialClass",
                     "RiskClampExtraction",
                 })
        {
            Assert.That(personRegistrySource, Does.Not.Contain(personRegistryToken), personRegistryToken);
        }

        foreach (string forbidden in new[]
                 {
                     "SecondHouseholdMobilityRuntimeRule",
                     "HouseholdMovementCommand",
                     "MoveHouseholdCommand",
                     "RelocateHouseholdCommand",
                     "RouteHistoryModel",
                     "HouseholdRouteHistory",
                     "MigrationEconomyEngine",
                     "CommonerStatusEngine",
                     "SocialClassEngine",
                     "HouseholdMobilityRiskClampState",
                     "HouseholdMobilityRiskClampLedger",
                     "MobilitySelectorWatermark",
                     "TargetCardinalityState",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "HouseholdMobilityEventRoutingLedger",
                     "HouseholdMobilityMigrationStartedSelectorState",
                     "HouseholdMobilityRulesDataLoader",
                     "HouseholdMobilityRulesDataFile",
                     "IRuntimeRulePlugin",
                     "RuntimePluginMarketplace",
                     "ArbitraryScriptRule",
                     "DynamicRuleAssembly",
                     "Assembly.Load(",
                     "DomainEvent.Summary.Split",
                     ".Summary.Split",
                     "ProjectionProseParser",
                     "ReceiptTextParser",
                     "PublicLifeLineParser",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMobility*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.HouseholdMovement*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.MigrationEconomy*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.RouteHistory*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.CommonerStatus*", SearchOption.TopDirectoryOnly), Is.Empty);
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.SocialClass*", SearchOption.TopDirectoryOnly), Is.Empty);
    }

    [Test]
    public void Regime_legitimacy_readback_v253_v260_must_stay_owner_laned_projection_only_and_schema_neutral()
    {
        string governanceSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Application",
            "PresentationReadModelBuilder",
            "PresentationReadModelBuilder.Governance.cs"));
        string publicLifeSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.PublicLifeAndRumor",
            "PublicLifeAndRumorModule",
            "PublicLifeAndRumorModule.cs"));
        string officeSource = File.ReadAllText(Path.Combine(
            SrcDir,
            "Zongzu.Modules.OfficeAndCareer",
            "OfficeAndCareerModule",
            "OfficeAndCareerModule.cs"));
        string unitySource = string.Join(Environment.NewLine, new[]
        {
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "DeskSandbox", "DeskSandboxShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "GovernanceShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity", "Adapters", "Office", "OfficeShellAdapter.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "DeskSandbox", "SettlementNodeViewModel.cs"),
            Path.Combine(SrcDir, "Zongzu.Presentation.Unity.ViewModels", "Office", "OfficeJurisdictionViewModel.cs"),
        }.Select(File.ReadAllText));
        string personRegistrySource = string.Join(Environment.NewLine,
            EnumerateSourceFiles(Path.Combine(SrcDir, "Zongzu.Modules.PersonRegistry")).Select(File.ReadAllText));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_regime-legitimacy-readback-v253-v260.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_regime-legitimacy-readback-v253-v260.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);

        int projectionStart = governanceSource.IndexOf("private static string BuildRegimeOfficeReadbackSummary", StringComparison.Ordinal);
        int projectionEnd = governanceSource.IndexOf("private static string BuildCanalRouteReadbackSummary", projectionStart, StringComparison.Ordinal);
        Assert.That(projectionStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(projectionEnd, Is.GreaterThan(projectionStart));
        string regimeProjectionMethod = governanceSource[projectionStart..projectionEnd];
        int publicLifeStart = publicLifeSource.IndexOf("private static void ApplyOfficeDefectionHeat", StringComparison.Ordinal);
        int publicLifeEnd = publicLifeSource.IndexOf("private static bool TryResolveEventSettlementId", publicLifeStart, StringComparison.Ordinal);
        Assert.That(publicLifeStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(publicLifeEnd, Is.GreaterThan(publicLifeStart));
        string officeDefectionPublicLifeMethod = publicLifeSource[publicLifeStart..publicLifeEnd];

        foreach (string token in new[]
                 {
                     "天命摇动读回",
                     "去就风险读回",
                     "官身承压姿态",
                     "公议向背读法",
                     "仍由Office/PublicLife分读",
                     "不是本户替朝廷修合法性",
                     "不是UI判定归附成败",
                 })
        {
            Assert.That(regimeProjectionMethod, Does.Contain(token));
            Assert.That(officeDefectionPublicLifeMethod, Does.Contain(token));
        }

        Assert.That(officeSource, Does.Contain("OfficeAndCareerEventNames.OfficeDefected"));
        Assert.That(officeSource, Does.Contain("DefectionProfile"));
        Assert.That(officeSource, Does.Contain("DomainEventMetadataKeys.DefectionRisk"));
        Assert.That(publicLifeSource, Does.Contain("ApplyOfficeDefectionHeat"));
        Assert.That(officeDefectionPublicLifeMethod, Does.Contain("ReadMetadataInt(domainEvent"));
        Assert.That(officeDefectionPublicLifeMethod, Does.Contain("DomainEventMetadataKeys.DefectionRisk"));
        Assert.That(regimeProjectionMethod, Does.Contain("OfficialDefectionRisk"));
        Assert.That(regimeProjectionMethod, Does.Contain("JurisdictionAuthoritySnapshot"));
        Assert.That(unitySource, Does.Contain("RegimeOfficeReadbackSummary"));

        foreach (string forbidden in new[]
                 {
                     "DomainEvent.Summary",
                     "domainEvent.Summary",
                     "LastAdministrativeTrace",
                     "LastPetitionOutcome",
                     "LastLocalResponseSummary",
                     "LastRefusalResponseSummary",
                     "WorldSettlementsState",
                     "GetMutableModuleState",
                     "PlayerCommandService",
                     "IssueModuleCommand",
                 })
        {
            Assert.That(regimeProjectionMethod, Does.Not.Contain(forbidden), forbidden);
            Assert.That(officeDefectionPublicLifeMethod, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(regimeProjectionMethod, Does.Not.Contain("OfficialNoticeLine"));
        Assert.That(regimeProjectionMethod, Does.Not.Contain("PrefectureDispatchLine"));

        foreach (string forbidden in new[]
                 {
                     "RegimeLedger",
                     "LegitimacyLedger",
                     "DefectionLedger",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "FactionAI",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                 })
        {
            Assert.That(governanceSource, Does.Not.Contain(forbidden), forbidden);
            Assert.That(publicLifeSource, Does.Not.Contain(forbidden), forbidden);
            Assert.That(officeSource, Does.Not.Contain(forbidden), forbidden);
            Assert.That(unitySource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(unitySource, Does.Not.Contain("DomainEventMetadataKeys"));
        Assert.That(unitySource, Does.Not.Contain("OfficeDefected"));
        Assert.That(unitySource, Does.Not.Contain("DefectionRisk"));
        Assert.That(personRegistrySource, Does.Not.Contain("RegimeLegitimacy"));
        Assert.That(personRegistrySource, Does.Not.Contain("OfficeDefected"));
        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Court*", SearchOption.TopDirectoryOnly), Is.Empty);

        Assert.That(topologyIndex, Does.Contain("Chain 9 Regime Legitimacy Readback - v253-v260"));
        Assert.That(acceptance, Does.Contain("Chain 9 regime legitimacy readback v253-v260 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V260"));
        Assert.That(schemaRules, Does.Contain("regime legitimacy readback v253-v260 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current regime legitimacy readback v253-v260 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("No full regime engine"));
    }

    [Test]
    public void Regime_legitimacy_readback_closeout_v261_v268_must_remain_audit_only_without_schema_or_authority_drift()
    {
        string topologyIndex = File.ReadAllText(Path.Combine(RepoRoot, "docs", "RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md"));
        string designAudit = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DESIGN_CODE_ALIGNMENT_AUDIT.md"));
        string moduleBoundaries = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_BOUNDARIES.md"));
        string integrationRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "MODULE_INTEGRATION_RULES.md"));
        string schemaRules = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SCHEMA_NAMESPACE_RULES.md"));
        string dataSchema = File.ReadAllText(Path.Combine(RepoRoot, "docs", "DATA_SCHEMA.md"));
        string simulation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "SIMULATION.md"));
        string uiPresentation = File.ReadAllText(Path.Combine(RepoRoot, "docs", "UI_AND_PRESENTATION.md"));
        string acceptance = File.ReadAllText(Path.Combine(RepoRoot, "docs", "ACCEPTANCE_TESTS.md"));
        string skillMatrix = File.ReadAllText(Path.Combine(RepoRoot, "docs", "CODEX_SKILL_RATIONALIZATION_MATRIX.md"));
        string execPlanPath = Path.Combine(
            RepoRoot,
            "docs",
            "exec-plans",
            "archive",
            "2026-04-28_regime-legitimacy-readback-closeout-v261-v268.md");
        if (!File.Exists(execPlanPath))
        {
            execPlanPath = Path.Combine(
                RepoRoot,
                "docs",
                "exec-plans",
                "active",
                "2026-04-28_regime-legitimacy-readback-closeout-v261-v268.md");
        }
        string execPlan = File.ReadAllText(execPlanPath);
        string productionSource = string.Join(Environment.NewLine, EnumerateSourceFiles(SrcDir).Select(File.ReadAllText));

        Assert.That(topologyIndex, Does.Contain("Regime Legitimacy Readback Closeout Audit - v261-v268"));
        Assert.That(topologyIndex, Does.Contain("Chain 9 first readback branch only"));
        Assert.That(designAudit, Does.Contain("v261-v268 regime legitimacy readback closeout audit"));
        Assert.That(moduleBoundaries, Does.Contain("Regime legitimacy readback closeout v261-v268 boundary note"));
        Assert.That(integrationRules, Does.Contain("Chain 9 v261-v268 regime legitimacy readback closeout integration note"));
        Assert.That(simulation, Does.Contain("Current regime legitimacy readback closeout v261-v268 note"));
        Assert.That(uiPresentation, Does.Contain("Regime legitimacy readback closeout v261-v268 UI note"));
        Assert.That(acceptance, Does.Contain("Chain 9 regime legitimacy readback closeout v261-v268 acceptance"));
        Assert.That(skillMatrix, Does.Contain("Skill Alignment Through V268"));

        Assert.That(schemaRules, Does.Contain("regime legitimacy readback closeout v261-v268 adds no persisted fields"));
        Assert.That(dataSchema, Does.Contain("Current regime legitimacy readback closeout v261-v268 note"));
        Assert.That(execPlan, Does.Contain("Target impact: none"));
        Assert.That(execPlan, Does.Contain("This pass must remain docs/tests only"));
        Assert.That(execPlan, Does.Contain("No production rule changes"));

        foreach (string token in new[]
                 {
                     "full regime engine",
                     "dynasty-cycle model",
                 })
        {
            Assert.That(topologyIndex, Does.Contain(token), token);
            Assert.That(designAudit, Does.Contain(token), token);
        }
        Assert.That(topologyIndex, Does.Contain("public allegiance simulation"));
        Assert.That(designAudit, Does.Contain("public-allegiance simulation"));
        Assert.That(designAudit, Does.Contain("durable regime SocialMemory residue"));

        foreach (string forbidden in new[]
                 {
                     "RegimeCloseoutLedger",
                     "RegimeRecognitionLedger",
                     "LegitimacyLedger",
                     "DefectionLedger",
                     "PublicAllegianceLedger",
                     "OwnerLaneLedger",
                     "CooldownLedger",
                     "SchedulerLedger",
                     "RegimeEngine",
                     "FactionAI",
                     "WorldManager",
                     "PersonManager",
                     "CharacterManager",
                     "GodController",
                     "v261-v268",
                 })
        {
            Assert.That(productionSource, Does.Not.Contain(forbidden), forbidden);
        }

        Assert.That(Directory.GetDirectories(SrcDir, "Zongzu.Modules.Court*", SearchOption.TopDirectoryOnly), Is.Empty);
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
