using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Application;

public sealed class MvpFamilyLifecyclePreviewScenario
{
    private static readonly IReadOnlyDictionary<string, int> LifecycleCommandPriority = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [PlayerCommandNames.SetMourningOrder] = 0,
        [PlayerCommandNames.SupportNewbornCare] = 1,
        [PlayerCommandNames.DesignateHeirPolicy] = 2,
        [PlayerCommandNames.ArrangeMarriage] = 3,
    };

    public MvpFamilyLifecyclePreviewResult Build(long seed = 20260419, int warmupMonths = 2)
    {
        if (warmupMonths < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(warmupMonths));
        }

        GameSimulation simulation = SimulationBootstrapper.CreateMvpBootstrap(seed);
        simulation.AdvanceMonths(warmupMonths);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot affordance = SelectPrimaryLifecycleAffordance(beforeBundle);

        PlayerCommandService commandService = new();
        PlayerCommandResult commandResult = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = affordance.SettlementId,
                ClanId = affordance.ClanId,
                CommandName = affordance.CommandName,
            });

        if (!commandResult.Accepted)
        {
            throw new InvalidOperationException($"Preview command was rejected: {commandResult.CommandName}.");
        }

        PresentationReadModelBundle afterCommandBundle = builder.BuildForM2(simulation);
        simulation.AdvanceOneMonth();
        PresentationReadModelBundle afterAdvanceBundle = builder.BuildForM2(simulation);

        return new MvpFamilyLifecyclePreviewResult
        {
            Seed = seed,
            WarmupMonths = warmupMonths,
            BeforeBundle = beforeBundle,
            SelectedAffordance = affordance,
            CommandResult = commandResult,
            AfterCommandBundle = afterCommandBundle,
            AfterAdvanceBundle = afterAdvanceBundle,
        };
    }

    public MvpFamilyLifecycleTenYearPreviewResult BuildTenYear(long seed = 20260419, int years = 10)
    {
        if (years <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(years));
        }

        GameSimulation simulation = SimulationBootstrapper.CreateMvpBootstrap(seed);
        PresentationReadModelBuilder builder = new();
        PlayerCommandService commandService = new();

        PresentationReadModelBundle initialBundle = builder.BuildForM2(simulation);
        List<MvpFamilyLifecycleTenYearCheckpoint> checkpoints = new();
        List<PlayerCommandResult> issuedCommands = new();

        for (int month = 1; month <= years * 12; month += 1)
        {
            PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
            PlayerCommandAffordanceSnapshot? affordance = TrySelectPrimaryLifecycleAffordance(beforeBundle);
            PlayerCommandResult? commandResult = null;

            if (affordance is not null)
            {
                commandResult = commandService.IssueIntent(
                    simulation,
                    new PlayerCommandRequest
                    {
                        SettlementId = affordance.SettlementId,
                        ClanId = affordance.ClanId,
                        CommandName = affordance.CommandName,
                    });

                if (!commandResult.Accepted)
                {
                    throw new InvalidOperationException($"Ten-year preview command was rejected: {commandResult.CommandName}.");
                }

                issuedCommands.Add(commandResult);
            }

            PresentationReadModelBundle afterCommandBundle = builder.BuildForM2(simulation);
            simulation.AdvanceOneMonth();
            PresentationReadModelBundle afterAdvanceBundle = builder.BuildForM2(simulation);

            if (month % 12 == 0)
            {
                checkpoints.Add(new MvpFamilyLifecycleTenYearCheckpoint
                {
                    YearNumber = month / 12,
                    SelectedAffordance = affordance,
                    CommandResult = commandResult,
                    BeforeCommandBundle = beforeBundle,
                    AfterCommandBundle = afterCommandBundle,
                    AfterAdvanceBundle = afterAdvanceBundle,
                });
            }
        }

        return new MvpFamilyLifecycleTenYearPreviewResult
        {
            Seed = seed,
            Years = years,
            InitialBundle = initialBundle,
            YearlyCheckpoints = checkpoints,
            IssuedCommands = issuedCommands,
        };
    }

    private static PlayerCommandAffordanceSnapshot SelectPrimaryLifecycleAffordance(PresentationReadModelBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        PlayerCommandAffordanceSnapshot? affordance = TrySelectPrimaryLifecycleAffordance(bundle);

        if (affordance is null)
        {
            throw new InvalidOperationException("Preview scenario could not find an enabled family lifecycle command.");
        }

        return affordance;
    }

    private static PlayerCommandAffordanceSnapshot? TrySelectPrimaryLifecycleAffordance(PresentationReadModelBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        return bundle.PlayerCommands.Affordances
            .Where(static entry =>
                entry.IsEnabled
                && string.Equals(entry.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && LifecycleCommandPriority.ContainsKey(entry.CommandName))
            .OrderBy(static entry => LifecycleCommandPriority[entry.CommandName])
            .ThenBy(static entry => entry.TargetLabel, StringComparer.Ordinal)
            .FirstOrDefault();
    }
}

public sealed class MvpFamilyLifecyclePreviewResult
{
    public long Seed { get; init; }

    public int WarmupMonths { get; init; }

    public PresentationReadModelBundle BeforeBundle { get; init; } = new();

    public PlayerCommandAffordanceSnapshot SelectedAffordance { get; init; } = new();

    public PlayerCommandResult CommandResult { get; init; } = new();

    public PresentationReadModelBundle AfterCommandBundle { get; init; } = new();

    public PresentationReadModelBundle AfterAdvanceBundle { get; init; } = new();
}

public sealed class MvpFamilyLifecycleTenYearPreviewResult
{
    public long Seed { get; init; }

    public int Years { get; init; }

    public PresentationReadModelBundle InitialBundle { get; init; } = new();

    public IReadOnlyList<MvpFamilyLifecycleTenYearCheckpoint> YearlyCheckpoints { get; init; } = Array.Empty<MvpFamilyLifecycleTenYearCheckpoint>();

    public IReadOnlyList<PlayerCommandResult> IssuedCommands { get; init; } = Array.Empty<PlayerCommandResult>();
}

public sealed class MvpFamilyLifecycleTenYearCheckpoint
{
    public int YearNumber { get; init; }

    public PlayerCommandAffordanceSnapshot? SelectedAffordance { get; init; }

    public PlayerCommandResult? CommandResult { get; init; }

    public PresentationReadModelBundle BeforeCommandBundle { get; init; } = new();

    public PresentationReadModelBundle AfterCommandBundle { get; init; } = new();

    public PresentationReadModelBundle AfterAdvanceBundle { get; init; } = new();
}
