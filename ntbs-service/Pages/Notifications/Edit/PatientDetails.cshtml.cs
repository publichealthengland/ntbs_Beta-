﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ntbs_service.DataAccess;
using ntbs_service.Helpers;
using ntbs_service.Models;
using ntbs_service.Models.Entities;
using ntbs_service.Models.ReferenceEntities;
using ntbs_service.Services;

namespace ntbs_service.Pages.Notifications.Edit
{
    public class PatientDetailsModel : NotificationEditModelBase
    {
        private readonly IPostcodeService _postcodeService;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public SelectList Ethnicities { get; set; }
        public SelectList Countries { get; set; }
        public List<string> RenderConditionalCountryFieldIds { get; set; }
        public List<Sex> Sexes { get; set; }
        public SelectList Occupations { get; set; }
        public List<string> RenderConditionalOccupationFieldIds { get; set; }
        public Dictionary<string, string> DuplicateNhsNumberNotifications { get; set; }

        [BindProperty]
        public PatientDetails PatientDetails { get; set; }

        [BindProperty]
        public FormattedDate FormattedDob { get; set; }

        public PatientDetailsModel(
            INotificationService service,
            IAuthorizationService authorizationService,
            INotificationRepository notificationRepository,
            IAlertRepository alertRepository,
            IPostcodeService postcodeService,
            IReferenceDataRepository referenceDataRepository) : base(service, authorizationService, notificationRepository, alertRepository)
        {
            _postcodeService = postcodeService;
            _referenceDataRepository = referenceDataRepository;

            CurrentPage = NotificationSubPaths.EditPatientDetails;
        }

        private async Task GenerateReferenceDataAsync()
        {
            Ethnicities = new SelectList(
                await _referenceDataRepository.GetAllOrderedEthnicitiesAsync(),
                nameof(Ethnicity.EthnicityId),
                nameof(Ethnicity.Label));

            var countries = await _referenceDataRepository.GetAllCountriesAsync();
            if (Notification.PatientDetails?.Country?.IsLegacy ?? false)
            {
                countries = countries.Prepend(Notification.PatientDetails.Country).ToList();
            }
            Countries = new SelectList(countries, nameof(Country.CountryId), nameof(Country.Name));
            RenderConditionalCountryFieldIds = countries
                .Where(c =>
                    c.IsoCode == Models.Countries.UkCode
                    || c.IsoCode == Models.Countries.UnknownCode)
                .Select(c => c.CountryId.ToString())
                .ToList();

            Sexes = (await _referenceDataRepository.GetAllSexesAsync()).ToList();

            var occupations = await _referenceDataRepository.GetAllOccupationsAsync();
            Occupations = new SelectList(
                items: occupations,
                dataValueField: nameof(Occupation.OccupationId),
                dataTextField: nameof(Occupation.Role),
                selectedValue: null,
                dataGroupField: nameof(Occupation.Sector));
            RenderConditionalOccupationFieldIds = occupations
                .Where(o => o.HasFreeTextField)
                .Select(o => o.OccupationId.ToString())
                .ToList();
        }

        protected override async Task<IActionResult> PrepareAndDisplayPageAsync(bool isBeingSubmitted)
        {
            PatientDetails = Notification.PatientDetails;
            await SetNotificationProperties(isBeingSubmitted, PatientDetails);

            await GenerateReferenceDataAsync();
            FormattedDob = PatientDetails.Dob.ConvertToFormattedDate();

            if (PatientDetails.ShouldValidateFull)
            {
                TryValidateModel(PatientDetails, "PatientDetails");
            }

            DuplicateNhsNumberNotifications = await GenerateDuplicateNhsNumberNotificationUrlsAsync(PatientDetails.NhsNumber, Notification.Group);

            return Page();
        }

        private async Task<Dictionary<string, string>> GenerateDuplicateNhsNumberNotificationUrlsAsync(string nhsNumber, NotificationGroup group)
        {
            // If NhsNumber is empty or does not pass validation - return null
            // Potential duplication of validation here so that both Server and Dynamic/JS routes to warnings
            // can use the same method.
            if (string.IsNullOrEmpty(nhsNumber) || !string.IsNullOrEmpty(
                ValidationService.GetPropertyValidationResult<PatientDetails>("NhsNumber", nhsNumber, false).Content))
            {
                return null;
            }

            var notificationIds = await NotificationRepository.GetNotificationIdsByNhsNumberAsync(nhsNumber);
            var idsInGroup = group?.Notifications?.Select(n => n.NotificationId) ?? new List<int>();
            var filteredIds = notificationIds
                .Except(idsInGroup)
                .Where(n => n != NotificationId)
                .ToDictionary(
                    id => id.ToString(),
                    id => RouteHelper.GetNotificationPath(id, NotificationSubPaths.Overview));

            return filteredIds;
        }

        protected override async Task ValidateAndSave()
        {
            await Service.UpdatePatientFlagsAsync(PatientDetails);
            // Remove already invalidated states from modelState as rely
            // on changes made in UpdatePatientFlags
            ModelState.ClearValidationState("PatientDetails.Postcode");
            ModelState.ClearValidationState("PatientDetails.NHSNumber");
            ModelState.ClearValidationState("PatientDetails.OccupationOther");
            if (PatientDetails.UkBorn != false)
            {
                ModelState.ClearValidationState("PatientDetails.YearOfUkEntry");
            }

            PatientDetails.SetValidationContext(Notification);
            await FindAndSetPostcodeAsync();

            ValidationService.TrySetFormattedDate(PatientDetails, "Patient", nameof(PatientDetails.Dob), FormattedDob);

            if (TryValidateModel(PatientDetails, nameof(PatientDetails)))
            {
                await Service.UpdatePatientDetailsAsync(Notification, PatientDetails);
            }
        }

        private async Task FindAndSetPostcodeAsync()
        {
            await FindAndSetPostcodeAsync(_postcodeService, PatientDetails);
        }

        public async Task<ContentResult> OnGetValidatePostcode(string postcode, bool shouldValidateFull)
        {
            var notification = await NotificationRepository.GetNotificationAsync(NotificationId);
            var foundPostcode = await _postcodeService.FindPostcodeAsync(postcode);
            var propertyValueTuples = new List<(string, object)>
            {
                ("PostcodeToLookup", foundPostcode?.Postcode),
                ("Postcode", postcode)
            };
            return ValidationService.GetMultiplePropertiesValidationResult<PatientDetails>(
                propertyValueTuples,
                shouldValidateFull,
                notification.IsLegacy);
        }

        protected override IActionResult RedirectForDraft(bool isBeingSubmitted)
        {
            return RedirectToPage("./HospitalDetails", new { NotificationId, isBeingSubmitted });
        }

        public ContentResult OnGetValidatePatientDetailsProperty(string key, string value, bool shouldValidateFull)
        {
            return ValidationService.GetPropertyValidationResult<PatientDetails>(key, value, shouldValidateFull);
        }

        public async Task<ContentResult> OnGetValidatePatientDetailsDate(string key, string day, string month, string year)
        {
            var isLegacy = await NotificationRepository.IsNotificationLegacyAsync(NotificationId);
            return ValidationService.GetDateValidationResult<PatientDetails>(key, day, month, year, isLegacy);
        }

        public async Task<JsonResult> OnGetNhsNumberDuplicates(int notificationId, string nhsNumber)
        {
            var group = await NotificationRepository.GetNotificationGroupAsync(notificationId);
            return new JsonResult(await GenerateDuplicateNhsNumberNotificationUrlsAsync(nhsNumber, group));
        }
    }
}
