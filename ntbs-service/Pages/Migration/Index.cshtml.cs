using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ntbs_service.DataMigration;
using ntbs_service.Models;
using ntbs_service.Models.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using ntbs_service.Services;
using System.ComponentModel.DataAnnotations;
using System;
using Hangfire;
using Microsoft.Extensions.Options;
using ntbs_service.Properties;

namespace ntbs_service.Pages.Migration
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly MigrationConfig _config;

        public IndexModel(IOptions<MigrationConfig> config)
        {
            _config = config.Value;
            ValidationService = new ValidationService(this);
        }

        [BindProperty]
        public string NotificationId { get; set; }

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        [BindProperty]
        [Display(Name = "Start Notification Date")]
        public PartialDate NotificationDateRangeStart { get; set; }


        [BindProperty]
        [Display(Name = "End Notification Date")]
        public PartialDate NotificationDateRangeEnd { get; set; }

        public IList<Notification> Results { get; set; } = new List<Notification>();
        public ValidationService ValidationService { get; }

        public IActionResult OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var requestId = HttpContext.TraceIdentifier;

            if (UploadedFile != null)
            {
                var notificationIds = await GetIdListFromFile(UploadedFile);
                var IdBatches = splitList(notificationIds);

                foreach (var IdBatch in IdBatches)
                {
                    BackgroundJob.Enqueue<INotificationImportService>(x => x.ImportByLegacyIdsAsync(null, requestId, IdBatch));
                }
            }
            else if (NotificationDateRangeStart != null)
            {
                NotificationDateRangeStart.TryConvertToDateTimeRange(out DateTime? notificationDateRangeStart, out _);
                NotificationDateRangeEnd.TryConvertToDateTimeRange(out DateTime? notificationDateRangeEnd, out _);

                var rangeEnd = notificationDateRangeEnd ?? DateTime.Now;

                for (var dateRangeStart = (DateTime)notificationDateRangeStart;
                    dateRangeStart <= rangeEnd;
                    dateRangeStart = dateRangeStart.AddMonths(_config.DateRangeJobIntervalInMonths))
                {
                    var start = dateRangeStart;
                    var end = dateRangeStart.AddMonths(_config.DateRangeJobIntervalInMonths);
                    if (end > rangeEnd)
                    {
                        end = rangeEnd;
                    }

                    BackgroundJob.Enqueue<INotificationImportService>(x =>
                        x.ImportByDateAsync(null, requestId, start, end));
                }
            }

            return Page();
        }

        private async Task<List<string>> GetIdListFromFile(IFormFile file)
        {
            var legacyIds = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    legacyIds.Add(await reader.ReadLineAsync());
            }
            return legacyIds;
        }

        public static IEnumerable<List<T>> splitList<T>(List<T> legacyIds, int nSize = 1000)
        {
            for (int index = 0; index < legacyIds.Count; index += nSize)
            {
                yield return legacyIds.GetRange(index, Math.Min(nSize, legacyIds.Count - index));
            }
        }
    }
}
