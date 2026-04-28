using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class GreatHallShellAdapter
{
	internal static GreatHallDashboardViewModel BuildGreatHall(
		PresentationReadModelBundle bundle,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications,
		NotificationProjectionContext notificationContext)
	{
		ArgumentNullException.ThrowIfNull(bundle);
		ArgumentNullException.ThrowIfNull(notifications);
		ArgumentNullException.ThrowIfNull(notificationContext);
		GreatHallProjectionContext context = GreatHallProjectionContext.Create(bundle, notifications);

		ClanSnapshot? leadClan = context.LeadClan;
		NarrativeNotificationSnapshot? leadNotification = context.LeadNotification;
		int studyingCount = context.StudyingCount;
		int passedCount = context.PassedCount;
		int profitableTrades = context.ProfitableTradeCount;
		int appointedCareers = context.AppointedCareerCount;
		JurisdictionAuthoritySnapshot? leadJurisdiction = context.LeadJurisdiction;
		SettlementPublicLifeSnapshot? leadPublicLife = context.LeadPublicLife;
		bool hasHallLead = context.HasHallLead;
		string title = context.HallLeadTitle;
		string guidance = context.HallLeadGuidance;
		GreatHallDashboardViewModel greatHall = new GreatHallDashboardViewModel
		{
			CurrentDateLabel = $"{bundle.CurrentDate.Year}-{bundle.CurrentDate.Month:D2}",
			ReplayHash = bundle.ReplayHash,
			UrgentCount = context.UrgentCount,
			ConsequentialCount = context.ConsequentialCount,
			BackgroundCount = context.BackgroundCount,
			FamilySummary = FamilyShellAdapter.BuildGreatHallFamilySummary(leadClan, bundle.PlayerCommands.Affordances),
			EducationSummary = $"塾馆在读{studyingCount}人，场屋得捷{passedCount}人。",
			TradeSummary = $"市账{bundle.ClanTrades.Count}册，得利{profitableTrades}支。",
			MobilitySummary = BuildGreatHallMobilitySummary(bundle),
			PublicLifeSummary = leadPublicLife == null
				? "乡里、镇市与县门今月尚静。"
				: PublicLifeShellAdapter.BuildGreatHallPublicLifeSummary(bundle.PublicLifeSettlements, leadPublicLife),
			GovernanceSummary = OfficeShellAdapter.BuildGreatHallGovernanceFallbackSummary(appointedCareers, leadJurisdiction),
			WarfareSummary = WarfareCampaignShellAdapter.BuildGreatHallWarfareSummary(bundle),
			AftermathDocketSummary = WarfareAftermathShellAdapter.BuildGreatHallAftermathDocketSummary(bundle, notifications),
			LeadNoticeTitle = hasHallLead ? title : leadNotification?.Title ?? "堂上暂无急报",
			LeadNoticeGuidance = hasHallLead
				? guidance
				: NotificationShellAdapter.BuildLeadNoticeGuidance(notificationContext, leadNotification),
			SecondaryDockets = HallDocketShellAdapter.BuildGreatHallSecondaryDockets(bundle.HallDocket)
		};

		greatHall.GovernanceSummary = GovernanceShellAdapter.BuildGreatHallGovernanceSummary(bundle, greatHall.GovernanceSummary);
		return greatHall;
	}

	private static string BuildGreatHallMobilitySummary(PresentationReadModelBundle bundle)
	{
		SettlementMobilitySnapshot? leadMobility = bundle.SettlementMobilities
			.OrderByDescending(static mobility => mobility.OutflowPressure + mobility.FloatingPopulation)
			.ThenBy(static mobility => mobility.SettlementId.Value)
			.FirstOrDefault();
		string personnelFlowReadiness = bundle.PlayerCommands.PersonnelFlowReadinessSummary;
		string personnelFlowOwnerLaneGate = bundle.PlayerCommands.PersonnelFlowOwnerLaneGateSummary;
		if (leadMobility is null)
		{
			return AppendPersonnelFlowReadbacks(
				$"{bundle.FidelityScale.Summary} {bundle.FidelityScale.InfluenceFootprintReadbackSummary}",
				personnelFlowReadiness,
				personnelFlowOwnerLaneGate);
		}

		return AppendPersonnelFlowReadbacks(
			$"{bundle.FidelityScale.Summary} {bundle.FidelityScale.InfluenceFootprintReadbackSummary} {leadMobility.MovementReadbackSummary}",
			personnelFlowReadiness,
			personnelFlowOwnerLaneGate);
	}

	private static string AppendPersonnelFlowReadbacks(string summary, params string[] personnelFlowReadbacks)
	{
		string[] visibleReadbacks = personnelFlowReadbacks
			.Where(static readback => !string.IsNullOrWhiteSpace(readback))
			.ToArray();

		return visibleReadbacks.Length == 0
			? summary
			: $"{summary} {string.Join(' ', visibleReadbacks)}";
	}
}
