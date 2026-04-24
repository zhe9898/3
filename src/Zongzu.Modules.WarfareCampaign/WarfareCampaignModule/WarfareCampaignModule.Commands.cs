using Zongzu.Contracts;

namespace Zongzu.Modules.WarfareCampaign;

public sealed partial class WarfareCampaignModule
{
    public override PlayerCommandResult HandleCommand(ModuleCommandHandlingScope<WarfareCampaignState> scope)
    {
        return WarfareCampaignCommandResolver.IssueIntent(new WarfareCampaignCommandContext
        {
            State = scope.State,
            Command = scope.Command,
        });
    }
}
