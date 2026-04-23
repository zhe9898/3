using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private PlayerCommandResult IssueWarfareIntent(GameSimulation simulation, PlayerCommandRequest command)

    {

        WarfareCampaignIntentResult warfareResult = _warfareCampaignCommandService.IssueIntent(

            simulation,

            new WarfareCampaignIntentCommand

            {

                SettlementId = command.SettlementId,

                CommandName = command.CommandName,

            });


        return new PlayerCommandResult

        {

            Accepted = warfareResult.Accepted,

            ModuleKey = KnownModuleKeys.WarfareCampaign,

            SurfaceKey = PlayerCommandSurfaceKeys.Warfare,

            SettlementId = command.SettlementId,

            ClanId = command.ClanId,

            CommandName = command.CommandName,

            Label = warfareResult.DirectiveLabel,

            Summary = warfareResult.Summary,

            TargetLabel = $"据点 {command.SettlementId.Value}",

        };

    }


}
