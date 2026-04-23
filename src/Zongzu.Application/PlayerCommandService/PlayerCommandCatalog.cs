using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.WarfareCampaign;

namespace Zongzu.Application;

public sealed class PlayerCommandRoute
{
    private readonly Func<PlayerCommandRequest, string>? _disabledTargetLabelFactory;

    public PlayerCommandRoute(
        string commandName,
        string moduleKey,
        string surfaceKey,
        string label,
        string disabledSummary,
        Func<PlayerCommandRequest, string>? disabledTargetLabelFactory = null)
    {
        CommandName = commandName;
        ModuleKey = moduleKey;
        SurfaceKey = surfaceKey;
        Label = label;
        DisabledSummary = disabledSummary;
        _disabledTargetLabelFactory = disabledTargetLabelFactory;
    }

    public string CommandName { get; }

    public string ModuleKey { get; }

    public string SurfaceKey { get; }

    public string Label { get; }

    public string DisabledSummary { get; }

    public string BuildDisabledTargetLabel(PlayerCommandRequest command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return _disabledTargetLabelFactory?.Invoke(command) ?? string.Empty;
    }
}

public static class PlayerCommandCatalog
{
    private static readonly PlayerCommandRoute[] Routes =
    [
        new(
            PlayerCommandNames.ArrangeMarriage,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.ArrangeMarriage),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.DesignateHeirPolicy,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.DesignateHeirPolicy),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.SupportNewbornCare,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.SupportNewbornCare),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.SetMourningOrder,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.SetMourningOrder),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.SupportSeniorBranch,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.SupportSeniorBranch),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.OrderFormalApology,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.OrderFormalApology),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.PermitBranchSeparation,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.PermitBranchSeparation),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.SuspendClanRelief,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.SuspendClanRelief),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.InviteClanEldersMediation,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.Family,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.InviteClanEldersMediation),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),
        new(
            PlayerCommandNames.InviteClanEldersPubliclyBroker,
            KnownModuleKeys.FamilyCore,
            PlayerCommandSurfaceKeys.PublicLife,
            FamilyCoreCommandResolver.DetermineFamilyCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b97\u623f\u88c1\u65ad\u3002"),

        new(
            PlayerCommandNames.PetitionViaOfficeChannels,
            KnownModuleKeys.OfficeAndCareer,
            PlayerCommandSurfaceKeys.Office,
            OfficeAndCareerCommandResolver.DetermineOfficeCommandLabel(PlayerCommandNames.PetitionViaOfficeChannels),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b98\u7f72\u6743\u67c4\u3002"),
        new(
            PlayerCommandNames.DeployAdministrativeLeverage,
            KnownModuleKeys.OfficeAndCareer,
            PlayerCommandSurfaceKeys.Office,
            OfficeAndCareerCommandResolver.DetermineOfficeCommandLabel(PlayerCommandNames.DeployAdministrativeLeverage),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b98\u7f72\u6743\u67c4\u3002"),
        new(
            PlayerCommandNames.PostCountyNotice,
            KnownModuleKeys.OfficeAndCareer,
            PlayerCommandSurfaceKeys.PublicLife,
            OfficeAndCareerCommandResolver.DeterminePublicLifeOfficeCommandLabel(PlayerCommandNames.PostCountyNotice),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b98\u7f72\u6743\u67c4\u3002"),
        new(
            PlayerCommandNames.DispatchRoadReport,
            KnownModuleKeys.OfficeAndCareer,
            PlayerCommandSurfaceKeys.PublicLife,
            OfficeAndCareerCommandResolver.DeterminePublicLifeOfficeCommandLabel(PlayerCommandNames.DispatchRoadReport),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5b98\u7f72\u6743\u67c4\u3002"),

        new(
            PlayerCommandNames.EscortRoadReport,
            KnownModuleKeys.OrderAndBanditry,
            PlayerCommandSurfaceKeys.PublicLife,
            OrderAndBanditryCommandResolver.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5730\u65b9\u6cbb\u5b89\u4e0e\u62a4\u8def\u3002"),
        new(
            PlayerCommandNames.FundLocalWatch,
            KnownModuleKeys.OrderAndBanditry,
            PlayerCommandSurfaceKeys.PublicLife,
            OrderAndBanditryCommandResolver.DeterminePublicLifeCommandLabel(PlayerCommandNames.FundLocalWatch),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5730\u65b9\u6cbb\u5b89\u4e0e\u62a4\u8def\u3002"),
        new(
            PlayerCommandNames.SuppressBanditry,
            KnownModuleKeys.OrderAndBanditry,
            PlayerCommandSurfaceKeys.PublicLife,
            OrderAndBanditryCommandResolver.DeterminePublicLifeCommandLabel(PlayerCommandNames.SuppressBanditry),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5730\u65b9\u6cbb\u5b89\u4e0e\u62a4\u8def\u3002"),
        new(
            PlayerCommandNames.NegotiateWithOutlaws,
            KnownModuleKeys.OrderAndBanditry,
            PlayerCommandSurfaceKeys.PublicLife,
            OrderAndBanditryCommandResolver.DeterminePublicLifeCommandLabel(PlayerCommandNames.NegotiateWithOutlaws),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5730\u65b9\u6cbb\u5b89\u4e0e\u62a4\u8def\u3002"),
        new(
            PlayerCommandNames.TolerateDisorder,
            KnownModuleKeys.OrderAndBanditry,
            PlayerCommandSurfaceKeys.PublicLife,
            OrderAndBanditryCommandResolver.DeterminePublicLifeCommandLabel(PlayerCommandNames.TolerateDisorder),
            "\u5f53\u524d\u5b58\u6863\u672a\u542f\u7528\u5730\u65b9\u6cbb\u5b89\u4e0e\u62a4\u8def\u3002"),

        new(
            PlayerCommandNames.DraftCampaignPlan,
            KnownModuleKeys.WarfareCampaign,
            PlayerCommandSurfaceKeys.Warfare,
            WarfareCampaignDescriptors.DetermineDirectiveLabel(PlayerCommandNames.DraftCampaignPlan),
            "\u5f53\u524d\u5b58\u6863\u5e76\u672a\u542f\u7528\u519b\u52a1\u6c99\u76d8\u3002",
            static command => $"\u636e\u70b9 {command.SettlementId.Value}"),
        new(
            PlayerCommandNames.CommitMobilization,
            KnownModuleKeys.WarfareCampaign,
            PlayerCommandSurfaceKeys.Warfare,
            WarfareCampaignDescriptors.DetermineDirectiveLabel(PlayerCommandNames.CommitMobilization),
            "\u5f53\u524d\u5b58\u6863\u5e76\u672a\u542f\u7528\u519b\u52a1\u6c99\u76d8\u3002",
            static command => $"\u636e\u70b9 {command.SettlementId.Value}"),
        new(
            PlayerCommandNames.ProtectSupplyLine,
            KnownModuleKeys.WarfareCampaign,
            PlayerCommandSurfaceKeys.Warfare,
            WarfareCampaignDescriptors.DetermineDirectiveLabel(PlayerCommandNames.ProtectSupplyLine),
            "\u5f53\u524d\u5b58\u6863\u5e76\u672a\u542f\u7528\u519b\u52a1\u6c99\u76d8\u3002",
            static command => $"\u636e\u70b9 {command.SettlementId.Value}"),
        new(
            PlayerCommandNames.WithdrawToBarracks,
            KnownModuleKeys.WarfareCampaign,
            PlayerCommandSurfaceKeys.Warfare,
            WarfareCampaignDescriptors.DetermineDirectiveLabel(PlayerCommandNames.WithdrawToBarracks),
            "\u5f53\u524d\u5b58\u6863\u5e76\u672a\u542f\u7528\u519b\u52a1\u6c99\u76d8\u3002",
            static command => $"\u636e\u70b9 {command.SettlementId.Value}"),
    ];

    private static readonly IReadOnlyDictionary<string, PlayerCommandRoute> RoutesByCommand =
        Routes.ToDictionary(static route => route.CommandName, StringComparer.Ordinal);

    public static IReadOnlyList<PlayerCommandRoute> All => Routes;

    public static PlayerCommandRoute GetRequired(string commandName)
    {
        return TryGet(commandName, out PlayerCommandRoute? route)
            ? route
            : throw new InvalidOperationException($"Unknown player command route: {commandName}.");
    }

    public static bool TryGet(string commandName, [NotNullWhen(true)] out PlayerCommandRoute? route)
    {
        ArgumentException.ThrowIfNullOrEmpty(commandName);
        return RoutesByCommand.TryGetValue(commandName, out route);
    }
}
