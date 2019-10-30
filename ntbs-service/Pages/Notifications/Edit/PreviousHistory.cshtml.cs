﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ntbs_service.Models;
using ntbs_service.Pages_Notifications;
using ntbs_service.Services;

namespace ntbs_service.Pages.Notifications.Edit
{
    public class PreviousHistoryModel : NotificationEditModelBase
    {

        public PreviousHistoryModel(INotificationService service, IAuthorizationService authorizationService) : base(service, authorizationService) {}

        [BindProperty]
        public PatientTBHistory PatientTBHistory { get; set; }

        protected override async Task<IActionResult> PreparePageForGet(int id, bool isBeingSubmitted)
        {
            PatientTBHistory = Notification.PatientTBHistory;
            await SetNotificationProperties(isBeingSubmitted, PatientTBHistory);

            if (PatientTBHistory.ShouldValidateFull)
            {
                TryValidateModel(PatientTBHistory, PatientTBHistory.GetType().Name);
            }

            return Page();
        }

        protected override IActionResult RedirectToNextPage(int notificationId, bool isBeingSubmitted)
        {
            // This is the last page in the flow, so there's no next page to go to
            return RedirectToPage("./PreviousHistory", new { id = notificationId, isBeingSubmitted });
        }

        protected override async Task ValidateAndSave()
        {
            UpdateFlags();
            PatientTBHistory.SetFullValidation(Notification.NotificationStatus);

            if (TryValidateModel(PatientTBHistory.GetType().Name))
            {
                await service.UpdatePatientTBHistoryAsync(Notification, PatientTBHistory);
            }
        }

        private void UpdateFlags()
        {
            if (PatientTBHistory.NotPreviouslyHadTB == true)
            {
                PatientTBHistory.PreviousTBDiagnosisYear = null;
                ModelState.Remove("PatientTBHistory.PreviousTBDiagnosisYear");
            }
        }

        public ContentResult OnGetValidatePreviousHistoryProperty(string key, string value, bool shouldValidateFull)
        {
            return validationService.ValidateModelProperty<PatientTBHistory>(key, value, shouldValidateFull);
        }
    }
}
