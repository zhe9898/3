using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Presentation.Unity;

Console.OutputEncoding = Encoding.UTF8;

long seed = 20260419;
int warmupMonths = 2;
int tenYearYears = 10;
string outputPath = Path.GetFullPath(
    Path.Combine(Directory.GetCurrentDirectory(), "content", "generated", "mvp-family-lifecycle-preview.md"));
string tenYearOutputPath = Path.GetFullPath(
    Path.Combine(Directory.GetCurrentDirectory(), "content", "generated", "mvp-family-lifecycle-ten-year-preview.md"));

for (int index = 0; index < args.Length; index += 1)
{
    switch (args[index])
    {
        case "--seed" when index + 1 < args.Length && long.TryParse(args[index + 1], out long parsedSeed):
            seed = parsedSeed;
            index += 1;
            break;
        case "--warmup-months" when index + 1 < args.Length && int.TryParse(args[index + 1], out int parsedMonths):
            warmupMonths = parsedMonths;
            index += 1;
            break;
        case "--years" when index + 1 < args.Length && int.TryParse(args[index + 1], out int parsedYears):
            tenYearYears = parsedYears;
            index += 1;
            break;
        case "--output" when index + 1 < args.Length:
            outputPath = Path.GetFullPath(args[index + 1], Directory.GetCurrentDirectory());
            index += 1;
            break;
        case "--ten-year-output" when index + 1 < args.Length:
            tenYearOutputPath = Path.GetFullPath(args[index + 1], Directory.GetCurrentDirectory());
            index += 1;
            break;
    }
}

MvpFamilyLifecyclePreviewScenario scenario = new();
MvpFamilyLifecyclePreviewResult preview = scenario.Build(seed, warmupMonths);
MvpFamilyLifecycleTenYearPreviewResult tenYearPreview = scenario.BuildTenYear(seed, tenYearYears);

string markdown = RenderPreviewMarkdown(preview);
string tenYearMarkdown = RenderTenYearPreviewMarkdown(tenYearPreview);

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(tenYearOutputPath)!);

File.WriteAllText(outputPath, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
File.WriteAllText(tenYearOutputPath, tenYearMarkdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

Console.WriteLine(markdown);
Console.WriteLine();
Console.WriteLine(tenYearMarkdown);
Console.WriteLine();
Console.WriteLine($"已生成预览：{outputPath}");
Console.WriteLine($"已生成十年回合：{tenYearOutputPath}");

static string RenderPreviewMarkdown(MvpFamilyLifecyclePreviewResult preview)
{
    PresentationShellViewModel beforeShell = FirstPassPresentationShell.Compose(preview.BeforeBundle);
    PresentationShellViewModel afterCommandShell = FirstPassPresentationShell.Compose(preview.AfterCommandBundle);
    PresentationShellViewModel afterAdvanceShell = FirstPassPresentationShell.Compose(preview.AfterAdvanceBundle);

    StringBuilder builder = new();
    builder.AppendLine("# MVP 家族生命周期预览");
    builder.AppendLine();
    builder.AppendLine($"- 起盘 seed：`{preview.Seed}`");
    builder.AppendLine($"- 预热月份：`{preview.WarmupMonths}`");
    builder.AppendLine($"- 选中的家内处置：`{preview.SelectedAffordance.Label}`");
    builder.AppendLine($"- 目标宗房：`{preview.SelectedAffordance.TargetLabel}`");
    builder.AppendLine();
    builder.AppendLine("这份预览按默认 MVP 路径起盘：先让门内压力浮出来，再下一道有边界的家内命令，最后再推进一个月看余波。");
    builder.AppendLine();

    AppendShellSection(builder, "一、起盘", preview.BeforeBundle, beforeShell);
    AppendCommandResult(builder, preview.CommandResult);
    AppendShellSection(builder, "二、下令后", preview.AfterCommandBundle, afterCommandShell);
    AppendShellSection(builder, "三、再过一月", preview.AfterAdvanceBundle, afterAdvanceShell);

    return builder.ToString();
}

static string RenderTenYearPreviewMarkdown(MvpFamilyLifecycleTenYearPreviewResult preview)
{
    StringBuilder builder = new();
    List<string> auditIssues = new();

    builder.AppendLine("# MVP 十年家内回合预览");
    builder.AppendLine();
    builder.AppendLine($"- 起盘 seed：`{preview.Seed}`");
    builder.AppendLine($"- 推演年数：`{preview.Years}`");
    builder.AppendLine($"- 已下家内命令：`{preview.IssuedCommands.Count}`");
    builder.AppendLine();
    builder.AppendLine("这份回合稿沿默认 MVP 家族生命周期路径逐月推进，每年截一帧，核对堂上摘要、族内摘要与通知后续是否仍指向同一类家内下一步。");
    builder.AppendLine();

    AppendTenYearCheckpoint(
        builder,
        "起盘",
        0,
        preview.InitialBundle,
        FirstPassPresentationShell.Compose(preview.InitialBundle),
        null,
        null,
        auditIssues);

    foreach (MvpFamilyLifecycleTenYearCheckpoint checkpoint in preview.YearlyCheckpoints)
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(checkpoint.AfterAdvanceBundle);
        AppendTenYearCheckpoint(
            builder,
            $"第{checkpoint.YearNumber}年末",
            checkpoint.YearNumber,
            checkpoint.AfterAdvanceBundle,
            shell,
            checkpoint.SelectedAffordance,
            checkpoint.CommandResult,
            auditIssues);
    }

    builder.AppendLine("## 口径核对");
    builder.AppendLine();
    if (auditIssues.Count == 0)
    {
        builder.AppendLine("- 十年回合内，堂上摘要、族内摘要与通知后续始终还能对回同一条家内下一步。");
    }
    else
    {
        foreach (string issue in auditIssues)
        {
            builder.AppendLine($"- {issue}");
        }
    }

    builder.AppendLine();
    builder.AppendLine("## 若后面要接你自己的 UI");
    builder.AppendLine();
    builder.AppendLine("- 直接吃 `PresentationReadModelBundle`，现有 `FirstPassPresentationShell` 可以继续当一层只读壳。");
    builder.AppendLine("- 家内命令优先从 `PlayerCommands.Affordances` 里选，不要在 UI 自己推规则。");
    builder.AppendLine("- 通知、家内摘要、回执尽量保留 source wording，不要先揉成单一大摘要。");

    return builder.ToString();
}

static void AppendCommandResult(StringBuilder builder, PlayerCommandResult result)
{
    builder.AppendLine("## 命令回执");
    builder.AppendLine();
    builder.AppendLine($"- 命令：`{result.Label}`");
    builder.AppendLine($"- 结果：{(result.Accepted ? "已准" : "未准")}");
    builder.AppendLine($"- 摘要：{result.Summary}");
    builder.AppendLine();
}

static void AppendShellSection(
    StringBuilder builder,
    string heading,
    PresentationReadModelBundle bundle,
    PresentationShellViewModel shell)
{
    builder.AppendLine($"## {heading}");
    builder.AppendLine();
    builder.AppendLine($"- 月份：`{bundle.CurrentDate}`");
    builder.AppendLine($"- 堂上家内摘要：{shell.GreatHall.FamilySummary}");

    if (!string.IsNullOrWhiteSpace(shell.GreatHall.LeadNoticeTitle))
    {
        builder.AppendLine($"- 堂上首条：{shell.GreatHall.LeadNoticeTitle}");
    }

    if (!string.IsNullOrWhiteSpace(shell.GreatHall.LeadNoticeGuidance))
    {
        builder.AppendLine($"- 堂上后续：{shell.GreatHall.LeadNoticeGuidance}");
    }

    if (!string.IsNullOrWhiteSpace(shell.GreatHall.PublicLifeSummary))
    {
        builder.AppendLine($"- 案头外缘：{shell.GreatHall.PublicLifeSummary}");
    }

    builder.AppendLine();
    builder.AppendLine("### 族内");
    builder.AppendLine();
    builder.AppendLine($"- 族议摘要：{shell.FamilyCouncil.Summary}");

    FamilyConflictTileViewModel? clan = shell.FamilyCouncil.Clans.FirstOrDefault();
    if (clan is not null)
    {
        builder.AppendLine($"- 宗房：`{clan.ClanName}`");
        builder.AppendLine($"- 生命周期：{clan.LifecycleSummary}");
        builder.AppendLine($"- 旧议余波：{clan.LastOrderSummary}");
    }

    AppendReceipts(builder, shell.FamilyCouncil.RecentReceipts, "最近回执");

    SettlementNodeViewModel? settlement = shell.DeskSandbox.Settlements.FirstOrDefault();
    if (settlement is not null)
    {
        builder.AppendLine();
        builder.AppendLine("### 案头节点");
        builder.AppendLine();
        builder.AppendLine($"- 节点：`{settlement.SettlementName}`");

        if (!string.IsNullOrWhiteSpace(settlement.PublicLifeSummary))
        {
            builder.AppendLine($"- 里门风色：{settlement.PublicLifeSummary}");
        }

        if (!string.IsNullOrWhiteSpace(settlement.GovernanceSummary))
        {
            builder.AppendLine($"- 官署牍报：{settlement.GovernanceSummary}");
        }

        if (!string.IsNullOrWhiteSpace(settlement.CampaignSummary))
        {
            builder.AppendLine($"- 军务沙盘：{settlement.CampaignSummary}");
        }

        if (!string.IsNullOrWhiteSpace(settlement.AftermathSummary))
        {
            builder.AppendLine($"- 战后案牍：{settlement.AftermathSummary}");
        }
    }

    if (!string.IsNullOrWhiteSpace(shell.Office.Summary))
    {
        builder.AppendLine();
        builder.AppendLine("### 官署");
        builder.AppendLine();
        builder.AppendLine($"- {shell.Office.Summary}");
    }

    if (!string.IsNullOrWhiteSpace(shell.Warfare.Summary))
    {
        builder.AppendLine();
        builder.AppendLine("### 军务");
        builder.AppendLine();
        builder.AppendLine($"- {shell.Warfare.Summary}");

        CampaignBoardViewModel? board = shell.Warfare.CampaignBoards.FirstOrDefault();
        if (board is not null)
        {
            builder.AppendLine($"- 桌面主案：{board.BoardAtmosphereSummary}");
            builder.AppendLine($"- 路线提要：{board.MarkerSummary}");
        }
    }

    builder.AppendLine();
    builder.AppendLine("### 通知");
    builder.AppendLine();

    IReadOnlyList<NotificationItemViewModel> notices = shell.NotificationCenter.Items.Take(3).ToArray();
    if (notices.Count == 0)
    {
        builder.AppendLine("- 暂无新告。");
        builder.AppendLine();
        return;
    }

    foreach (NotificationItemViewModel notice in notices)
    {
        builder.AppendLine($"1. **{notice.Title}**：{notice.Summary}");
        if (!string.IsNullOrWhiteSpace(notice.WhyItHappened))
        {
            builder.AppendLine($"   缘由：{notice.WhyItHappened}");
        }

        if (!string.IsNullOrWhiteSpace(notice.WhatNext))
        {
            builder.AppendLine($"   后续：{notice.WhatNext}");
        }
    }

    builder.AppendLine();
}

static void AppendTenYearCheckpoint(
    StringBuilder builder,
    string heading,
    int yearNumber,
    PresentationReadModelBundle bundle,
    PresentationShellViewModel shell,
    PlayerCommandAffordanceSnapshot? selectedAffordance,
    PlayerCommandResult? commandResult,
    ICollection<string> auditIssues)
{
    builder.AppendLine($"## {heading}");
    builder.AppendLine();
    builder.AppendLine($"- 月份：`{bundle.CurrentDate}`");
    builder.AppendLine($"- 堂上摘要：{shell.GreatHall.FamilySummary}");
    builder.AppendLine($"- 族议摘要：{shell.FamilyCouncil.Summary}");

    FamilyConflictTileViewModel? clan = shell.FamilyCouncil.Clans.FirstOrDefault();
    if (clan is not null)
    {
        builder.AppendLine($"- 宗房生命周期：{clan.LifecycleSummary}");
    }

    if (selectedAffordance is not null)
    {
        builder.AppendLine($"- 本年主处置：`{selectedAffordance.Label}` -> `{selectedAffordance.TargetLabel}`");
    }

    if (commandResult is not null)
    {
        builder.AppendLine($"- 最近回执：`{commandResult.Label}`：{commandResult.Summary}");
    }

    NotificationItemViewModel? leadNotice = shell.NotificationCenter.Items.FirstOrDefault();
    if (leadNotice is not null)
    {
        builder.AppendLine($"- 首条通知：{leadNotice.Title}");
        if (!string.IsNullOrWhiteSpace(leadNotice.WhatNext))
        {
            builder.AppendLine($"- 通知后续：{leadNotice.WhatNext}");
        }
    }

    string? issue = AuditLifecycleAlignment(yearNumber, shell, bundle);
    if (!string.IsNullOrWhiteSpace(issue))
    {
        auditIssues.Add(issue);
    }

    builder.AppendLine();
}

static void AppendReceipts(
    StringBuilder builder,
    IReadOnlyList<CommandReceiptViewModel> receipts,
    string heading)
{
    if (receipts.Count == 0)
    {
        return;
    }

    builder.AppendLine();
    builder.AppendLine($"### {heading}");
    builder.AppendLine();

    foreach (CommandReceiptViewModel receipt in receipts.Take(3))
    {
        builder.AppendLine($"- `{receipt.Label}`：{receipt.OutcomeSummary}");
    }
}

static string? AuditLifecycleAlignment(
    int yearNumber,
    PresentationShellViewModel shell,
    PresentationReadModelBundle bundle)
{
    PlayerCommandAffordanceSnapshot? primary = GetPrimaryFamilyAffordance(bundle);
    if (primary is null)
    {
        return null;
    }

    List<string> missing = new();

    if (!shell.GreatHall.FamilySummary.Contains(primary.Label, StringComparison.Ordinal))
    {
        missing.Add("堂上家内摘要");
    }

    if (!shell.FamilyCouncil.Summary.Contains(primary.Label, StringComparison.Ordinal))
    {
        missing.Add("族议摘要");
    }

    NotificationItemViewModel? familyNotice = shell.NotificationCenter.Items
        .FirstOrDefault(static item => item.SourceModuleKey == KnownModuleKeys.FamilyCore);
    if (familyNotice is not null
        && !string.IsNullOrWhiteSpace(familyNotice.WhatNext)
        && !familyNotice.WhatNext.Contains(primary.Label, StringComparison.Ordinal))
    {
        missing.Add("通知后续");
    }

    if (missing.Count == 0)
    {
        return null;
    }

    return $"第{yearNumber}年末未完全对齐：{string.Join("、", missing)} 没落到“{primary.Label}”。";
}

static PlayerCommandAffordanceSnapshot? GetPrimaryFamilyAffordance(PresentationReadModelBundle bundle)
{
    return bundle.PlayerCommands.Affordances
        .Where(static command =>
            command.IsEnabled
            && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
            && command.CommandName is
                PlayerCommandNames.SetMourningOrder
                or PlayerCommandNames.SupportNewbornCare
                or PlayerCommandNames.DesignateHeirPolicy
                or PlayerCommandNames.ArrangeMarriage)
        .OrderBy(static command => command.CommandName switch
        {
            PlayerCommandNames.SetMourningOrder => 0,
            PlayerCommandNames.SupportNewbornCare => 1,
            PlayerCommandNames.DesignateHeirPolicy => 2,
            PlayerCommandNames.ArrangeMarriage => 3,
            _ => 10,
        })
        .ThenBy(static command => command.TargetLabel, StringComparer.Ordinal)
        .FirstOrDefault();
}
