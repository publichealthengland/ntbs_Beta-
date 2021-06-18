using System;
using Hangfire;
using ntbs_service.Properties;

namespace ntbs_service.Jobs
{
    internal static class HangfireJobScheduler
    {
        private const string UserSyncJobId = "user-sync";
        private const string CloseInactiveNotificationsJobId = "close-inactive-notifications";
        private const string DrugResistanceProfileUpdateJobId = "drug-resistance-profile-update";
        private const string UnmatchedLabResultAlertsJobId = "unmatched-lab-result-alerts";
        private const string DataQualityAlertsJobId = "data-quality-alerts";
        private const string NotificationClusterUpdateJobId = "notification-cluster-update";
        private const string MarkImportedNotificationsAsImportedJobId = "mark-notifications-as-imported";
        private const string GenericStoredProcedureJobId = "generic-stored-procedure-execution";
        private const string GenerateReportingDataJobId = "generate-report-data";
        private const string ReportingDataRefreshJobId = "reporting-data-refresh";
        private const string ReportingDataProcessingJobId = "reporting-data-processing";

        public static void ScheduleRecurringJobs(ScheduledJobsConfig scheduledJobsConfig)
        {
            if (scheduledJobsConfig.UserSyncEnabled)
            {
                RecurringJob.AddOrUpdate<UserSyncJob>(
                    UserSyncJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.UserSyncCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(UserSyncJobId);
            }

            if (scheduledJobsConfig.CloseInactiveNotificationsEnabled)
            {
                RecurringJob.AddOrUpdate<CloseInactiveNotificationsJob>(
                    CloseInactiveNotificationsJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.CloseInactiveNotificationsCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(CloseInactiveNotificationsJobId);
            }

            if (scheduledJobsConfig.DrugResistanceProfileUpdateEnabled)
            {
                RecurringJob.AddOrUpdate<DrugResistanceProfileUpdateJob>(
                    DrugResistanceProfileUpdateJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.DrugResistanceProfileUpdateCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(DrugResistanceProfileUpdateJobId);
            }

            if (scheduledJobsConfig.UnmatchedLabResultAlertsEnabled)
            {
                RecurringJob.AddOrUpdate<UnmatchedLabResultAlertsJob>(
                    UnmatchedLabResultAlertsJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.UnmatchedLabResultAlertsCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(UnmatchedLabResultAlertsJobId);
            }

            if (scheduledJobsConfig.DataQualityAlertsEnabled)
            {
                RecurringJob.AddOrUpdate<DataQualityAlertsJob>(
                    DataQualityAlertsJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.DataQualityAlertsCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(DataQualityAlertsJobId);
            }

            if (scheduledJobsConfig.NotificationClusterUpdateEnabled)
            {
                RecurringJob.AddOrUpdate<NotificationClusterUpdateJob>(
                    NotificationClusterUpdateJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.NotificationClusterUpdateCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(NotificationClusterUpdateJobId);
            }

            if (scheduledJobsConfig.MarkImportedNotificationsAsImportedEnabled)
            {
                RecurringJob.AddOrUpdate<MarkImportedNotificationsAsImportedJob>(
                    MarkImportedNotificationsAsImportedJobId,
                    job => job.Run(JobCancellationToken.Null),
                    scheduledJobsConfig.MarkImportedNotificationsAsImportedCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(MarkImportedNotificationsAsImportedJobId);
            }

            if (scheduledJobsConfig.GenerateReportingDataJobEnabled)
            {
                // PerformContext context is passed in via Hangfire Server
                RecurringJob.AddOrUpdate<GenerateReportingDataJob>(
                    GenerateReportingDataJobId,
                    job => job.Run(null, JobCancellationToken.Null),
                    scheduledJobsConfig.GenerateReportingDataJobCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(GenerateReportingDataJobId);
            }

            if (scheduledJobsConfig.GenericStoredProcedureJobEnabled)
            {
                // PerformContext context is passed in via Hangfire Server
                RecurringJob.AddOrUpdate<GenericStoredProcedureJob>(
                    GenericStoredProcedureJobId,
                    job => job.Run(null, JobCancellationToken.Null),
                    scheduledJobsConfig.GenericStoredProcedureJobCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(GenericStoredProcedureJobId);
            }

            if (scheduledJobsConfig.ReportingDataRefreshJobEnabled)
            {
                // PerformContext context is passed in via Hangfire Server
                RecurringJob.AddOrUpdate<ReportingDataRefreshJob>(
                    ReportingDataRefreshJobId,
                    job => job.Run(null, JobCancellationToken.Null),
                    scheduledJobsConfig.ReportingDataRefreshJobCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(ReportingDataRefreshJobId);
            }

            if (scheduledJobsConfig.ReportingDataProcessingJobEnabled)
            {
                // PerformContext context is passed in via Hangfire Server
                RecurringJob.AddOrUpdate<ReportingDataProcessingJob>(
                    ReportingDataProcessingJobId,
                    job => job.Run(null, JobCancellationToken.Null),
                    scheduledJobsConfig.ReportingDataProcessingJobCron,
                    TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(ReportingDataProcessingJobId);
            }

            RecurringJob.AddOrUpdate<TestJob>(
                "test",
                job => job.Run(JobCancellationToken.Null),
                "0 23 * * *",
                TimeZoneInfo.Local);
        }
    }
}
