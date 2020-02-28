﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ntbs_service.DataAccess;
using ntbs_service.Models.Entities;
using ntbs_service.Models.ReferenceEntities;
using ntbs_service.Services;

namespace ntbs_service.Pages.Notifications.Edit.Items
{
    public class MBovisAnimalExposureModel : NotificationEditModelBase
    {
        private readonly IItemRepository<MBovisAnimalExposure> _mBovisAnimalExposureRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        [BindProperty(SupportsGet = true)] 
        public int? RowId { get; set; }

        [BindProperty] 
        public MBovisAnimalExposure MBovisAnimalExposure { get; set; }

        public SelectList Countries { get; set; }

        public MBovisAnimalExposureModel(
            INotificationService service,
            IAuthorizationService authorizationService,
            INotificationRepository notificationRepository,
            IReferenceDataRepository referenceDataRepository,
            IItemRepository<MBovisAnimalExposure> mBovisAnimalExposureRepository) : base(service, authorizationService, notificationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _mBovisAnimalExposureRepository = mBovisAnimalExposureRepository;

            Countries = new SelectList(
                _referenceDataRepository.GetAllCountriesAsync().Result,
                nameof(Country.CountryId),
                nameof(Country.Name));
        }

        // Pragma disabled 'not using async' to allow auto-magical wrapping in a Task
#pragma warning disable 1998
        protected override async Task<IActionResult> PrepareAndDisplayPageAsync(bool isBeingSubmitted)
#pragma warning restore 1998
        {
            if (!Notification.IsMBovis)
            {
                return NotFound();
            }
            
            if (RowId == null)
            {
                return Page();
            }

            MBovisAnimalExposure = Notification.MBovisDetails.MBovisAnimalExposures
                .SingleOrDefault(m => m.MBovisAnimalExposureId == RowId);
            if (MBovisAnimalExposure == null)
            {
                return NotFound();
            }

            return Page();
        }

        protected override async Task ValidateAndSave()
        {
            MBovisAnimalExposure.SetValidationContext(Notification);
            MBovisAnimalExposure.NotificationId = NotificationId;
            MBovisAnimalExposure.DobYear = Notification.PatientDetails.Dob?.Year;

            if (TryValidateModel(MBovisAnimalExposure, nameof(MBovisAnimalExposure)))
            {
                if (RowId == null)
                {
                    await _mBovisAnimalExposureRepository.AddAsync(MBovisAnimalExposure);
                }
                else
                {
                    MBovisAnimalExposure.MBovisAnimalExposureId = RowId.Value;
                    await _mBovisAnimalExposureRepository.UpdateAsync(Notification, MBovisAnimalExposure);
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            Notification = await GetNotificationAsync(NotificationId);
            if (await AuthorizationService.GetPermissionLevelForNotificationAsync(User, Notification) != Models.Enums.PermissionLevel.Edit)
            {
                return ForbiddenResult();
            }

            var mBovisAnimalExposure = Notification.MBovisDetails.MBovisAnimalExposures
                .SingleOrDefault(m => m.MBovisAnimalExposureId == RowId);
            if (mBovisAnimalExposure == null)
            {
                return NotFound();
            }

            await _mBovisAnimalExposureRepository.DeleteAsync(mBovisAnimalExposure);

            return RedirectToPage("/Notifications/Edit/MBovisAnimalExposures", new {NotificationId});
        }

        public ContentResult OnGetValidateMBovisAnimalExposureProperty(string key, string value, bool shouldValidateFull)
        {
            return ValidationService.GetPropertyValidationResult<MBovisAnimalExposure>(key, value, shouldValidateFull);
        }

        protected override IActionResult RedirectAfterSaveForNotified()
        {
            return RedirectToPage("/Notifications/Edit/MBovisAnimalExposures", new {NotificationId});
        }

        protected override IActionResult RedirectAfterSaveForDraft(bool isBeingSubmitted)
        {
            return RedirectToPage("/Notifications/Edit/MBovisAnimalExposures", new {NotificationId});
        }

        protected override async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await NotificationRepository.GetNotificationWithMBovisAnimalExposuresAsync(notificationId);
        }
    }
}