﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EFAuditer;
using MoreLinq.Extensions;
using Microsoft.EntityFrameworkCore;
using ntbs_service.DataAccess;
using ntbs_service.Models;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using Serilog;

namespace ntbs_service.Services
{
    public interface INotificationService
    {
        Task AddNotificationAsync(Notification notification);
        Task UpdatePatientDetailsAsync(Notification notification, PatientDetails patientDetails);
        Task UpdatePatientFlagsAsync(PatientDetails patientDetails);
        Task UpdateClinicalDetailsAsync(Notification notification, ClinicalDetails timeline);
        Task UpdateTestDataAsync(Notification notification, TestData testData);
        Task UpdateSitesAsync(int notificationId, IEnumerable<NotificationSite> notificationSites);
        Task UpdateComorbidityAsync(Notification notification, ComorbidityDetails comorbidityDetails);
        Task UpdateHospitalDetailsAsync(Notification notification, HospitalDetails hospitalDetails);
        Task SubmitNotificationAsync(Notification notification);
        Task UpdateContactTracingAsync(Notification notification, ContactTracing contactTracing);
        Task UpdateTravelAndVisitorAsync(Notification notification, TravelDetails travelDetails, VisitorDetails visitorDetails);
        void ClearTravelOrVisitorFields(ITravelOrVisitorDetails travelOrVisitorDetails);
        Task UpdatePatientTbHistoryAsync(Notification notification, PatientTBHistory history);
        Task UpdateSocialRiskFactorsAsync(Notification notification, SocialRiskFactors riskFactors);
        Task UpdateImmunosuppresionDetailsAsync(Notification notification, ImmunosuppressionDetails immunosuppressionDetails);
        Task UpdateMDRDetailsAsync(Notification notification, MDRDetails details);
        Task<Notification> CreateLinkedNotificationAsync(Notification notification, ClaimsPrincipal user);
        Task DenotifyNotificationAsync(int notificationId, DenotificationDetails denotificationDetails,
            string auditUsername);
        Task DeleteNotificationAsync(int notificationId, string deletionReason);
        Task<Notification> CreateNewNotificationForUserAsync(ClaimsPrincipal user);
        Task UpdateNotificationClustersAsync(IEnumerable<NotificationClusterValue> clusterValues);
        Task UpdateDrugResistanceProfileAsync(Notification notification, DrugResistanceProfile drugResistanceProfile);
        Task UpdateMBovisDetailsExposureToKnownCasesAsync(Notification notification, MBovisDetails mBovisDetails);
        Task UpdateMBovisDetailsUnpasteurisedMilkConsumptionAsync(Notification notification, MBovisDetails mBovisDetails);
        Task UpdateMBovisDetailsOccupationExposureAsync(Notification notification, MBovisDetails mBovisDetails);
        Task UpdateMBovisDetailsAnimalExposureAsync(Notification notification, MBovisDetails mBovisDetails);
        Task CloseInactiveNotifications();

    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IUserService _userService;
        private readonly NtbsContext _context;
        private readonly IItemRepository<TreatmentEvent> _treatmentEventRepository;
        private readonly ISpecimenService _specimenService;
        private readonly IAlertService _alertService;


        public NotificationService(
            INotificationRepository notificationRepository,
            IReferenceDataRepository referenceDataRepository,
            IUserService userService,
            IItemRepository<TreatmentEvent> treatmentEventRepository,
            NtbsContext context, 
            ISpecimenService specimenService, 
            IAlertService alertService)
        {
            _notificationRepository = notificationRepository;
            _referenceDataRepository = referenceDataRepository;
            _userService = userService;
            _treatmentEventRepository = treatmentEventRepository;
            _context = context;
            _specimenService = specimenService;
            _alertService = alertService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notification.Add(notification);
            await UpdateDatabaseAsync(NotificationAuditType.Added);
        }

        public async Task UpdatePatientDetailsAsync(Notification notification, PatientDetails patient)
        {
            await UpdatePatientFlagsAsync(patient);
            _context.SetValues(notification.PatientDetails, patient);

            await UpdateDatabaseAsync();
            await _alertService.AutoDismissAlertAsync<DataQualityBirthCountryAlert>(notification);
        }

        public async Task UpdatePatientFlagsAsync(PatientDetails patientDetails)
        {
            if (patientDetails.NhsNumberNotKnown)
            {
                patientDetails.NhsNumber = null;
            }

            if (patientDetails.NoFixedAbode)
            {
                patientDetails.Postcode = null;
            }

            await UpdateUkBorn(patientDetails);
            UpdateEntryYearToUk(patientDetails);

            await UpdateOccupation(patientDetails);
        }

        private async Task UpdateUkBorn(PatientDetails patient)
        {
            if (patient.CountryId == null)
            {
                patient.UkBorn = null;
                return;
            }

            var country = await _referenceDataRepository.GetCountryByIdAsync(patient.CountryId.Value);
            if (country == null)
            {
                patient.UkBorn = null;
                return;
            }

            switch (country.IsoCode)
            {
                case Countries.UkCode:
                    patient.UkBorn = true;
                    break;
                case Countries.UnknownCode:
                    patient.UkBorn = null;
                    break;
                default:
                    patient.UkBorn = false;
                    break;
            }
        }

        private static void UpdateEntryYearToUk(PatientDetails patient)
        {
            if (patient.UkBorn != false)
            {
                patient.YearOfUkEntry = null;
            }
        }

        private async Task UpdateOccupation(PatientDetails patient)
        {
            if (patient.OccupationId.HasValue)
            {
                var occupation = await _referenceDataRepository.GetOccupationByIdAsync(patient.OccupationId.Value);
                if (occupation != null)
                {
                    if (occupation.HasFreeTextField)
                    {
                        return;
                    }
                }
            }

            patient.OccupationOther = null;
        }

        public async Task UpdateClinicalDetailsAsync(Notification notification, ClinicalDetails clinicalDetails)
        {
            _context.SetValues(notification.ClinicalDetails, clinicalDetails);

            await UpdateDatabaseAsync();
            await _alertService.AutoDismissAlertAsync<DataQualityClinicalDatesAlert>(notification);
        }

        public async Task UpdateTestDataAsync(Notification notification, TestData testData)
        {
            testData.NotificationId = notification.NotificationId;
            _context.SetValues(notification.TestData, testData);

            await UpdateDatabaseAsync();
        }

        public async Task UpdateHospitalDetailsAsync(Notification notification, HospitalDetails hospitalDetails)
        {
            _context.SetValues(notification.HospitalDetails, hospitalDetails);

            await UpdateDatabaseAsync();
        }

        public async Task UpdateContactTracingAsync(Notification notification, ContactTracing contactTracing)
        {
            _context.SetValues(notification.ContactTracing, contactTracing);

            await UpdateDatabaseAsync();
        }

        public async Task UpdateTravelAndVisitorAsync(Notification notification, TravelDetails travelDetails, VisitorDetails visitorDetails)
        {
            UpdateTravelDetails(notification, travelDetails);
            UpdateVisitorDetails(notification, visitorDetails);

            await UpdateDatabaseAsync();
        }

        private void UpdateTravelDetails(Notification notification, TravelDetails travelDetails)
        {
            if (travelDetails.HasTravel != true)
            {
                ClearTravelOrVisitorFields(travelDetails);
            }
            _context.SetValues(notification.TravelDetails, travelDetails);
        }

        private void UpdateVisitorDetails(Notification notification, VisitorDetails visitorDetails)
        {
            if (visitorDetails.HasVisitor != true)
            {
                ClearTravelOrVisitorFields(visitorDetails);
            }
            _context.SetValues(notification.VisitorDetails, visitorDetails);
        }

        public void ClearTravelOrVisitorFields(ITravelOrVisitorDetails travelOrVisitorDetails)
        {
            travelOrVisitorDetails.TotalNumberOfCountries = null;
            travelOrVisitorDetails.Country1 = null;
            travelOrVisitorDetails.Country1Id = null;
            travelOrVisitorDetails.StayLengthInMonths1 = null;
            travelOrVisitorDetails.Country2 = null;
            travelOrVisitorDetails.Country2Id = null;
            travelOrVisitorDetails.StayLengthInMonths2 = null;
            travelOrVisitorDetails.Country3 = null;
            travelOrVisitorDetails.Country3Id = null;
            travelOrVisitorDetails.StayLengthInMonths3 = null;
        }

        public async Task UpdatePatientTbHistoryAsync(Notification notification, PatientTBHistory tBHistory)
        {
            _context.SetValues(notification.PatientTBHistory, tBHistory);

            await UpdateDatabaseAsync();
        }

        public async Task UpdateSocialRiskFactorsAsync(Notification notification, SocialRiskFactors socialRiskFactors)
        {
            UpdateSocialRiskFactorsFlags(socialRiskFactors);
            _context.SetValues(notification.SocialRiskFactors, socialRiskFactors);

            _context.SetValues(notification.SocialRiskFactors.RiskFactorDrugs, socialRiskFactors.RiskFactorDrugs);

            _context.SetValues(notification.SocialRiskFactors.RiskFactorHomelessness, socialRiskFactors.RiskFactorHomelessness);

            _context.SetValues(notification.SocialRiskFactors.RiskFactorImprisonment, socialRiskFactors.RiskFactorImprisonment);
            _context.SetValues(notification.SocialRiskFactors.RiskFactorSmoking, socialRiskFactors.RiskFactorSmoking);

            await UpdateDatabaseAsync();
        }

        private static void UpdateSocialRiskFactorsFlags(SocialRiskFactors socialRiskFactors)
        {
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorDrugs);
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorHomelessness);
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorImprisonment);
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorSmoking);
        }

        private static void UpdateRiskFactorFlags(RiskFactorDetails riskFactor)
        {
            if (riskFactor.Status == Status.Yes)
            {
                riskFactor.IsCurrent = riskFactor.IsCurrent ?? false;
                riskFactor.InPastFiveYears = riskFactor.InPastFiveYears ?? false;
                riskFactor.MoreThanFiveYearsAgo = riskFactor.MoreThanFiveYearsAgo ?? false;
            }
            else
            {
                riskFactor.IsCurrent = null;
                riskFactor.InPastFiveYears = null;
                riskFactor.MoreThanFiveYearsAgo = null;
            }
        }

        public async Task UpdateImmunosuppresionDetailsAsync(Notification notification, ImmunosuppressionDetails immunosuppressionDetails)
        {
            if (immunosuppressionDetails.Status != Status.Yes)
            {
                immunosuppressionDetails.HasBioTherapy = null;
                immunosuppressionDetails.HasTransplantation = null;
                immunosuppressionDetails.HasOther = null;
                immunosuppressionDetails.OtherDescription = null;
            }

            if (immunosuppressionDetails.HasOther == false)
            {
                immunosuppressionDetails.OtherDescription = null;
            }

            _context.SetValues(notification.ImmunosuppressionDetails, immunosuppressionDetails);
            await UpdateDatabaseAsync();
        }

        public async Task UpdateMDRDetailsAsync(Notification notification, MDRDetails details)
        {
            _context.SetValues(notification.MDRDetails, details);
            await UpdateDatabaseAsync();
        }

        public async Task UpdateSitesAsync(int notificationId, IEnumerable<NotificationSite> notificationSites)
        {
            var currentSites = await _context.NotificationSite.Where(ns => ns.NotificationId == notificationId).ToListAsync();

            foreach (var newSite in notificationSites)
            {
                var existingSite = currentSites.FirstOrDefault(s => s.SiteId == newSite.SiteId);
                if (existingSite == null)
                {
                    _context.NotificationSite.Add(newSite);
                }
                else if (existingSite.SiteDescription != newSite.SiteDescription)
                {
                    existingSite.SiteDescription = newSite.SiteDescription;
                }
            }

            var sitesToRemove = currentSites.Where(s => !notificationSites.Select(ns => ns.SiteId).Contains(s.SiteId));
            _context.NotificationSite.RemoveRange(sitesToRemove);

            await UpdateDatabaseAsync();
        }

        public async Task SubmitNotificationAsync(Notification notification)
        {
            _context.Attach(notification);
            notification.NotificationStatus = NotificationStatus.Notified;
            notification.SubmissionDate = DateTime.UtcNow;

            await UpdateDatabaseAsync(NotificationAuditType.Notified);
            if (notification.ClinicalDetails.IsPostMortem != true)
            {
                await CreateTreatmentEventNotificationStart(notification);
            }
            await _alertService.AutoDismissAlertAsync<DataQualityDraftAlert>(notification);
        }

        private async Task CreateTreatmentEventNotificationStart(Notification notification)
        {
            await _treatmentEventRepository.AddAsync(new TreatmentEvent
            {
                NotificationId = notification.NotificationId,
                TreatmentEventType = TreatmentEventType.TreatmentStart,
                EventDate = notification.ClinicalDetails.TreatmentStartDate ?? notification.NotificationDate,
                CaseManager = notification.HospitalDetails.CaseManager,
                TbService = notification.HospitalDetails.TBService
            });
        }

        public async Task<Notification> CreateLinkedNotificationAsync(Notification notification, ClaimsPrincipal user)
        {
            var linkedNotification = await CreateNewNotificationForUserAsync(user);
            _context.Attach(linkedNotification);
            _context.SetValues(linkedNotification.PatientDetails, notification.PatientDetails);
            await UpdateDatabaseAsync();

            if (notification.GroupId != null)
            {
                linkedNotification.GroupId = notification.GroupId;
                await UpdateDatabaseAsync();
            }
            else
            {
                var group = new NotificationGroup();
                _context.NotificationGroup.Add(group);
                await UpdateDatabaseAsync(NotificationAuditType.Added);

                linkedNotification.GroupId = group.NotificationGroupId;
                await UpdateDatabaseAsync();

                notification.GroupId = group.NotificationGroupId;
                await UpdateDatabaseAsync();
            }

            return linkedNotification;
        }

        public async Task<Notification> CreateNewNotificationForUserAsync(ClaimsPrincipal user)
        {
            var defaultTbService = await _userService.GetDefaultTbService(user);
            var caseManagerEmail = await GetDefaultCaseManagerEmail(user, defaultTbService?.Code);

            var notification = new Notification
            {
                CreationDate = DateTime.Now,
                HospitalDetails =
                {
                    TBService = defaultTbService,
                    CaseManagerUsername = caseManagerEmail
                }
            };

            await AddNotificationAsync(notification);
            return notification;
        }

        public async Task UpdateNotificationClustersAsync(IEnumerable<NotificationClusterValue> clusterValues)
        {
            foreach (var clusterValue in clusterValues)
            {
                var notification = await _notificationRepository.GetNotificationAsync(clusterValue.NotificationId);
                if (notification == null)
                {
                    throw new DataException(
                        $"Reporting database sourced NotificationId {clusterValue.NotificationId} was not found in NTBS database.");
                }
                
                notification.ClusterId = clusterValue.ClusterId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDrugResistanceProfileAsync(Notification notification, DrugResistanceProfile drugResistanceProfile)
        {
            _context.SetValues(notification.DrugResistanceProfile, drugResistanceProfile);
            await UpdateDatabaseAsync();
        }

        public async Task UpdateMBovisDetailsExposureToKnownCasesAsync(Notification notification, MBovisDetails mBovisDetails)
        {
            _context.SetValues(notification.MBovisDetails, new {mBovisDetails.HasExposureToKnownCases});
            await UpdateDatabaseAsync();
        }

        public async Task UpdateMBovisDetailsUnpasteurisedMilkConsumptionAsync(Notification notification, MBovisDetails mBovisDetails)
        {
            _context.SetValues(notification.MBovisDetails, new {mBovisDetails.HasUnpasteurisedMilkConsumption});
            await UpdateDatabaseAsync();        
        }

        public async Task UpdateMBovisDetailsOccupationExposureAsync(Notification notification, MBovisDetails mBovisDetails)
        {
            _context.SetValues(notification.MBovisDetails, new {mBovisDetails.HasOccupationExposure});
            await UpdateDatabaseAsync();
        }

        public async Task UpdateMBovisDetailsAnimalExposureAsync(Notification notification, MBovisDetails mBovisDetails)
        {
            _context.SetValues(notification.MBovisDetails, new {mBovisDetails.HasAnimalExposure});
            await UpdateDatabaseAsync();            
        }

        private async Task<string> GetDefaultCaseManagerEmail(ClaimsPrincipal user, string tbServiceCode)
        {
            var caseManagersForTbService =
                await _referenceDataRepository.GetCaseManagersByTbServiceCodesAsync(new List<string> { tbServiceCode });
            var userEmail = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var upperUserEmail = userEmail?.ToUpperInvariant();

            return caseManagersForTbService.Any(c => c.Username.ToUpperInvariant() == upperUserEmail) ? userEmail : null;
        }

        private async Task UpdateDatabaseAsync(NotificationAuditType auditType = NotificationAuditType.Edited)
        {
            _context.AddAuditCustomField(CustomFields.AuditDetails, auditType);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateComorbidityAsync(Notification notification, ComorbidityDetails comorbidityDetails)
        {
            _context.SetValues(notification.ComorbidityDetails, comorbidityDetails);

            await UpdateDatabaseAsync();
        }

        public async Task DenotifyNotificationAsync(int notificationId, DenotificationDetails denotificationDetails,
            string auditUsername)
        {
            Log.Debug($"Denotifying {notificationId}");
            var notification = await _notificationRepository.GetNotificationAsync(notificationId);
            if (notification.DenotificationDetails == null)
            {
                notification.DenotificationDetails = denotificationDetails;
            }
            else
            {
                _context.SetValues(notification.DenotificationDetails, denotificationDetails);
            }

            notification.NotificationStatus = NotificationStatus.Denotified;

            await UpdateDatabaseAsync(NotificationAuditType.Denotified);
            
            Log.Debug($"{notificationId} denotified, removing lab result matches");
            var success = await _specimenService.UnmatchAllSpecimensForNotification(notificationId, auditUsername);
            if (!success)
            {
                Log.Error($"Failed to remove some lab results for {notificationId}");
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, string deletionReason)
        {
            var notification = await _notificationRepository.GetNotificationAsync(notificationId);
            notification.DeletionReason = deletionReason;
            notification.GroupId = null;
            notification.NotificationStatus = NotificationStatus.Deleted;

            await UpdateDatabaseAsync(NotificationAuditType.Deleted);
        }

        public async Task CloseInactiveNotifications()
        {
            var notificationsToSetClosed = await _notificationRepository.GetInactiveNotificationsToCloseAsync();
            foreach (var notification in notificationsToSetClosed)
            {
                notification.NotificationStatus = NotificationStatus.Closed;
            }
            await UpdateDatabaseAsync(NotificationAuditType.Closed);
        }
    }
}
