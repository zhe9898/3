using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal sealed class GreatHallProjectionContext
{
	private GreatHallProjectionContext()
	{
	}

	internal ClanSnapshot? LeadClan { get; private init; }

	internal NarrativeNotificationSnapshot? LeadNotification { get; private init; }

	internal int StudyingCount { get; private init; }

	internal int PassedCount { get; private init; }

	internal int ProfitableTradeCount { get; private init; }

	internal int AppointedCareerCount { get; private init; }

	internal int UrgentCount { get; private init; }

	internal int ConsequentialCount { get; private init; }

	internal int BackgroundCount { get; private init; }

	internal JurisdictionAuthoritySnapshot? LeadJurisdiction { get; private init; }

	internal SettlementPublicLifeSnapshot? LeadPublicLife { get; private init; }

	internal bool HasHallLead { get; private init; }

	internal string HallLeadTitle { get; private init; } = string.Empty;

	internal string HallLeadGuidance { get; private init; } = string.Empty;

	internal static GreatHallProjectionContext Create(
		PresentationReadModelBundle bundle,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(bundle);
		ArgumentNullException.ThrowIfNull(notifications);

		bool hasHallLead = HallDocketShellAdapter.TryBuildGreatHallLead(bundle.HallDocket, out string title, out string guidance);

		return new GreatHallProjectionContext
		{
			LeadClan = bundle.Clans
				.OrderByDescending(clan => clan.Prestige)
				.ThenBy(clan => clan.ClanName, StringComparer.Ordinal)
				.FirstOrDefault(),
			LeadNotification = notifications.FirstOrDefault(),
			StudyingCount = bundle.EducationCandidates.Count(candidate => candidate.IsStudying),
			PassedCount = bundle.EducationCandidates.Count(candidate => candidate.HasPassedLocalExam),
			ProfitableTradeCount = bundle.ClanTrades.Count(trade => string.Equals(trade.LastOutcome, "Profit", StringComparison.Ordinal)),
			AppointedCareerCount = bundle.OfficeCareers.Count(career => career.HasAppointment),
			UrgentCount = notifications.Count(notification => notification.Tier == NotificationTier.Urgent),
			ConsequentialCount = notifications.Count(notification => notification.Tier == NotificationTier.Consequential),
			BackgroundCount = notifications.Count(notification => notification.Tier == NotificationTier.Background),
			LeadJurisdiction = bundle.OfficeJurisdictions
				.OrderByDescending(jurisdiction => jurisdiction.AuthorityTier)
				.ThenByDescending(jurisdiction => jurisdiction.JurisdictionLeverage)
				.ThenBy(jurisdiction => jurisdiction.SettlementId.Value)
				.FirstOrDefault(),
			LeadPublicLife = bundle.PublicLifeSettlements
				.OrderByDescending(settlement =>
					settlement.StreetTalkHeat
					+ settlement.MarketBuzz
					+ settlement.NoticeVisibility
					+ settlement.RoadReportLag
					+ settlement.PrefectureDispatchPressure)
				.ThenBy(settlement => settlement.SettlementName, StringComparer.Ordinal)
				.FirstOrDefault(),
			HasHallLead = hasHallLead,
			HallLeadTitle = title,
			HallLeadGuidance = guidance
		};
	}
}
