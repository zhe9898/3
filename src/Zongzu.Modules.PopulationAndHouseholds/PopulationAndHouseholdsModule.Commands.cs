using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    public override PlayerCommandResult HandleCommand(ModuleCommandHandlingScope<PopulationAndHouseholdsState> scope)
    {
        PlayerCommandResult result = PopulationAndHouseholdsCommandResolver.IssueIntent(new PopulationAndHouseholdsCommandContext
        {
            State = scope.State,
            Command = scope.Command,
            SocialMemoryQueries = scope.TryGetQuery<ISocialMemoryAndRelationsQueries>(),
        });

        if (result.Accepted)
        {
            SynchronizeMembershipLivelihoodsAndActivities(scope.State);
            RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
        }

        return result;
    }
}
