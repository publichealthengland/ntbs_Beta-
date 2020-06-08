using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using ntbs_service.DataMigration.Exceptions;
using ntbs_service.Models.Entities;

namespace ntbs_service.DataMigration
{
    /// <summary>
    /// A specialized service for marking legacy notifications in the migration database as imported into this
    /// instance of NTBS. This marking is important, as it allows the legacy search code to only surface legacy
    /// notifications that have not been migrated over yet.
    /// </summary>
    public interface IMigratedNotificationsMarker
    {
        /// <summary>
        /// Marks the legacy ids of passed notifications as already imported in the migration db. Typically called right
        /// after the migration of these notifications. Sets import time to now.
        /// </summary>
        /// <param name="notifications">The notifications to be marked as imported</param>
        Task MarkNotificationsAsImportedAsync(ICollection<Notification> notifications);

        /// <summary>
        /// Goes through the notifications held in the NTBS db and ensures they have all been marked as imported. Where
        /// they have been, leaves the import time as originally recorded. Where the import record is missing, adds it
        /// with import time set to now.
        ///
        /// Designed as a fallback when the marking of individual imported notifications fails. To be called after the
        /// completion of mass migration, or as a periodical clean up.
        ///
        /// Requires the NTBS and migration dbs to be hosted in a cross-queryable way.
        /// </summary>
        Task BulkMarkNotificationsAsImported();
    }

    public class MigratedNotificationsMarker : IMigratedNotificationsMarker
    {
        private readonly string _connectionString;
        private readonly INotificationImportHelper _importHelper;

        public MigratedNotificationsMarker(IConfiguration _configuration, INotificationImportHelper importHelper)
        {
            _connectionString = _configuration.GetConnectionString("migration");
            _importHelper = importHelper;
        }

        public async Task MarkNotificationsAsImportedAsync(ICollection<Notification> notifications)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    var importedAt = DateTime.Now.ToString("s");

                    foreach (var notification in notifications) // TODO NTBS-1440 mark both ETS and LTBR ids.
                    {
                        await connection.ExecuteAsync(
                            _importHelper.InsertImportedNotificationQuery,
                            new {notification.LegacyId, ImportedAt = importedAt}
                        );
                    }
                }
                catch (Exception exception)
                {
                    throw new MarkingNotificationsAsImportedFailedException(notifications, exception);
                }
            }
        }

        public async Task BulkMarkNotificationsAsImported()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                await connection.ExecuteAsync(_importHelper.BulkInsertImportedNotificationsQuery);
            }
        }
    }
}
