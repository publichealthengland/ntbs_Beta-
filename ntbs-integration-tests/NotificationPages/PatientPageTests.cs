﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ntbs_integration_tests.Helpers;
using ntbs_service;
using ntbs_service.Helpers;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using Xunit;

namespace ntbs_integration_tests.NotificationPages
{
    public class PatientPageTests : TestRunnerNotificationBase
    {
        protected override string NotificationSubPath => NotificationSubPaths.EditPatientDetails;

        public PatientPageTests(NtbsWebApplicationFactory<Startup> factory) : base(factory) { }

        public static IList<Notification> GetSeedingNotifications()
        {
            return new List<Notification>()
            {
                new Notification
                {
                    NotificationId = Utilities.PATIENT_GROUPED_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                    NotificationStatus = NotificationStatus.Notified,
                    GroupId = Utilities.NOTIFICATION_GROUP_ID,
                    PatientDetails = new PatientDetails
                    {
                        NhsNumberNotKnown = false,
                        NhsNumber = Utilities.NHS_NUMBER_SHARED
                    }
                },
                new Notification
                {
                    NotificationId = Utilities.PATIENT_GROUPED_DENOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                    NotificationStatus = NotificationStatus.Denotified,
                    GroupId = Utilities.NOTIFICATION_GROUP_ID,
                    PatientDetails = new PatientDetails
                    {
                        NhsNumberNotKnown = false,
                        NhsNumber = Utilities.NHS_NUMBER_SHARED
                    }
                },
                new Notification
                {
                    NotificationId = Utilities.PATIENT_DRAFT_NOTIFICATION_SHARED_NHS_NUMBER,
                    NotificationStatus = NotificationStatus.Draft,
                    PatientDetails = new PatientDetails
                    {
                        NhsNumberNotKnown = false,
                        NhsNumber = Utilities.NHS_NUMBER_SHARED
                    }
                },
                new Notification
                {
                    NotificationId = Utilities.PATIENT_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                    NotificationStatus = NotificationStatus.Notified,
                    PatientDetails = new PatientDetails
                    {
                        NhsNumberNotKnown = false,
                        NhsNumber = Utilities.NHS_NUMBER_SHARED
                    }
                }
            };
        }

        [Fact]
        public async Task PostDraft_ReturnsPageWithModelErrors_IfModelNotValid()
        {
            // Arrange
            const int id = Utilities.DRAFT_ID;
            var url = GetCurrentPathForId(id);
            var initialDocument = await GetDocumentForUrl(url);

            var formData = new Dictionary<string, string>
            {
                ["NotificationId"] = Utilities.DRAFT_ID.ToString(),
                ["PatientDetails.GivenName"] = "111",
                ["PatientDetails.FamilyName"] = "111",
                ["FormattedDob.Day"] = "31",
                ["FormattedDob.Month"] = "12",
                ["FormattedDob.Year"] = "1899",
                ["PatientDetails.NhsNumber"] = "123",
                ["PatientDetails.Address"] = "$$$",
                ["PatientDetails.LocalPatientId"] = "|||"
            };

            // Act
            var result = await SendPostFormWithData(initialDocument, formData, url);

            // Assert
            var resultDocument = await GetDocumentAsync(result);
            resultDocument.AssertErrorSummaryMessage("PatientDetails-GivenName", "given-name", "Given name can only contain letters and the symbols ' - . ,");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-FamilyName", "family-name", "Family name can only contain letters and the symbols ' - . ,");
            // Cannot easily check for equality with FullErrorMessage here as the error field is formatted oddly due to there being two fields in the error span.
            Assert.Contains("Date of birth must not be before 01/01/1900", resultDocument.GetError("dob"));
            resultDocument.AssertErrorSummaryMessage("PatientDetails-NhsNumber", "nhs-number", "NHS number needs to be 10 digits long");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-Address", "address", "Address can only contain letters, numbers and the symbols ' - . , /");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-LocalPatientId", "local-patient-id", "Invalid character found in Local Patient Id");
        }

        [Fact]
        public async Task PostDraft_ReturnsPageWithModelErrors_IfYearOfUkEntryBeforeDob()
        {
            // Arrange
            const int id = Utilities.DRAFT_ID;
            var url = GetCurrentPathForId(id);
            var initialDocument = await GetDocumentForUrl(url);

            var formData = new Dictionary<string, string>
            {
                ["NotificationId"] = Utilities.DRAFT_ID.ToString(),
                ["FormattedDob.Day"] = "31",
                ["FormattedDob.Month"] = "12",
                ["FormattedDob.Year"] = "2000",
                ["PatientDetails.CountryId"] = "1",
                ["PatientDetails.YearOfUkEntry"] = "1999"
            };

            // Act
            var result = await SendPostFormWithData(initialDocument, formData, url);

            // Assert
            var resultDocument = await GetDocumentAsync(result);
            resultDocument.AssertErrorSummaryMessage("PatientDetails-YearOfUkEntry", "year-of-entry", "Year of entry to the UK must be after patient's date of birth");
        }

        [Fact]
        public async Task PostNotified_ReturnsPageWithAllModelErrors_IfModelNotValid()
        {
            // Arrange
            const int id = Utilities.NOTIFIED_ID;
            var url = GetCurrentPathForId(id);
            var initialDocument = await GetDocumentForUrl(url);

            var formData = new Dictionary<string, string>
            {
                ["NotificationId"] = Utilities.NOTIFIED_ID.ToString(),
                ["PatientDetails.NhsNumberNotKnown"] = "false",
                ["PatientDetails.NoFixedAbode"] = "false",
            };

            // Act
            var result = await SendPostFormWithData(initialDocument, formData, url);

            // Assert
            var resultDocument = await GetDocumentAsync(result);

            result.EnsureSuccessStatusCode();

            resultDocument.AssertErrorSummaryMessage("PatientDetails-FamilyName", "family-name", "Family name is a mandatory field");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-GivenName", "given-name", "Given name is a mandatory field");
            Assert.Contains("Date of birth is a mandatory field", resultDocument.GetError("dob"));
            resultDocument.AssertErrorSummaryMessage("PatientDetails-NhsNumber", "nhs-number", "NHS number is a mandatory field");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-Postcode", "postcode", "Postcode is a mandatory field");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-SexId", "sex", "Sex is a mandatory field");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-EthnicityId", "ethnicity", "Ethnic group is a mandatory field");
            resultDocument.AssertErrorSummaryMessage("PatientDetails-CountryId", "birth-country", "Birth country is a mandatory field");
        }

        [Fact]
        public async Task Post_RedirectsToNextPageAndSavesContent_IfModelValid()
        {
            // Arrange
            const int id = Utilities.DRAFT_ID;
            var url = GetCurrentPathForId(id);
            var initialDocument = await GetDocumentForUrl(url);

            const string givenName = "Test";
            const string familyName = "User";
            const string birthDay = "5";
            const string birthMonth = "5";
            const string birthYear = "1992";
            const string nhsNumber = "5864552852";
            const string address = "123 Fake Street, London";
            const string ethnicityId = "1";
            const string sexId = "2";
            const string countryId = "3";
            const string localPatientId = "123#";
            const string occupationId = "1";
            var formData = new Dictionary<string, string>
            {
                ["NotificationId"] = Utilities.DRAFT_ID.ToString(),
                ["PatientDetails.GivenName"] = givenName,
                ["PatientDetails.FamilyName"] = familyName,
                ["FormattedDob.Day"] = birthDay,
                ["FormattedDob.Month"] = birthMonth,
                ["FormattedDob.Year"] = birthYear,
                ["PatientDetails.NhsNumber"] = nhsNumber,
                ["PatientDetails.Address"] = address,
                ["PatientDetails.NoFixedAbode"] = "true",
                ["PatientDetails.Postcode"] = "NW5 1TL",
                ["PatientDetails.EthnicityId"] = ethnicityId,
                ["PatientDetails.SexId"] = sexId,
                ["PatientDetails.CountryId"] = countryId,
                ["PatientDetails.LocalPatientId"] = localPatientId,
                ["PatientDetails.OccupationId"] = occupationId
            };

            // Act
            var result = await SendPostFormWithData(initialDocument, formData, url);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
            Assert.Contains(GetPathForId(NotificationSubPaths.EditEpisode, id), GetRedirectLocation(result));

            var reloadedPage = await Client.GetAsync(url);
            var reloadedDocument = await GetDocumentAsync(reloadedPage);
            reloadedDocument.AssertInputTextValue("PatientDetails_GivenName", givenName);
            reloadedDocument.AssertInputTextValue("PatientDetails_FamilyName", familyName);
            reloadedDocument.AssertInputTextValue("FormattedDob_Day", birthDay);
            reloadedDocument.AssertInputTextValue("FormattedDob_Month", birthMonth);
            reloadedDocument.AssertInputTextValue("FormattedDob_Year", birthYear);
            reloadedDocument.AssertInputTextValue("PatientDetails_NhsNumber", nhsNumber);
            reloadedDocument.AssertInputTextValue("PatientDetails_Postcode", string.Empty);
            reloadedDocument.AssertInputTextValue("PatientDetails_LocalPatientId", localPatientId);
            reloadedDocument.AssertInputSelectValue("PatientDetails_EthnicityId", ethnicityId);
            reloadedDocument.AssertInputSelectValue("PatientDetails_CountryId", countryId);
            reloadedDocument.AssertInputSelectValue("PatientDetails_OccupationId", occupationId);
            reloadedDocument.AssertInputRadioValue("sexId-2", true);
            reloadedDocument.AssertTextAreaValue("PatientDetails_Address", address);
        }

        [Fact]
        public async Task NonDuplicateNhsNumber_DoesNotShowWarning()
        {
            // Arrange
            const int id = Utilities.DRAFT_ID;
            var url = GetCurrentPathForId(id);

            // Act
            var initialPage = await Client.GetAsync(url);
            var initialDocument = await GetDocumentAsync(initialPage);

            // Assert
            Assert.True(initialDocument.GetElementById("nhs-number-warning").ClassList.Contains("hidden"));
        }

        public static IEnumerable<object[]> WarningScenarios()
        {
            yield return new object[]
            {
                Utilities.PATIENT_DRAFT_NOTIFICATION_SHARED_NHS_NUMBER,
                new List<int>
                {
                    Utilities.PATIENT_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                    Utilities.PATIENT_GROUPED_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                    Utilities.PATIENT_GROUPED_DENOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER
                }
            };
            yield return new object[]
            {
                Utilities.PATIENT_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                new List<int>
                {
                    Utilities.PATIENT_GROUPED_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                    Utilities.PATIENT_GROUPED_DENOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER
                }
            };
            yield return new object[]
            {
                Utilities.PATIENT_GROUPED_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                new List<int>
                {
                    Utilities.PATIENT_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                }
            };
            yield return new object[]
            {
                Utilities.PATIENT_GROUPED_DENOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                new List<int>
                {
                    Utilities.PATIENT_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER,
                }
            };
        }

        [Theory, MemberData(nameof(WarningScenarios))]
        public async Task DuplicateNhsNumber_ShowsWarningsWithExpectedIds(int pageNotificationId, List<int> expectedWarningNotificationIds)
        {
            // Arrange
            var url = GetCurrentPathForId(pageNotificationId);

            // Act
            var initialDocument = await GetDocumentForUrl(url);

            // Assert
            Assert.False(initialDocument.GetElementById("nhs-number-warning").ClassList.Contains("hidden"));
            var linksContainer = initialDocument.GetElementById("nhs-number-links");
            Assert.Equal(expectedWarningNotificationIds.Count, linksContainer.ChildElementCount);

            foreach (var notificationId in expectedWarningNotificationIds)
            {
                var warningUrl = RouteHelper.GetNotificationPath(notificationId, NotificationSubPaths.Overview);
                Assert.Equal($"#{notificationId}", linksContainer.QuerySelector($"a[href='{warningUrl}']").TextContent);
            }
        }

        [Fact]
        public async Task OnGetNhsNumberDuplicates_ReturnsExpectedEmptyResponseForNonDuplicate()
        {
            // Arrange
            const int id = Utilities.DRAFT_ID;
            const string nonDuplicateNhsNumber = "5864552852";
            var formData = new Dictionary<string, string>
            {
                ["notificationId"] = id.ToString(),
                ["nhsNumber"] = nonDuplicateNhsNumber
            };

            // Act
            var response = await Client.GetAsync(GetHandlerPath(formData, "NhsNumberDuplicates", id));

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("{}", result);
        }

        [Fact]
        public async Task OnGetNhsNumberDuplicates_ReturnsExpectedResponseForGroupedDuplicateNhsNumber()
        {
            // Arrange
            const int id = Utilities.PATIENT_GROUPED_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER;
            const string nhsNumber = Utilities.NHS_NUMBER_SHARED;
            var formData = new Dictionary<string, string>
            {
                ["notificationId"] = id.ToString(),
                ["nhsNumber"] = nhsNumber
            };
            const int expectedWarningNotificationId = Utilities.PATIENT_NOTIFIED_NOTIFICATION_SHARED_NHS_NUMBER;
            var expectedWarningUrl = RouteHelper.GetNotificationPath(expectedWarningNotificationId, NotificationSubPaths.Overview);

            // Act
            var response = await Client.GetAsync(GetHandlerPath(formData, "NhsNumberDuplicates", id));

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains($"\"{expectedWarningNotificationId}\":\"{expectedWarningUrl}\"", result);
        }

        [Fact]
        public async Task IfDateTooEarly_ValidatePatientDate_ReturnsEarliestBirthDateErrorMessage()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                ["key"] = "Dob",
                ["day"] = "1",
                ["month"] = "1",
                ["year"] = "1899"
            };

            // Act
            var response = await Client.GetAsync(GetHandlerPath(formData, "ValidatePatientDetailsDate"));

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("Date of birth must not be before 01/01/1900", result);
        }

        [Theory]
        [InlineData("ABC", "NHS number can only contain digits 0-9")]
        [InlineData("123", "NHS number needs to be 10 digits long")]
        [InlineData("5647382911", "This NHS number is not valid. Confirm you have entered it correctly")]
        public async Task WhenNhsNumberInvalid_ValidatePatientProperty_ReturnsExpectedResult(string nhsNumber, string validationResult)
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                ["key"] = "NhsNumber",
                ["value"] = nhsNumber
            };

            // Act
            var response = await Client.GetAsync(GetHandlerPath(formData, "ValidatePatientDetailsProperty"));

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(validationResult, result);
        }

        [Theory]
        [InlineData("true", "NHS number is a mandatory field")]
        [InlineData("false", "")]
        public async Task DependentOnShouldValidateFull_ValidatePatientProperty_ReturnsRequiredOrNoError(string shouldValidateFull, string validationResult)
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                ["shouldValidateFull"] = shouldValidateFull,
                ["key"] = "NhsNumber"
            };

            // Act
            var response = await Client.GetAsync(GetHandlerPath(formData, "ValidatePatientDetailsProperty"));

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(validationResult, result);
        }
    }
}
