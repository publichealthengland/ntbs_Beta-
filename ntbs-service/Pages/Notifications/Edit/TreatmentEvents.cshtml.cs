﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ntbs_service.DataAccess;
using ntbs_service.Helpers;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using ntbs_service.Services;

namespace ntbs_service.Pages.Notifications.Edit
{
    public class TreatmentEventsModel : NotificationEditModelBase
    {
        public ICollection<TreatmentEvent> TreatmentEvents { get; set; }

        public TreatmentEventsModel(
            INotificationService notificationService,
            IAuthorizationService authorizationService,
            INotificationRepository notificationRepository) : base(notificationService, authorizationService, notificationRepository)
        {
            CurrentPage = NotificationSubPaths.EditTreatmentEvents;
        }

        protected override async Task<IActionResult> PrepareAndDisplayPageAsync(bool isBeingSubmitted)
        {
            // Page is not accessible in draft state
            if (Notification.NotificationStatus == NotificationStatus.Draft)
            {
                return NotFound();
            }

            TreatmentEvents = Notification.TreatmentEvents;
            await SetNotificationProperties(isBeingSubmitted);

            return Page();
        }

        protected override IActionResult RedirectToCreate()
        {
            return RedirectToPage("./Items/NewTreatmentEvent", new { NotificationId });
        }

#pragma warning disable 1998
        protected override async Task ValidateAndSave()
        {
            // No validation or saving happening on list
        }
#pragma warning restore 1998


        protected override async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await NotificationRepository.GetNotificationWithTreatmentEventsAsync(notificationId);
        }

        protected override IActionResult RedirectAfterSaveForDraft(bool isBeingSubmitted)
        {
            // Page is not accessible in draft state
            throw new NotImplementedException();
        }
    }
}