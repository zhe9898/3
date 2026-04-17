using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Persistence;

namespace Zongzu.Persistence.Tests;

[TestFixture]
public sealed class SaveRoundtripTests
{
    [Test]
    public void SaveCodec_RoundtripPreservesSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260418);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM0M1(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.WorldSettlements,
            }));
    }

    [Test]
    public void SaveCodec_RoundtripPreservesM2LiteSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260419);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM2(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));
    }
}
