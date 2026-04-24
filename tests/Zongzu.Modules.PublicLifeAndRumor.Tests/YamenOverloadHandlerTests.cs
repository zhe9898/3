using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor.Tests;

[TestFixture]
public sealed class YamenOverloadHandlerTests
{
    [Test]
    public void YamenOverloaded_ForMatchedSettlement_DoesNotAffectOtherSettlements()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            StreetTalkHeat = 30,
        });
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(2),
            SettlementName = "North Ford",
            StreetTalkHeat = 20,
        });

        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state,
            context,
            [
                new DomainEventRecord(
                    KnownModuleKeys.OfficeAndCareer,
                    OfficeAndCareerEventNames.YamenOverloaded,
                    "Yamen overloaded at settlement 1.",
                    "1"),
            ]));

        SettlementPublicLifeState lanxi = state.Settlements.Single(s => s.SettlementId == new SettlementId(1));
        SettlementPublicLifeState northFord = state.Settlements.Single(s => s.SettlementId == new SettlementId(2));

        Assert.That(lanxi.StreetTalkHeat, Is.EqualTo(45));
        Assert.That(northFord.StreetTalkHeat, Is.EqualTo(20),
            "Off-scope settlement must not receive yamen-overload heat.");
    }
}
