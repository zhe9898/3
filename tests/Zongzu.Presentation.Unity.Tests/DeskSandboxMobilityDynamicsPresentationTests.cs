using System.Linq;
using NUnit.Framework;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

[TestFixture]
public sealed class DeskSandboxMobilityDynamicsPresentationTests
{
    [Test]
    public void DeskSandbox_CopiesProjectedHouseholdMobilityDynamicsWithoutComputingRules()
    {
        PresentationReadModelBundle bundle = new()
        {
            Settlements =
            [
                new SettlementSnapshot
                {
                    Id = new SettlementId(1),
                    Name = "Lanxi",
                    Security = 42,
                    Prosperity = 47,
                },
            ],
            PopulationSettlements =
            [
                new PopulationSettlementSnapshot
                {
                    SettlementId = new SettlementId(1),
                    CommonerDistress = 76,
                    LaborSupply = 38,
                    MigrationPressure = 72,
                },
            ],
            HouseholdSocialPressures =
            [
                new HouseholdSocialPressureSnapshot
                {
                    HouseholdId = new HouseholdId(7),
                    HouseholdName = "Li household",
                    SettlementId = new SettlementId(1),
                    PressureScore = 83,
                    MobilityDynamicsExplanationSummary =
                        "Household mobility dynamics: Li household reads dimensions=Mobility/DebtAndSubsistence; PopulationAndHouseholds owns household dynamics, far summary stays pooled, no PersonRegistry status authority.",
                    MobilityDynamicsDimensionKeys =
                    [
                        HouseholdSocialPressureSignalKeys.Mobility,
                        HouseholdSocialPressureSignalKeys.DebtAndSubsistence,
                    ],
                    Signals =
                    [
                        new HouseholdSocialPressureSignalSnapshot
                        {
                            SignalKey = HouseholdSocialPressureSignalKeys.Mobility,
                            Label = "mobility",
                            Score = 80,
                            Summary = "projected mobility signal",
                            SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
                        },
                    ],
                    SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel node = shell.DeskSandbox.Settlements.Single();

        Assert.That(node.HouseholdMobilityDynamicsSummary, Does.Contain("Household mobility dynamics"));
        Assert.That(node.HouseholdMobilityDynamicsSummary, Does.Contain("PopulationAndHouseholds owns household dynamics"));
        Assert.That(node.HouseholdMobilityDynamicsSummary, Does.Contain("far summary stays pooled"));
        Assert.That(node.HouseholdMobilityDynamicsSummary, Does.Contain("no PersonRegistry status authority"));
    }
}
