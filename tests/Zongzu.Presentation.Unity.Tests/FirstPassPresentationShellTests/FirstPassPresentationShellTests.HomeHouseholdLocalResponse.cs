using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

public sealed partial class FirstPassPresentationShellTests
{
    [Test]
    public void Compose_ProjectsHomeHouseholdLocalResponsePublicLifeFieldsWithoutShellAuthority()
    {
        SettlementId settlementId = new(1);
        PresentationReadModelBundle bundle = new()
        {
            CurrentDate = new GameDate(2026, 4),
            Settlements =
            [
                new SettlementSnapshot
                {
                    Id = settlementId,
                    Name = "兰泽县",
                    Security = 52,
                    Prosperity = 48,
                },
            ],
            PlayerCommands = new PlayerCommandSurfaceSnapshot
            {
                Affordances =
                [
                    new PlayerCommandAffordanceSnapshot
                    {
                        ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
                        SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                        SettlementId = settlementId,
                        CommandName = PlayerCommandNames.RestrictNightTravel,
                        Label = "暂缩夜行",
                        Summary = "张家户暂缩夜行。",
                        AvailabilitySummary = "本户迁徙之念72，巡防后账46。 本户底色：债压已高，丁力偏薄。 旧账记忆：人情23。 回应承受线：丁力偏薄，暂缉夜行可做，但会挤压口粮人手。",
                        LeverageSummary = "本户只动自家夜行、脚程与临时避险。",
                        CostSummary = "代价：丁力会被收紧。 承受线代价：会吃丁力，若旧账再硬，容易转成吃紧后账。",
                        ReadbackSummary = "下月看迁徙之念、丁力和后账。 承受线读回：看夜路是否缓住迁徙之念，以及丁力是否被压过线。 社会记忆读回：人情23，本户后账已缓。",
                        TargetLabel = "张家户",
                        IsEnabled = true,
                    },
                ],
                Receipts =
                [
                    new PlayerCommandReceiptSnapshot
                    {
                        ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
                        SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                        SettlementId = settlementId,
                        CommandName = PlayerCommandNames.RestrictNightTravel,
                        Label = "暂缩夜行",
                        Summary = "张家户暂缩夜行，迁徙之念缓下。",
                        OutcomeSummary = "本户已缓",
                        LeverageSummary = "本户回应只结算自家劳力、债压、民困与迁徙险。",
                        CostSummary = "本户余账：民困58，债压49，丁力37，迁徙之念62。 本户底色：迁徙之念仍在。 承受线代价：会吃丁力，若旧账再硬，容易转成吃紧后账。",
                        ReadbackSummary = "张家户：后账已从本户脚程上缓下。 承受线读回：看夜路是否缓住迁徙之念，以及丁力是否被压过线。 社会记忆读回：人情23，本户后账已缓。",
                        TargetLabel = "张家户",
                    },
                ],
            },
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel node = shell.DeskSandbox.Settlements.Single();

        CommandAffordanceViewModel affordance = node.PublicLifeCommandAffordances.Single();
        Assert.That(affordance.CommandName, Is.EqualTo(PlayerCommandNames.RestrictNightTravel));
        Assert.That(affordance.Label, Is.EqualTo("暂缩夜行"));
        Assert.That(affordance.TargetLabel, Is.EqualTo("张家户"));
        Assert.That(affordance.AvailabilitySummary, Does.Contain("本户底色"));
        Assert.That(affordance.AvailabilitySummary, Does.Contain("债压已高"));
        Assert.That(affordance.AvailabilitySummary, Does.Contain("旧账记忆"));
        Assert.That(affordance.AvailabilitySummary, Does.Contain("回应承受线"));
        Assert.That(affordance.CostSummary, Does.Contain("丁力"));
        Assert.That(affordance.CostSummary, Does.Contain("承受线代价"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("下月看"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("承受线读回"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("社会记忆读回"));
        Assert.That(affordance.IsEnabled, Is.True);

        CommandReceiptViewModel receipt = node.PublicLifeRecentReceipts.Single();
        Assert.That(receipt.CommandName, Is.EqualTo(PlayerCommandNames.RestrictNightTravel));
        Assert.That(receipt.OutcomeSummary, Is.EqualTo("本户已缓"));
        Assert.That(receipt.CostSummary, Does.Contain("迁徙之念"));
        Assert.That(receipt.CostSummary, Does.Contain("本户底色"));
        Assert.That(receipt.CostSummary, Does.Contain("承受线代价"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("张家户"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("承受线读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("社会记忆读回"));
    }
}
