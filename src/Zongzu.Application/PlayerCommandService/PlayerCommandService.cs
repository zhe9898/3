using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    public PlayerCommandResult IssueIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(command);

        return command.CommandName switch
        {
            PlayerCommandNames.ArrangeMarriage
                or PlayerCommandNames.DesignateHeirPolicy
                or PlayerCommandNames.SupportNewbornCare
                or PlayerCommandNames.SetMourningOrder
                or PlayerCommandNames.SupportSeniorBranch
                or PlayerCommandNames.OrderFormalApology
                or PlayerCommandNames.PermitBranchSeparation
                or PlayerCommandNames.SuspendClanRelief
                or PlayerCommandNames.InviteClanEldersMediation
                or PlayerCommandNames.InviteClanEldersPubliclyBroker
                => IssueFamilyIntent(simulation, command),

            PlayerCommandNames.PetitionViaOfficeChannels
                or PlayerCommandNames.DeployAdministrativeLeverage
                or PlayerCommandNames.PostCountyNotice
                or PlayerCommandNames.DispatchRoadReport
                => IssueOfficeIntent(simulation, command),

            PlayerCommandNames.EscortRoadReport
                or PlayerCommandNames.FundLocalWatch
                or PlayerCommandNames.SuppressBanditry
                or PlayerCommandNames.NegotiateWithOutlaws
                or PlayerCommandNames.TolerateDisorder
                => IssueExpandedOrderIntent(simulation, command),

            PlayerCommandNames.DraftCampaignPlan
                or PlayerCommandNames.CommitMobilization
                or PlayerCommandNames.ProtectSupplyLine
                or PlayerCommandNames.WithdrawToBarracks
                => IssueWarfareIntent(simulation, command),

            _ => new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = string.Empty,
                SurfaceKey = string.Empty,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = command.CommandName,
                Summary = $"Unknown player command: {command.CommandName}.",
            },
        };
    }
}
