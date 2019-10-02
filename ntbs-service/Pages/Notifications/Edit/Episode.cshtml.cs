using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ntbs_service.Models;
using ntbs_service.Services;

namespace ntbs_service.Pages_Notifications
{
    public class EpisodeModel : NotificationModelBase
    {
        private readonly NtbsContext context;
        private readonly IAuditService auditService;
        
        public SelectList TBServices { get; set; }
        public SelectList Hospitals { get; set; }


        [BindProperty]
        public Episode Episode { get; set; }

        public EpisodeModel(INotificationService service, NtbsContext context, IAuditService auditService) : base(service)
        {
            this.context = context;
            this.auditService = auditService;
        }

        public override async Task<IActionResult> OnGetAsync(int? id, bool isBeingSubmitted)
        {
            Notification = await service.GetNotificationAsync(id);
            if (Notification == null)
            {
                return NotFound();
            }


            Episode = Notification.Episode;
            if (Episode == null)
            {
                return NotFound();
            }

            SetNotificationProperties<Episode>(isBeingSubmitted, Episode);
            if (Episode.ShouldValidateFull)
            {
                TryValidateModel(Episode, Episode.GetType().Name);
            }

            TBServices = new SelectList(context.GetAllTbServicesAsync().Result, 
                                        nameof(TBService.Code), 
                                        nameof(TBService.Name));

            Hospitals = new SelectList(context.GetAllHospitalsAsync().Result, 
                                        nameof(Hospital.HospitalId), 
                                        nameof(Hospital.Name));

            await auditService.OnGetAuditAsync(Notification.NotificationId, Episode);
            return Page();
        }

        public JsonResult OnGetHospitalsByTBService(string tbServiceCode) 
        {
            var tbServices = context.GetHospitalsByTBService(tbServiceCode).Result;
            return new JsonResult(tbServices);
        }

        protected override IActionResult RedirectToNextPage(int? notificationId)
        {
            return RedirectToPage("./ClinicalDetails", new {id = notificationId});
        }

        protected override async Task<bool> ValidateAndSave() {
    
            if (!ModelState.IsValid)
            {
                return false;
            }

            await service.UpdateEpisodeAsync(Notification, Episode);
            return true;
        }

        public ContentResult OnGetValidateEpisodeProperty(string key, string value)
        {
            return ValidateProperty(new Episode(), key, value);
        }
    }
}