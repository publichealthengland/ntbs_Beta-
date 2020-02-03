﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ntbs_service.DataAccess;
using ntbs_service.Helpers;
using ntbs_service.Models;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using ntbs_service.Models.ReferenceEntities;
using ntbs_service.Models.Validations;
using ntbs_service.Services;

namespace ntbs_service.Pages.Notifications.Edit
{
    [BindProperties]
    public class ClinicalDetailsModel : NotificationEditModelBase
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAlertService _alertService;

        public ClinicalDetails ClinicalDetails { get; set; }

        public Dictionary<SiteId, bool> NotificationSiteMap { get; set; }

        public List<Site> Sites { get; set; }
        // We want to bind to the full model rather than a string for SiteDescription so we can validate against the data annotation
        public NotificationSite OtherSite { get; set; }

        public int? PatientBirthYear { get; set; }

        public FormattedDate FormattedSymptomDate { get; set; }
        public FormattedDate FormattedFirstPresentationDate { get; set; }
        public FormattedDate FormattedTbServicePresentationDate { get; set; }
        public FormattedDate FormattedDiagnosisDate { get; set; }
        public FormattedDate FormattedTreatmentDate { get; set; }
        public FormattedDate FormattedDeathDate { get; set; }
        public FormattedDate FormattedMdrTreatmentDate { get; set; }
        public FormattedDate FormattedHomeVisitDate { get; set; }

        public ClinicalDetailsModel(
            INotificationService service,
            IAuthorizationService authorizationService,
            INotificationRepository notificationRepository,
            IReferenceDataRepository referenceDataRepository,
            IAlertService alertService) : base(service, authorizationService, notificationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _alertService = alertService;

            CurrentPage = NotificationSubPaths.EditClinicalDetails;
        }

        protected override async Task<IActionResult> PrepareAndDisplayPageAsync(bool isBeingSubmitted)
        {
            ClinicalDetails = Notification.ClinicalDetails;
            await SetNotificationProperties(isBeingSubmitted, ClinicalDetails);

            var notificationSites = Notification.NotificationSites;
            notificationSites.ForEach(x => x.ShouldValidateFull = Notification.ShouldValidateFull);

            SetupNotificationSiteMap(notificationSites);
            OtherSite = new NotificationSite
            {
                SiteId = (int)SiteId.OTHER,
                SiteDescription = notificationSites.FirstOrDefault(ns => ns.SiteId == (int)SiteId.OTHER)?.SiteDescription,
                ShouldValidateFull = Notification.ShouldValidateFull
            };
            Sites = (await _referenceDataRepository.GetAllSitesAsync()).ToList();

            PatientBirthYear = Notification.PatientDetails.Dob?.Year;

            FormattedSymptomDate = ClinicalDetails.SymptomStartDate.ConvertToFormattedDate();
            FormattedFirstPresentationDate = ClinicalDetails.FirstPresentationDate.ConvertToFormattedDate();
            FormattedTbServicePresentationDate = ClinicalDetails.TBServicePresentationDate.ConvertToFormattedDate();
            FormattedDiagnosisDate = ClinicalDetails.DiagnosisDate.ConvertToFormattedDate();
            FormattedTreatmentDate = ClinicalDetails.TreatmentStartDate.ConvertToFormattedDate();
            FormattedDeathDate = ClinicalDetails.DeathDate.ConvertToFormattedDate();
            FormattedMdrTreatmentDate = ClinicalDetails.MDRTreatmentStartDate.ConvertToFormattedDate();
            FormattedHomeVisitDate = ClinicalDetails.FirstHomeVisitDate.ConvertToFormattedDate();

            if (ClinicalDetails.ShouldValidateFull)
            {
                TryValidateModel(this);
                
                // EditPageErrorDictionary is null only if coming from a GET call, we want to guard here
                // only in a GET call
                if (EditPageErrorDictionary == null && 
                    (!NotificationSiteMap.ContainsKey(SiteId.OTHER) || !NotificationSiteMap[SiteId.OTHER]))
                {
                    ModelState.Remove("OtherSite.SiteDescription");
                }
            }

            return Page();
        }

        protected override async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await NotificationRepository.GetNotificationWithNotificationSitesAsync(notificationId);
        }

        private void SetupNotificationSiteMap(IEnumerable<NotificationSite> notificationSites)
        {
            var notificationSitesList = notificationSites.ToList();
            NotificationSiteMap = new Dictionary<SiteId, bool>();
            foreach (SiteId siteId in Enum.GetValues(typeof(SiteId)))
            {
                NotificationSiteMap.Add(siteId, notificationSitesList.FirstOrDefault(ns => ns.SiteId == (int)siteId) != null);
            }
        }

        protected override IActionResult RedirectAfterSaveForDraft(bool isBeingSubmitted)
        {
            return RedirectToPage("./TestResults", new { NotificationId, isBeingSubmitted });
        }

        protected override async Task ValidateAndSave()
        {
            UpdateFlags();
            new List<(string key, FormattedDate date)> {
                (nameof(ClinicalDetails.SymptomStartDate), FormattedSymptomDate),
                (nameof(ClinicalDetails.FirstPresentationDate), FormattedFirstPresentationDate),
                (nameof(ClinicalDetails.TBServicePresentationDate), FormattedTbServicePresentationDate),
                (nameof(ClinicalDetails.DiagnosisDate), FormattedDiagnosisDate),
                (nameof(ClinicalDetails.TreatmentStartDate), FormattedTreatmentDate),
                (nameof(ClinicalDetails.DeathDate), FormattedDeathDate),
                (nameof(ClinicalDetails.FirstHomeVisitDate), FormattedHomeVisitDate),
                (nameof(ClinicalDetails.MDRTreatmentStartDate), FormattedMdrTreatmentDate)
            }.ForEach(item => 
                ValidationService.TrySetFormattedDate(ClinicalDetails, "ClinicalDetails", item.key, item.date)
            );

            if (ClinicalDetails.BCGVaccinationYear != null)
            {
                ValidationService.ValidateYearComparison(ClinicalDetails, nameof(ClinicalDetails.BCGVaccinationYear),
                (int)ClinicalDetails.BCGVaccinationYear, PatientBirthYear);
            }

            var notificationSites = CreateNotificationSitesFromModel(Notification).ToList();

            // Add additional field required for date validation
            ClinicalDetails.Dob = Notification.PatientDetails.Dob;

            ClinicalDetails.SetFullValidation(Notification.NotificationStatus);
            OtherSite?.SetFullValidation(Notification.NotificationStatus);

            // Since notification has other properties which are not populated by this page but have validation rules, 
            // validation of a whole Notification model will result in validation errors.
            // Therefore we need to manually validate NotificationSites and TryValidate other models
            if (Notification.ShouldValidateFull && !notificationSites.Any())
            {
                ModelState.AddModelError("Notification.NotificationSites", ValidationMessages.DiseaseSiteIsRequired);
            }

            TryValidateModel(ClinicalDetails, nameof(ClinicalDetails));
            if (OtherSite != null)
            {
                TryValidateModel(OtherSite, nameof(OtherSite));
            }

            await CreateOrDismissMDRAlertDependentOnIsMDRTreatmentState();

            if (ModelState.IsValid)
            {
                await Service.UpdateClinicalDetailsAsync(Notification, ClinicalDetails);
                await Service.UpdateSitesAsync(Notification.NotificationId, notificationSites);
            }
        }

        private async Task CreateOrDismissMDRAlertDependentOnIsMDRTreatmentState()
        {
            if (Notification.ClinicalDetails.IsMDRTreatment != true && ClinicalDetails.IsMDRTreatment == true) // TODO NTBS-368 drug resistance profile check
            {
                var mdrAlert = new MdrAlert() {NotificationId = NotificationId};
                await _alertService.AddUniqueAlertAsync(mdrAlert);
            }
            else if (Notification.ClinicalDetails.IsMDRTreatment == true && ClinicalDetails.IsMDRTreatment == false)  // TODO NTBS-368 drug resistance profile check
            {
                if (Notification.MDRDetails.MDRDetailsEntered)
                {
                    ModelState.AddModelError("ClinicalDetails.IsMDRTreatment", ValidationMessages.MDRCantChange);
                }
                else
                {
                    await _alertService.DismissMatchingAlertAsync(NotificationId, AlertType.EnhancedSurveillanceMDR);
                }
            }
        }

        private void UpdateFlags()
        {
            if (ClinicalDetails.IsSymptomatic == false)
            {
                ClinicalDetails.SymptomStartDate = null;
                FormattedSymptomDate = ClinicalDetails.SymptomStartDate.ConvertToFormattedDate();
                ModelState.Remove("ClinicalDetails.SymptomStartDate");
            }

            if (ClinicalDetails.DidNotStartTreatment == true)
            {
                ClinicalDetails.TreatmentStartDate = null;
                FormattedTreatmentDate = ClinicalDetails.TreatmentStartDate.ConvertToFormattedDate();
                ModelState.Remove("ClinicalDetails.TreatmentStartDate");
            }

            if (ClinicalDetails.IsPostMortem == false)
            {
                ClinicalDetails.DeathDate = null;
                FormattedDeathDate = ClinicalDetails.DeathDate.ConvertToFormattedDate();
                ModelState.Remove("ClinicalDetails.DeathDate");
            }

            if (ClinicalDetails.BCGVaccinationState != Status.Yes)
            {
                ClinicalDetails.BCGVaccinationYear = null;
                ModelState.Remove("ClinicalDetails.BCGVaccinationYear");
            }

            if (ClinicalDetails.IsMDRTreatment.HasValue && !ClinicalDetails.IsMDRTreatment.Value)
            {
                ClinicalDetails.MDRTreatmentStartDate = null;
                FormattedMdrTreatmentDate = ClinicalDetails.MDRTreatmentStartDate.ConvertToFormattedDate();
                ModelState.Remove("ClinicalDetails.MDRTreatmentStartDate");
            }

            if (!NotificationSiteMap.ContainsKey(SiteId.OTHER) || !NotificationSiteMap[SiteId.OTHER])
            {
                OtherSite = null;
                ModelState.Remove("OtherSite.SiteDescription");
            }

            if (ClinicalDetails.HomeVisitCarriedOut != Status.Yes)
            {
                ClinicalDetails.FirstHomeVisitDate = null;
                FormattedHomeVisitDate = ClinicalDetails.FirstHomeVisitDate.ConvertToFormattedDate();
                ModelState.Remove("ClinicalDetails.FirstHomeVisitDate");
            }

            if (ClinicalDetails.HealthcareSetting != HealthcareSetting.Other)
            {
                ClinicalDetails.HealthcareDescription = null;
            }

            if (ClinicalDetails.IsDotOffered == false)
            {
                ClinicalDetails.DotStatus = null;
            }
        }

        private IEnumerable<NotificationSite> CreateNotificationSitesFromModel(Notification notification)
        {
            foreach (var (siteId, value) in NotificationSiteMap)
            {
                if (value)
                {
                    yield return new NotificationSite
                    {
                        ShouldValidateFull = notification.ShouldValidateFull,
                        NotificationId = notification.NotificationId,
                        SiteId = (int)siteId,
                        SiteDescription = siteId == SiteId.OTHER ? OtherSite.SiteDescription : null
                    };
                }
            }
        }

        public ContentResult OnGetValidateClinicalDetailsDate(string key, string day, string month, string year)
        {
            return ValidationService.GetDateValidationResult<ClinicalDetails>(key, day, month, year);
        }

        public ContentResult OnGetValidateNotificationSites(IEnumerable<string> valueList, bool shouldValidateFull)
        {
            const string key = "NotificationSites";
            var notificationSites = new List<NotificationSite>();
            foreach (var value in valueList)
            {
                notificationSites.Add(new NotificationSite
                {
                    SiteId = (int)Enum.Parse(typeof(SiteId), value)
                });
            }

            return ValidationService.GetPropertyValidationResult<Notification>(key, notificationSites, shouldValidateFull);
        }

        public ContentResult OnGetValidateNotificationSiteProperty(string key, string value, bool shouldValidateFull)
        {
            var notificationSite = new NotificationSite
            {
                ShouldValidateFull = shouldValidateFull,
                SiteId = (int)SiteId.OTHER
            };
            return ValidationService.GetPropertyValidationResult(notificationSite, key, value);
        }

        public ContentResult OnGetValidateClinicalDetailsYearComparison(int newYear, int existingYear, string propertyName)
        {
            return ValidationService.GetYearComparisonValidationResult(newYear, existingYear, propertyName);
        }

        public ContentResult OnGetValidateClinicalDetailsProperties(IEnumerable<Dictionary<string, string>> keyValuePairs)
        {
            List<(string, object)> propertyValueTuples = new List<(string key, object property)>();
            foreach (var keyValuePair in keyValuePairs)
            {
                propertyValueTuples.Add((keyValuePair["key"], keyValuePair["value"]));
            }
            return ValidationService.GetMultiplePropertiesValidationResult<ClinicalDetails>(propertyValueTuples);
        }
        
        public ContentResult OnGetValidateClinicalDetailsProperty(string key, string value, bool shouldValidateFull)
        {
            return ValidationService.GetPropertyValidationResult<ClinicalDetails>(key, value, shouldValidateFull);
        }

    }
}
