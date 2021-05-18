using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ntbs_service.Models.Validations;
using ntbs_ui_tests.Helpers;
using ntbs_ui_tests.Hooks;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace ntbs_ui_tests.Steps
{
    [Binding]
    public class AssertSteps
    {
        private readonly IWebDriver Browser;
        private readonly TestConfig Settings;
        private readonly TestContext TestContext;

        public AssertSteps(IWebDriver driver, TestConfig settings, TestContext testContext)
        {
            Browser = driver;
            Settings = settings;
            TestContext = testContext;
        }

        #region Location checks

        [Then(@"I should be on the (.*) page")]
        public void ThenIShouldBeOnPage(string pageName)
        {
            // Remove any query string parameters
            Assert.Equal(pageName, Browser.Url.Split('/').Last().Split('?').First());
            // Wait for everything to load
            if (!Settings.IsHeadless)
            {
                Thread.Sleep(2000);
            }
        }

        [Then(@"I should be on the Homepage")]
        public void ThenIShouldBeOnTheHomepage()
        {
            Assert.Equal($"{Settings.EnvironmentConfig.RootUri}/", Browser.Url);
        }

        [Then(@"I should see the Notification")]
        public void ThenIShouldSeeTheNotification()
        {
            var urlRegex = new Regex(@".*/Notifications/(\d+)/?(#.+)?$");
            var match = urlRegex.Match(Browser.Url);
            Assert.True(match.Success, $"Url I am on instead: {Browser.Url}");
        }

        #endregion

        #region Notificaion edit pages

        [Then(@"I should see all submission error messages")]
        public void ThenIShouldSeeAllSubmissionErrorMessages()
        {
            var summaryText = Browser.FindElement(By.ClassName("nhsuk-error-summary")).Text;
            Assert.Contains("Notification date is a mandatory field", summaryText);
            Assert.Contains("Hospital is a mandatory field", summaryText);
            Assert.Contains(ValidationMessages.DiseaseSiteIsRequired, summaryText);
            Assert.Contains("Diagnosis date is a mandatory field", summaryText);
            Assert.Contains("Date of birth is a mandatory field", summaryText);
            Assert.Contains("Sex is a mandatory field", summaryText);
            Assert.Contains("Postcode is a mandatory field", summaryText);
            Assert.Contains("Birth country is a mandatory field", summaryText);
            Assert.Contains("Given name is a mandatory field", summaryText);
            Assert.Contains("NHS number is a mandatory field", summaryText);
            Assert.Contains("Family name is a mandatory field", summaryText);
            Assert.Contains("Ethnic group is a mandatory field", summaryText);
        }

        [Then(@"I can see the error '(.*)'")]
        public void ThenICanSeeTheError(string errorMessage)
        {
            var errorSection = HtmlElementHelper.FindElementByXpath(Browser, "//*[@class='nhsuk-error-summary']");
            Assert.Contains(errorMessage, errorSection.Text);
        }

        [Then(@"I see the warning '(.*)' for '(.*)'")]
        public void ThenICanSeeTheWarningForId(string warningMessage, string warningId)
        {
            var warningElement = HtmlElementHelper.FindElementById(Browser, warningId);
            Assert.Contains(warningMessage, warningElement.Text);
        }
        
        #endregion

        #region Transfer pages

        [Then(@"I can see '(.*)' as the rejection note")]
        public void ThenICanSeeRejectionNote(string rejectionNote)
        {
            var noteOnPage = Browser.FindElement(By.ClassName("rejection-note"));
            Assert.Contains(rejectionNote, noteOnPage.Text);
        }

        [Then(@"I can see the value (.*) for '(.*)' transfer information")]
        public void ThenICanSeeTransferInformationValue(string value, string title)
        {
            var transferInformationElements = Browser.FindElements(By.ClassName("transfer-request-information"));
            Assert.Contains(transferInformationElements, t => t.Text.Contains(title) && t.Text.Contains(value));
        }
        
        #endregion

        #region Notification overview

        [Then(@"I can see the correct titles for the '(.*)' table")]
        public void ThenICanSeeCorrectTitlesForTable(string tableId)
        {
            var tableColumnTitles = HtmlElementHelper.FindElementById(Browser, tableId).FindElement(By.TagName("thead"))
                .FindElements(By.TagName("th")).Select(title => title.Text).ToArray();
            var expectedTitles = GetExpectedColumnsTitlesForTable(tableId);
            Assert.Equal(tableColumnTitles.Select(titles => titles.Replace("\n", "").Replace("\r", "")), expectedTitles);
        }

        [Then(@"I can see the correct labels for the '(.*)' overview section")]
        public void ThenICanSeeTheCorrectLabelsForOverviewSection(string section)
        {
            var sectionId = HtmlElementHelper.GetSectionIdFromSection(section);
            var expectedLabels = GetExpectedLabels(section);
            var actualLabels = HtmlElementHelper.FindElementById(Browser, sectionId).FindElements(By.ClassName("notification-details-label"))
                .Select(label => label.Text).ToArray();
            Assert.Equal(expectedLabels, actualLabels);
        }

        [Then(@"A (.*) alert is present on the notification")]
        public void ThenAlertIsPresent(string alertTitle)
        {
            var alerts = HtmlElementHelper.FindElementsByXpath(Browser, "//*[contains(@id, 'alert-')]");
            Assert.NotNull(alerts.SingleOrDefault(a => a.Text.Contains(alertTitle)));
        }

        [Then(@"I can see the value '(.*)' for the field '(.*)' in the '(.*)' overview section")]
        public void ThenICanSeeValueForFieldInTheOverviewSection(string value, string field, string section)
        {
            var sectionId =HtmlElementHelper.GetSectionIdFromSection(section);
            var htmlId = $"{sectionId}-{field}";
            Assert.Contains(value, HtmlElementHelper.FindElementById(Browser, htmlId).Text);
        }

        [Then(@"I can see the value '(.*)' in the '(.*)' table overview section")]
        public void ThenICanSeeValueInTheOverviewSection(string value, string section)
        {
            var sectionId =HtmlElementHelper.GetSectionIdFromSection(section);
            Assert.Contains(value, HtmlElementHelper.FindElementById(Browser, sectionId).Text);
        }

        [Then(@"The notification should be denotified")]
        public void ThenCurrentNotificationShouldBeDenotified()
        {
            var notificationHeading = Browser.FindElement(By.TagName("h1")).Text;
            Assert.Contains("Denotified", notificationHeading);
        }

        #endregion

        #region General checks

        [Then(@"I can see the value (.*) for element with id '(.*)'")]
        public void ThenICanSeeValueForId(string value, string elementId)
        {
            Assert.Contains(value, HtmlElementHelper.FindElementById(Browser, elementId).Text);
        }

        [Then(@"The element with id '(.*)' is not present")]
        public void ThenICannotSeeTheElement(string elementId)
        {
            var elements = HtmlElementHelper.FindElementsById(Browser, elementId);
            Assert.False(elements.Any());
        }

        [Then("A new notification should have been created")]
        public void ThenNotificationCreated()
        {
            var notificationId = GetNotificationIdAndAssertMatchFromUrl();
            Assert.DoesNotContain(notificationId, TestContext.AddedNotificationIds);
            TestContext.AddedNotificationIds.Add(notificationId);
        }
        
        #endregion

        private int GetNotificationIdAndAssertMatchFromUrl()
        {
            var urlRegex = new Regex(@".*/Notifications/(\d+).*$");
            var match = urlRegex.Match(Browser.Url);
            var idString = match.Groups[1].Value;
            Assert.True(match.Success, $"Url I am on instead: {Browser.Url}");
            return int.Parse(idString);
        }

        private string[] GetExpectedColumnsTitlesForTable(string tableId)
        {
            return tableId switch
            {
                "alerts-table" => new[] {"NTBS Id", "Alert date", "Alert type", "Case ManagerTB Service", "Dismiss"},
                "draft-notifications-table" => new [] {"NTBS Id", "Name", "Date created", "TB Service", "Case Manager"},
                "recent-notifications-table" => new [] {"NTBS Id", "Name", "Date notified", "TB Service", "Case Manager"}
            };
        }

        private string[] GetExpectedLabels(string tableId)
        {
            return tableId switch
            {
                "PatientDetails" => new[] {"Name", "Sex", "NHS number", "Occupation", "Notification date",
                    "Date of birth", "Age at notification", "Address", "Postcode", "Local authority", "Residence PHEC",
                    "Ethnic group", "Birth country", "UK entry year", "Treatment PHEC", "Legacy IDs", "Local patient ID"},
                "HospitalDetails" => new [] {"TB service", "Hospital", "Case Manager", "Consultant"},
                "ClinicalDetails" => new [] { "Sites of disease", "Patient has symptoms", "Symptom onset date",
                    "Symptom onset to treatment start", "Presentation to any health service date",
                    "Symptom onset to health service presentation", "Healthcare setting", "Presentation to TB service date",
                    "Health service to TB service presentation", "Diagnosis date", "TB service presentation to diagnosis",
                    "Treatment start date", "Diagnosis to treatment start", "Postmortem diagnosis", "Home visit",
                    "HIV test offered", "DOT offered", "BCG vaccination", "Enhanced case management",
                    "Planned treatment regimen", "Notes"},
                "ContactTracing" => new [] {"Number of contacts identified for screening",
                    "Number of contacts screened and found to have latent TB", "Number of contacts screened for TB",
                    "Number of contacts that have started treatment for latent TB",
                    "Number of contacts screened and found to have active TB",
                    "Number of contacts that have completed treatment for latent TB"},
                "SocialRiskFactors" => new [] {"History of drug misuse", "Time periods", "History of homelessness",
                    "Time periods", "History of imprisonment", "Time periods", "History of smoking", "Time periods",
                    "Is the patient’s ability to self-administer treatment affected by alcohol misuse or abuse?",
                    "Is the patient’s ability to self-administer treatment affected by mental health illness?",
                    "Is the patient an asylum seeker?", "Is the patient an immigration removal centre detainee?"},
                "Travel" => new [] {
                    "Patient has travelled to one or more high incidence countries", "Total countries travelled to",
                    "Total number of months travelled", "Country 1", "Country 2", "Country 3",
                    "Patient has had visitors from one or more high incidence countries", "Total visitor countries",
                    "Total number of months of visits", "Country 1", "Country 2", "Country 3"},
                "Comorbidities" => new [] {"Diabetes", "Hepatitis B", "Hepatitis C", "Chronic liver disease",
                    "Chronic renal disease", "Immunosuppression", "Type", "Other immunosuppression"},
                "PreviousHistory" => new [] {
                    "Previous TB occurrence", "Year of previous diagnosis", "Previously treated", "Country of treatment"}
            };
        }
    }
}
