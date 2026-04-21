using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed partial class ConflictAndForceModule : ModuleRunner<ConflictAndForceState>
{
    private sealed class ConflictAndForceQueries : IConflictAndForceQueries

    {

        private readonly ConflictAndForceState _state;


        public ConflictAndForceQueries(ConflictAndForceState state)

        {

            _state = state;

        }


        public LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId)

        {

            SettlementForceState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);

            return Clone(settlement);

        }


        public IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces()

        {

            return _state.Settlements

                .OrderBy(static settlement => settlement.SettlementId.Value)

                .Select(Clone)

                .ToArray();

        }


        private static LocalForcePoolSnapshot Clone(SettlementForceState settlement)

        {

            return new LocalForcePoolSnapshot

            {

                SettlementId = settlement.SettlementId,

                GuardCount = settlement.GuardCount,

                RetainerCount = settlement.RetainerCount,

                MilitiaCount = settlement.MilitiaCount,

                EscortCount = settlement.EscortCount,

                Readiness = settlement.Readiness,

                CommandCapacity = settlement.CommandCapacity,

                ResponseActivationLevel = settlement.ResponseActivationLevel,

                OrderSupportLevel = settlement.OrderSupportLevel,

                IsResponseActivated = settlement.IsResponseActivated,

                HasActiveConflict = settlement.HasActiveConflict,

                CampaignFatigue = settlement.CampaignFatigue,

                CampaignEscortStrain = settlement.CampaignEscortStrain,

                LastCampaignFalloutTrace = settlement.LastCampaignFalloutTrace,

                LastConflictTrace = settlement.LastConflictTrace,

            };

        }


        private static ForceGroupSnapshot CloneGroup(ForceGroupState group)

        {

            return new ForceGroupSnapshot

            {

                ForceId = group.ForceId,

                Family = group.Family,

                OwnerKey = group.OwnerKey,

                Location = group.Location,

                Strength = group.Strength,

                Readiness = group.Readiness,

                Morale = group.Morale,

                Discipline = group.Discipline,

                Fatigue = group.Fatigue,

            };

        }


        private static ConflictIncidentSnapshot CloneIncident(ConflictIncidentState incident)

        {

            return new ConflictIncidentSnapshot

            {

                IncidentId = incident.IncidentId,

                Scale = incident.Scale,

                Location = incident.Location,

                RouteId = incident.RouteId,

                Attackers = incident.Attackers.ToArray(),

                Defenders = incident.Defenders.ToArray(),

                Outcome = incident.Outcome,

                CauseKey = incident.CauseKey,

                OccurredYear = incident.OccurredYear,

                OccurredMonth = incident.OccurredMonth,

            };

        }

    }


    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)

    {

        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);

    }


    private static class EmptyDisorderSnapshot

    {

        public static SettlementDisorderSnapshot For(SettlementId settlementId)

        {

            return new SettlementDisorderSnapshot

            {

                SettlementId = settlementId,

            };

        }

    }
}
