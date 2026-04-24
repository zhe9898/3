using Zongzu.Contracts;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule
{
    public override PlayerCommandResult HandleCommand(ModuleCommandHandlingScope<OfficeAndCareerState> scope)
    {
        return OfficeAndCareerCommandResolver.IssueIntent(new OfficeAndCareerCommandContext
        {
            State = scope.State,
            Command = scope.Command,
        });
    }
}
