using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private sealed class OfficeQueries : IOfficeAndCareerQueries

    {

        private readonly OfficeAndCareerState _state;


        public OfficeQueries(OfficeAndCareerState state)

        {

            _state = state;

        }


        public OfficeCareerSnapshot GetRequiredCareer(PersonId personId)

        {

            OfficeCareerState career = _state.People.Single(person => person.PersonId == personId);

            return CloneCareer(career);

        }


        public IReadOnlyList<OfficeCareerSnapshot> GetCareers()

        {

            return _state.People

                .OrderBy(static person => person.PersonId.Value)

                .Select(CloneCareer)

                .ToArray();

        }


        public JurisdictionAuthoritySnapshot GetRequiredJurisdiction(SettlementId settlementId)

        {

            JurisdictionAuthorityState jurisdiction = _state.Jurisdictions.Single(authority => authority.SettlementId == settlementId);

            return CloneJurisdiction(jurisdiction);

        }


        public IReadOnlyList<JurisdictionAuthoritySnapshot> GetJurisdictions()

        {

            return _state.Jurisdictions

                .OrderBy(static authority => authority.SettlementId.Value)

                .Select(CloneJurisdiction)

                .ToArray();

        }


        public IReadOnlyList<OfficialPostSnapshot> GetOfficialPosts()
        {
            return _state.OfficialPosts
                .OrderBy(static post => post.Location.Value)
                .ThenBy(static post => post.PostId, StringComparer.Ordinal)
                .Select(static post => new OfficialPostSnapshot
                {
                    PostId = post.PostId,
                    Location = post.Location,
                    Rank = post.Rank,
                    PostTitle = post.PostTitle,
                    CurrentHolder = post.CurrentHolder,
                    VacancyMonths = post.VacancyMonths,
                    PetitionBacklog = post.PetitionBacklog,
                    ClerkDependence = post.ClerkDependence,
                    EvaluationPressure = post.EvaluationPressure,
                })
                .ToArray();
        }

        public IReadOnlyList<WaitingListEntrySnapshot> GetWaitingList()
        {
            return _state.WaitingList
                .OrderBy(static entry => entry.PersonId.Value)
                .Select(static entry => new WaitingListEntrySnapshot
                {
                    PersonId = entry.PersonId,
                    SettlementId = entry.SettlementId,
                    DisplayName = entry.DisplayName,
                    QualificationTier = entry.QualificationTier,
                    WaitingMonths = entry.WaitingMonths,
                    PatronageSupport = entry.PatronageSupport,
                })
                .ToArray();
        }


        private static OfficeCareerSnapshot CloneCareer(OfficeCareerState career)

        {

            return new OfficeCareerSnapshot

            {

                PersonId = career.PersonId,

                ClanId = career.ClanId,

                SettlementId = career.SettlementId,

                DisplayName = career.DisplayName,

                IsEligible = career.IsEligible,

                HasAppointment = career.HasAppointment,

                OfficeTitle = career.OfficeTitle,

                AuthorityTier = career.AuthorityTier,

                AppointmentPressure = career.AppointmentPressure,

                ClerkDependence = career.ClerkDependence,

                JurisdictionLeverage = career.JurisdictionLeverage,

                PetitionPressure = career.PetitionPressure,

                PetitionBacklog = career.PetitionBacklog,

                ServiceMonths = career.ServiceMonths,

                PromotionMomentum = career.PromotionMomentum,

                DemotionPressure = career.DemotionPressure,

                OfficialDefectionRisk = career.OfficialDefectionRisk,

                CurrentAdministrativeTask = career.CurrentAdministrativeTask,

                AdministrativeTaskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(career.CurrentAdministrativeTask, career.AuthorityTier),

                AdministrativeTaskLoad = career.AdministrativeTaskLoad,

                OfficeReputation = career.OfficeReputation,

                LastOutcome = career.LastOutcome,

                LastPetitionOutcome = career.LastPetitionOutcome,

                PetitionOutcomeCategory = OfficeAndCareerDescriptors.DeterminePetitionOutcomeCategory(career.LastPetitionOutcome),

                PromotionPressureLabel = OfficeAndCareerDescriptors.DescribePromotionPressure(career.PromotionMomentum),

                DemotionPressureLabel = OfficeAndCareerDescriptors.DescribeDemotionPressure(career.DemotionPressure),

                AuthorityTrajectorySummary = OfficeAndCareerDescriptors.BuildAuthorityTrajectorySummary(career),

                LastExplanation = career.LastExplanation,

            };

        }


        private static JurisdictionAuthoritySnapshot CloneJurisdiction(JurisdictionAuthorityState jurisdiction)

        {

            return new JurisdictionAuthoritySnapshot

            {

                SettlementId = jurisdiction.SettlementId,

                LeadOfficialPersonId = jurisdiction.LeadOfficialPersonId,

                LeadOfficialName = jurisdiction.LeadOfficialName,

                LeadOfficeTitle = jurisdiction.LeadOfficeTitle,

                AuthorityTier = jurisdiction.AuthorityTier,

                JurisdictionLeverage = jurisdiction.JurisdictionLeverage,

                ClerkDependence = jurisdiction.ClerkDependence,

                PetitionPressure = jurisdiction.PetitionPressure,

                PetitionBacklog = jurisdiction.PetitionBacklog,

                CurrentAdministrativeTask = jurisdiction.CurrentAdministrativeTask,

                AdministrativeTaskLoad = jurisdiction.AdministrativeTaskLoad,

                LastPetitionOutcome = jurisdiction.LastPetitionOutcome,

                AdministrativeTaskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(jurisdiction.CurrentAdministrativeTask, jurisdiction.AuthorityTier),

                PetitionOutcomeCategory = OfficeAndCareerDescriptors.DeterminePetitionOutcomeCategory(jurisdiction.LastPetitionOutcome),

                LastAdministrativeTrace = jurisdiction.LastAdministrativeTrace,

            };

        }

    }


}
