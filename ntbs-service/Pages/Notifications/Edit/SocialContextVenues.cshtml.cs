﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ntbs_service.DataAccess;
using ntbs_service.Helpers;
using ntbs_service.Models.Entities;
using ntbs_service.Services;

namespace ntbs_service.Pages.Notifications.Edit
{
    public class SocialContextVenuesModel : NotificationEditModelBase
    {
        public ICollection<SocialContextVenue> SocialContextVenues { get; set; }

        public SocialContextVenuesModel(
            INotificationService service,
            IAuthorizationService authorizationService,
            INotificationRepository notificationRepository,
            IAlertRepository alertRepository,
            IUserHelper userHelper) : base(service, authorizationService, notificationRepository, alertRepository, userHelper)
        {
            CurrentPage = NotificationSubPaths.EditSocialContextVenues;
        }

        protected override async Task<IActionResult> PrepareAndDisplayPageAsync(bool isBeingSubmitted)
        {
            SocialContextVenues = Notification.SocialContextVenues;
            await SetNotificationProperties(isBeingSubmitted);

            return Page();
        }

#pragma warning disable 1998
        protected override async Task ValidateAndSave()
        {
            // No validation or saving happening on list
        }
#pragma warning restore 1998

        protected override IActionResult RedirectToCreate()
        {
            return RedirectToPage("./Items/NewSocialContextVenue", "", new { NotificationId }, "social-context-venue-form");
        }

        protected override IActionResult RedirectForDraft(bool isBeingSubmitted)
        {
            return RedirectToPage("./PreviousHistory", new { NotificationId, isBeingSubmitted });
        }

        protected override async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await NotificationRepository.GetNotificationWithSocialContextVenuesAsync(notificationId);
        }
    }
}
