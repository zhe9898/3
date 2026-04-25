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
        });

        if (result.Accepted)
        {
            RebuildSettlementSummaries(scope.State);
        }

        return result;
    }
}
