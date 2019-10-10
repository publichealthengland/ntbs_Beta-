using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFAuditer;
using Microsoft.EntityFrameworkCore;
using ntbs_service.DataAccess;
using ntbs_service.Models;
using ntbs_service.Models.Enums;

namespace ntbs_service.Services
{
    public interface INotificationService
    {
        IQueryable<Notification> GetBaseNotificationIQueryable();
        IQueryable<Notification> GetBaseQueryableNotificationByStatus(IList<NotificationStatus> statuses);
        Task<IList<Notification>> GetRecentNotificationsAsync(IEnumerable<TBService> TBServices);
        Task<IList<Notification>> GetDraftNotificationsAsync(IEnumerable<TBService> TBServices);
        Task<Notification> GetNotificationAsync(int? id);
        Task<Notification> GetNotificationWithSocialRisksAsync(int? id);
        Task<Notification> GetNotificationWithNotificationSitesAsync(int? id);
        Task<Notification> GetNotificationWithImmunosuppressionDetailsAsync(int? id);
        Task<Notification> GetNotificationWithAllInfoAsync(int? id);
        Task<NotificationGroup> GetNotificationGroupAsync(int id);
        Task UpdatePatientAsync(Notification notification, PatientDetails patientDetails);
        Task UpdateClinicalDetailsAsync(Notification notification, ClinicalDetails timeline);
        Task UpdateSitesAsync(int notificationId, IEnumerable<NotificationSite> notificationSites);
        Task UpdateEpisodeAsync(Notification notification, Episode episode);
        Task SubmitNotification(Notification notification);
        Task UpdateContactTracingAsync(Notification notification, ContactTracing contactTracing);
        Task UpdatePatientTBHistoryAsync(Notification notification, PatientTBHistory history);
        Task UpdateSocialRiskFactorsAsync(Notification notification, SocialRiskFactors riskFactors);
        Task UpdateImmunosuppresionDetailsAsync(Notification notification, ImmunosuppressionDetails immunosuppressionDetails);
        Task<Notification> CreateLinkedNotificationAsync(Notification notification);
        IQueryable<Notification> FilterBySex(IQueryable<Notification> notifications, int sexId);
        IQueryable<Notification> FilterByPartialDate(IQueryable<Notification> notifications, PartialDate partialDate);
        IQueryable<Notification> FilterById(IQueryable<Notification> notifications, string IdFilter);
        IQueryable<Notification> OrderQueryableByNotificationDate(IQueryable<Notification> query);
        Task<IEnumerable<Notification>> GetNotificationsByIdAsync(IList<int> ids);
        Task<IList<T>> GetPaginatedItemsAsync<T>(IQueryable<T> source, PaginationParameters paginationParameters);
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository repository;
        private readonly NtbsContext context;

        public NotificationService(INotificationRepository repository, NtbsContext context) {
            this.repository = repository;
            this.context = context;
        }

        public IQueryable<Notification> GetBaseNotificationIQueryable() {
            return repository.GetBaseNotificationIQueryable();
        }

        public async Task<IList<Notification>> GetRecentNotificationsAsync(IEnumerable<TBService> TBServices) {
            return await repository.GetRecentNotificationsAsync(TBServices);
        }

        public async Task<IList<Notification>> GetDraftNotificationsAsync(IEnumerable<TBService> TBServices) {
            return await repository.GetDraftNotificationsAsync(TBServices);
        }

        public async Task<Notification> GetNotificationAsync(int? id) {
            return await repository.GetNotificationAsync(id);
        }

        public async Task<NotificationGroup> GetNotificationGroupAsync(int id) {
            return await context.NotificationGroup.Include(n => n.Notifications)
                .FirstOrDefaultAsync(n => n.NotificationGroupId == id);
        }

        public async Task UpdatePatientAsync(Notification notification, PatientDetails patient)
        {
            await UpdatePatientFlags(patient);
            context.Entry(notification.PatientDetails).CurrentValues.SetValues(patient);

            await UpdateDatabase();
        }

        private async Task UpdatePatientFlags(PatientDetails patient)
        {
            await UpdateUkBorn(patient);
        }

        private async Task UpdateUkBorn(PatientDetails patient)
        {
            var country = await context.GetCountryByIdAsync(patient.CountryId);
            if (country == null) {
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

        public async Task UpdateClinicalDetailsAsync(Notification notification, ClinicalDetails clinicalDetails)
        {
            context.Entry(notification.ClinicalDetails).CurrentValues.SetValues(clinicalDetails);

            await UpdateDatabase();
        }

        public async Task UpdateEpisodeAsync(Notification notification, Episode episode)
        {
            context.Entry(notification.Episode).CurrentValues.SetValues(episode);

            await UpdateDatabase();
        }

        public async Task UpdateContactTracingAsync(Notification notification, ContactTracing contactTracing) {
            context.Entry(notification.ContactTracing).CurrentValues.SetValues(contactTracing);

            await UpdateDatabase();
        }

        public async Task UpdatePatientTBHistoryAsync(Notification notification, PatientTBHistory tBHistory)
        {
            context.Entry(notification.PatientTBHistory).CurrentValues.SetValues(tBHistory);

            await UpdateDatabase();
        }

        public async Task UpdateSocialRiskFactorsAsync(Notification notification, SocialRiskFactors socialRiskFactors)
        {
            UpdateSocialRiskFactorsFlags(socialRiskFactors);
            context.Entry(notification.SocialRiskFactors).CurrentValues.SetValues(socialRiskFactors);

            context.Entry(notification.SocialRiskFactors.RiskFactorDrugs).CurrentValues.SetValues(socialRiskFactors.RiskFactorDrugs);

            context.Entry(notification.SocialRiskFactors.RiskFactorHomelessness).CurrentValues.SetValues(socialRiskFactors.RiskFactorHomelessness);

            context.Entry(notification.SocialRiskFactors.RiskFactorImprisonment).CurrentValues.SetValues(socialRiskFactors.RiskFactorImprisonment);

            await UpdateDatabase();
        }

        private void UpdateSocialRiskFactorsFlags(SocialRiskFactors socialRiskFactors) 
        {
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorDrugs);
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorHomelessness);
            UpdateRiskFactorFlags(socialRiskFactors.RiskFactorImprisonment);
        }

        private void UpdateRiskFactorFlags(RiskFactorDetails riskFactor)
        {
            if (riskFactor.Status != Status.Yes)
            {
                riskFactor.IsCurrent = false;
                riskFactor.InPastFiveYears = false;
                riskFactor.MoreThanFiveYearsAgo = false;
            }
        }

        public async Task<Notification> GetNotificationWithSocialRisksAsync(int? id)
        {
            return await repository.GetNotificationWithSocialRiskFactorsAsync(id);
        }

        public async Task<Notification> GetNotificationWithNotificationSitesAsync(int? id) 
        {
            return await repository.GetNotificationWithNotificationSitesAsync(id);
        }

        public async Task<Notification> GetNotificationWithImmunosuppressionDetailsAsync(int? id)
        {
            return await repository.GetNotificationWithImmunosuppresionDetailsAsync(id);
        }

        public async Task UpdateImmunosuppresionDetailsAsync(Notification notification, ImmunosuppressionDetails immunosuppressionDetails)
        {
            if (immunosuppressionDetails.Status != Status.Yes)
            {
                immunosuppressionDetails.HasBioTherapy = false;
                immunosuppressionDetails.HasTransplantation = false;
                immunosuppressionDetails.HasOther = false;
                immunosuppressionDetails.OtherDescription = null;
            }

            if (immunosuppressionDetails.HasOther == false)
            {
                immunosuppressionDetails.OtherDescription = null;
            }

            context.Entry(notification.ImmunosuppressionDetails).CurrentValues.SetValues(immunosuppressionDetails);
            await UpdateDatabase();
        }

        public async Task UpdateSitesAsync(int notificationId, IEnumerable<NotificationSite> notificationSites) 
        {
            var currentSites = context.NotificationSite.Where(ns => ns.NotificationId == notificationId);

            foreach (var newSite in notificationSites)
            {
                var existingSite = currentSites.FirstOrDefault(s => s.SiteId == newSite.SiteId);
                if (existingSite == null)
                {
                    context.NotificationSite.Add(newSite);
                } else if (existingSite.SiteDescription != newSite.SiteDescription)
                {
                    existingSite.SiteDescription = newSite.SiteDescription;
                }
            }

            var sitesToRemove = currentSites.Where(s => !notificationSites.Select(ns => ns.SiteId).Contains(s.SiteId));
            context.NotificationSite.RemoveRange(sitesToRemove);

            await UpdateDatabase();
        }

        public async Task SubmitNotification(Notification notification)
        {
            context.Attach(notification);
            notification.NotificationStatus = NotificationStatus.Notified;
            notification.SubmissionDate = DateTime.UtcNow;

            await UpdateDatabase(AuditType.Notified);
        }
        
        public async Task<Notification> GetNotificationWithAllInfoAsync(int? id)
        {
            return await repository.GetNotificationWithAllInfoAsync(id);
        }

        public IQueryable<Notification> GetBaseQueryableNotificationByStatus(IList<NotificationStatus> statuses) {
            return repository.GetBaseQueryableNotificationByStatus(statuses);
        }
        
        public async Task<Notification> CreateLinkedNotificationAsync(Notification notification)
        {
            var linkedNotification = new Notification();
            context.Notification.Add(linkedNotification);
            context.Entry(linkedNotification.PatientDetails).CurrentValues.SetValues(notification.PatientDetails);

            if (notification.GroupId != null)
            {
                linkedNotification.GroupId = notification.GroupId;
            }
            else
            {
                var group = new NotificationGroup();
                context.NotificationGroup.Add(group);

                linkedNotification.GroupId = group.NotificationGroupId;
                notification.GroupId = group.NotificationGroupId;
            }

            await context.SaveChangesAsync();

            return linkedNotification;
        }

        private async Task UpdateDatabase(AuditType auditType = AuditType.Edit)
        {
            context.AddAuditCustomField(CustomFields.AuditDetails, auditType);
            await context.SaveChangesAsync();
        }

        public IQueryable<Notification> FilterById(IQueryable<Notification> notifications, string IdFilter) {
            int parsedIdFilter;
            int.TryParse(IdFilter, out parsedIdFilter);
            return notifications.Where(s => s.NotificationId.Equals(parsedIdFilter) 
                    || s.ETSID.Equals(IdFilter) || s.LTBRID.Equals(IdFilter) || s.PatientDetails.NhsNumber.Equals(IdFilter));
        }

        public IQueryable<Notification> FilterByPartialDate(IQueryable<Notification> notifications, PartialDate partialDate) 
        {
            partialDate.TryConvertToDateTimeRange(out DateTime? dateRangeStart, out DateTime? dateRangeEnd);
            return notifications.Where(s => s.PatientDetails.Dob >= dateRangeStart && s.PatientDetails.Dob < dateRangeEnd);
        }

        public IQueryable<Notification> FilterBySex(IQueryable<Notification> notifications, int sexId) 
        {
            return notifications.Where(s => s.PatientDetails.SexId.Equals(sexId));
        }

        public IQueryable<Notification> OrderQueryableByNotificationDate(IQueryable<Notification> query) 
        {
            return query.OrderByDescending(n => n.CreationDate)
                .OrderByDescending(n => n.SubmissionDate);
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByIdAsync(IList<int> ids)
        {
            return await repository.GetNotificationsByIdsAsync(ids);
        }

        public async Task<IList<T>> GetPaginatedItemsAsync<T>(IQueryable<T> items, PaginationParameters paginationParameters)
        {
            return await items.Skip((paginationParameters.PageIndex - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize).ToListAsync();
        }
    }
}