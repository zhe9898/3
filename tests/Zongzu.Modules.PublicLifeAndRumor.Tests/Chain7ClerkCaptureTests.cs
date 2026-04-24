using System.Linq;
using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.PublicLifeAndRumor;

namespace Zongzu.Modules.PublicLifeAndRumor.Tests;

[TestFixture]
public sealed class Chain7ClerkCaptureTests
{
    [Test]
    public void HandleEvents_ClerkCaptureDeepened_IncreasesStreetTalkHeat()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            SettlementTier = SettlementTier.CountySeat,
            StreetTalkHeat = 30,
            LastPublicTrace = "初始状态。",
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(19)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state,
            context,
            [
                new DomainEventRecord(
                    KnownModuleKeys.OfficeAndCareer,
                    OfficeAndCareerEventNames.ClerkCaptureDeepened,
                    "县主簿衙门书吏坐大，案牍渐被架空。",
                    "1"),
            ]));

        SettlementPublicLifeState settlement = state.Settlements.Single();
        Assert.That(settlement.StreetTalkHeat, Is.EqualTo(42));
        Assert.That(settlement.LastPublicTrace, Does.Contain("书吏坐大"));
        Assert.That(settlement.LastPublicTrace, Does.Contain("42"));
    }

    [Test]
    public void HandleEvents_ClerkCaptureDeepened_ClampsAtMax()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            SettlementTier = SettlementTier.CountySeat,
            StreetTalkHeat = 95,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(19)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state,
            context,
            [
                new DomainEventRecord(
                    KnownModuleKeys.OfficeAndCareer,
                    OfficeAndCareerEventNames.ClerkCaptureDeepened,
                    "书吏坐大。",
                    "1"),
            ]));

        Assert.That(state.Settlements.Single().StreetTalkHeat, Is.EqualTo(100));
    }

    [Test]
    public void HandleEvents_ClerkCaptureDeepened_UsesProfileMetadataForHeat()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            SettlementTier = SettlementTier.CountySeat,
            StreetTalkHeat = 30,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(19)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state,
            context,
            [
                new DomainEventRecord(
                    KnownModuleKeys.OfficeAndCareer,
                    OfficeAndCareerEventNames.ClerkCaptureDeepened,
                    "clerk capture profile",
                    "1",
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.ClerkCapturePressure] = "70",
                        [DomainEventMetadataKeys.ClerkCaptureBacklogPressure] = "18",
                        [DomainEventMetadataKeys.ClerkCaptureTaskPressure] = "15",
                        [DomainEventMetadataKeys.ClerkCaptureAuthorityBuffer] = "0",
                    }),
            ]));

        SettlementPublicLifeState settlement = state.Settlements.Single();
        Assert.That(settlement.StreetTalkHeat, Is.GreaterThan(42),
            "Profile metadata should let severe clerk capture heat the public surface more than the legacy +12 fallback.");
        Assert.That(settlement.LastPublicTrace, Does.Contain("捕获势70"));
    }

    [Test]
    public void HandleEvents_ClerkCaptureDeepened_OffScopeSettlement_DoesNotAffect()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            SettlementTier = SettlementTier.CountySeat,
            StreetTalkHeat = 30,
        });
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(2),
            SettlementName = "North Ford",
            SettlementTier = SettlementTier.MarketTown,
            StreetTalkHeat = 30,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(19)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state,
            context,
            [
                new DomainEventRecord(
                    KnownModuleKeys.OfficeAndCareer,
                    OfficeAndCareerEventNames.ClerkCaptureDeepened,
                    "书吏坐大。",
                    "1"),
            ]));

        SettlementPublicLifeState lanxi = state.Settlements.Single(s => s.SettlementId.Value == 1);
        SettlementPublicLifeState northFord = state.Settlements.Single(s => s.SettlementId.Value == 2);

        Assert.That(lanxi.StreetTalkHeat, Is.EqualTo(42));
        Assert.That(northFord.StreetTalkHeat, Is.EqualTo(30),
            "Off-scope settlement must not be affected by clerk capture deepened.");
    }

    [Test]
    public void HandleEvents_ClerkCaptureDeepened_InvalidEntityKey_NoOp()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            SettlementTier = SettlementTier.CountySeat,
            StreetTalkHeat = 30,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(19)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state,
            context,
            [
                new DomainEventRecord(
                    KnownModuleKeys.OfficeAndCareer,
                    OfficeAndCareerEventNames.ClerkCaptureDeepened,
                    "书吏坐大。",
                    "not-a-number"),
            ]));

        Assert.That(state.Settlements.Single().StreetTalkHeat, Is.EqualTo(30));
    }
}
