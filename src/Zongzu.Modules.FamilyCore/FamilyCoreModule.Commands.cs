using Zongzu.Contracts;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule
{
    public override PlayerCommandResult HandleCommand(ModuleCommandHandlingScope<FamilyCoreState> scope)
    {
        return FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = scope.State,
            Command = scope.Command,
            CurrentDate = scope.Context.CurrentDate,
            PersonRegistryQueries = scope.TryGetQuery<IPersonRegistryQueries>(),
            SocialMemoryQueries = scope.TryGetQuery<ISocialMemoryAndRelationsQueries>(),
            OrderQueries = scope.TryGetQuery<IOrderAndBanditryQueries>(),
        });
    }
}
