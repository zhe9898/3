using Zongzu.Contracts;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed partial class OrderAndBanditryModule
{
    public override PlayerCommandResult HandleCommand(ModuleCommandHandlingScope<OrderAndBanditryState> scope)
    {
        return OrderAndBanditryCommandResolver.IssueIntent(new OrderAndBanditryCommandContext
        {
            State = scope.State,
            Command = scope.Command,
            OfficeQueries = scope.TryGetQuery<IOfficeAndCareerQueries>(),
        });
    }
}
