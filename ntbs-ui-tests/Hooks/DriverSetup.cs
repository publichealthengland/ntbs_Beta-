using System.Data.SqlClient;
using System.Threading.Tasks;
using BoDi;
using Dapper;
using ntbs_service;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace ntbs_ui_tests.Hooks
{
    [Binding]
    public class DriverSetup
    {
        private readonly IObjectContainer objectContainer;
        public readonly TestContext testContext;
        public IWebDriver Browser;
        public TestConfig settings;

        public DriverSetup(
            IObjectContainer objectContainer,
            TestConfig settings,
            TestContext testContext)
        {
            this.objectContainer = objectContainer;
            this.settings = settings;
            this.testContext = testContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            var opts = new ChromeOptions();
            opts.AddArgument("--no-sandbox"); // Necessary to avoid `unknown error: DevToolsActivePort file doesn't exist` when running on docker
            if (settings.IsHeadless)
            {
                opts.AddArgument("--headless");
            }

            Browser = new RemoteWebDriver(opts);
            Browser.Manage().Timeouts().ImplicitWait = settings.ImplicitWait;
            objectContainer.RegisterInstanceAs(Browser);
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            using (var connection = new SqlConnection(settings.EnvironmentConfig.ConnectionString))
            {
                connection.Open();
                var deleteAlerts = "DELETE FROM Alert WHERE NotificationId IN @ids";
                var deleteNotifications = "DELETE FROM Notification WHERE NotificationId IN @ids";
                await connection.ExecuteAsync(deleteAlerts, new { ids = testContext.AddedNotificationIds.ToArray() });
                await connection.ExecuteAsync(deleteNotifications, new { ids = testContext.AddedNotificationIds.ToArray() });
            }
            Browser.Quit();
        }

        [Scope(Feature = "Import legacy notification")]
        [BeforeScenario]
        private async Task RemoveImportedNotificationBefore()
        {
            await CleanUpMigratedNotification();
        }

        [Scope(Feature = "Import legacy notification")]
        [AfterScenario]
        private async Task RemoveImportedNotificationAfter()
        {
            await CleanUpMigratedNotification();
        }

        private async Task CleanUpMigratedNotification()
        {
            await using (var connection = new SqlConnection(settings.EnvironmentConfig.ConnectionString))
            {
                connection.Open();
                var deleteNotification = "DELETE FROM Notification WHERE ETSID = '189045'";
                await connection.ExecuteAsync(deleteNotification);
                connection.Close();
            }
            await using (var connection = new SqlConnection(settings.EnvironmentConfig.MigrationConnectionString))
            {
                connection.Open();
                var deleteImportedNotification = $"DELETE FROM {settings.EnvironmentConfig.ImportedNotificationTableName} WHERE LegacyId = '189045'";
                await connection.ExecuteAsync(deleteImportedNotification);
            }
        }
    }
}
