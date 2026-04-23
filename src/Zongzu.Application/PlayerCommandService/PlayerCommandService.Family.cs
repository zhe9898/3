using System;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private static PlayerCommandResult IssueFamilyIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            return BuildDisabledFamilyResult(command, "当前存档未启用宗房裁断。");
        }

        QueryRegistry queries = BuildQueries(simulation);
        PlayerCommandResult result = FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = simulation.GetMutableModuleState<FamilyCoreState>(KnownModuleKeys.FamilyCore),
            Command = command,
            CurrentDate = simulation.CurrentDate,
            PersonRegistryQueries = TryGetQuery<IPersonRegistryQueries>(queries),
            SocialMemoryQueries = TryGetQuery<ISocialMemoryAndRelationsQueries>(queries),
        });

        if (result.Accepted)
        {
            simulation.RefreshReplayHash();
        }

        return result;
    }

    internal static string DetermineFamilyCommandLabel(string commandName)
    {
        return FamilyCoreCommandResolver.DetermineFamilyCommandLabel(commandName);
    }

    private static PlayerCommandResult BuildDisabledFamilyResult(PlayerCommandRequest command, string summary)
    {
        bool isPublicLife = string.Equals(command.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal);
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = isPublicLife ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Family,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = isPublicLife ? DeterminePublicLifeCommandLabel(command.CommandName) : DetermineFamilyCommandLabel(command.CommandName),
            Summary = summary,
        };
    }

    private static TQuery? TryGetQuery<TQuery>(QueryRegistry queries)
        where TQuery : class
    {
        try
        {
            return queries.GetRequired<TQuery>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
