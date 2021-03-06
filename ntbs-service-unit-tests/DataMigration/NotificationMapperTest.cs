﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ntbs_service.DataAccess;
using ntbs_service.DataMigration;
using ntbs_service.DataMigration.RawModels;
using ntbs_service.Helpers;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using ntbs_service.Models.ReferenceEntities;
using ntbs_service.Services;
using Xunit;

namespace ntbs_service_unit_tests.DataMigration
{
    // This test suite attempts to test the logic of the migration mapper. The mapper is responsible for mapping the
    // DTO objects obtained through dapper from the migration db, to the entities used by the app.
    //
    // This mapping is non-trivial, due to:
    //   - the size of the objects, with ~200 fields involved
    //   - some migration logic happening at this stage e.g.
    //        - looking up hospital ids to TB services based on NTBS reference data
    //        - providing fallback values
    //
    // This test was added quite late into the migration development process. Rather than trying to recreate every
    // single edge case, it aims to provide an example of how to add more regression cases as bugs are dealt with.
    public class NotificationMapperTest
    {
        private const int TreatmentOutcomeStillOnTreatmentId = 16;
        private const int TreatmentOutcomeTbCausedDeathId = 7;

        // SUTs - we're breaking the convention a little here by testing two classes in one suit, but
        // they are realistically never used separately and this way we have can simulate the import logic a little
        // closer
        private readonly NotificationMapper _notificationMapper;
        private readonly IImportValidator _importValidator;

        private readonly MigrationRepositoryStub _migrationRepository = new MigrationRepositoryStub();

        private readonly Mock<IReferenceDataRepository> _referenceDataRepositoryMock =
            new Mock<IReferenceDataRepository>();
        private readonly Mock<IPostcodeService> _postcodeService = new Mock<IPostcodeService>();
        private readonly Mock<ICaseManagerImportService> _caseManagerImportService =
            new Mock<ICaseManagerImportService>();

        private readonly Mock<IImportLogger> _importLoggerMock = new Mock<IImportLogger>();
        private readonly ITreatmentEventMapper _treatmentEventMapper;

        private Dictionary<Guid, TBService> _hospitalToTbServiceCodeDict;
        private Dictionary<string, User> _usernameToUserDict = SetUserDict();

        public NotificationMapperTest()
        {
            _caseManagerImportService
                .Setup(serv => serv.ImportOrUpdateLegacyUser(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);
            _referenceDataRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
                .Returns((string username) => Task.FromResult(_usernameToUserDict[username]));
            _referenceDataRepositoryMock.Setup(repo => repo.GetTbServiceFromHospitalIdAsync(It.IsAny<Guid>()))
                .Returns((Guid guid) => Task.FromResult(_hospitalToTbServiceCodeDict[guid]));
            _referenceDataRepositoryMock.Setup(repo =>
                    repo.GetTreatmentOutcomeForTypeAndSubType(
                        TreatmentOutcomeType.Died,
                        TreatmentOutcomeSubType.Unknown))
                .ReturnsAsync(new TreatmentOutcome
                {
                    TreatmentOutcomeType = TreatmentOutcomeType.Died,
                    TreatmentOutcomeSubType = TreatmentOutcomeSubType.Unknown
                });
            _referenceDataRepositoryMock.Setup(repo =>
                    repo.GetTreatmentOutcomesForType(TreatmentOutcomeType.Died))
                .ReturnsAsync(new List<TreatmentOutcome> {
                    new TreatmentOutcome { TreatmentOutcomeId = 7, TreatmentOutcomeType = TreatmentOutcomeType.Died, TreatmentOutcomeSubType = TreatmentOutcomeSubType.TbCausedDeath },
                    new TreatmentOutcome { TreatmentOutcomeId = 8, TreatmentOutcomeType = TreatmentOutcomeType.Died, TreatmentOutcomeSubType = TreatmentOutcomeSubType.TbContributedToDeath },
                    new TreatmentOutcome { TreatmentOutcomeId = 9, TreatmentOutcomeType = TreatmentOutcomeType.Died, TreatmentOutcomeSubType = TreatmentOutcomeSubType.TbIncidentalToDeath },
                    new TreatmentOutcome { TreatmentOutcomeId = 10, TreatmentOutcomeType = TreatmentOutcomeType.Died, TreatmentOutcomeSubType = TreatmentOutcomeSubType.Unknown }
                });
            _postcodeService.Setup(service => service.FindPostcodeAsync(It.IsAny<string>()))
                .ReturnsAsync((string postcode) => new PostcodeLookup { Postcode = postcode.Replace(" ", "").ToUpper() });

            // Needs to happen after the mocking, as the constructor uses a method from reference data repo
            _treatmentEventMapper =
                new TreatmentEventMapper(_caseManagerImportService.Object, _referenceDataRepositoryMock.Object);
            _notificationMapper = new NotificationMapper(
                _migrationRepository,
                _referenceDataRepositoryMock.Object,
                _importLoggerMock.Object,
                _postcodeService.Object,
                _caseManagerImportService.Object,
                _treatmentEventMapper);
            _importValidator = new ImportValidator(_importLoggerMock.Object, _referenceDataRepositoryMock.Object);
        }

        // The data for this test is mainly sourced from the test data created for NTBS during the alpha phase
        // so there is no real patient data here
        // Bits were "stolen" from different notifications and adapted though,
        // so together it might not paint a "cohesive" picture.
        [Fact]
        public async Task correctlyCreates_basicNotification()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "130331" };
            SetupNotificationsInGroups(("130331", "1"));
            const string royalBerkshireCode = "TBS001";
            const string bristolRoyalCode = "TBS002";
            const string westonGeneralCode = "TBS003";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("B8AA918D-233F-4C41-B9AE-BE8A8DC8BE7A"), new TBService {Code = royalBerkshireCode}},
                {new Guid("F026FDCD-7BAF-4C96-994C-20E436CC8C59"), new TBService {Code = bristolRoyalCode}},
                {new Guid("0AC033AB-9A11-4FA6-AA1A-1FCA71180C2F"), new TBService {Code = westonGeneralCode}}
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();
            var validationErrors =
                await _importValidator.CleanAndValidateNotification(null, runId, notification);

            // Assert
            Assert.Equal("ETS", notification.LegacySource);
            Assert.Equal("130331", notification.ETSID);
            Assert.Equal("130331", notification.LegacyId);
            Assert.Equal(new DateTime(2015, 3, 31), notification.NotificationDate?.Date);
            Assert.Equal(NotificationStatus.Notified, notification.NotificationStatus);

            Assert.Equal("RG145UT", notification.PatientDetails.Postcode);
            Assert.False(notification.PatientDetails.NoFixedAbode);
            Assert.Equal("Winford", notification.PatientDetails.GivenName);
            Assert.Equal("Wongus", notification.PatientDetails.FamilyName);
            Assert.Equal("9815779000", notification.PatientDetails.NhsNumber);
            Assert.Equal(new DateTime(1981, 3, 24), notification.PatientDetails.Dob);
            Assert.Equal(2010, notification.PatientDetails.YearOfUkEntry);
            Assert.Equal("3 Winglass Place\nWongaton", notification.PatientDetails.Address);

            Assert.Equal(new Guid("B8AA918D-233F-4C41-B9AE-BE8A8DC8BE7A"), notification.HospitalDetails.HospitalId);
            Assert.Equal(royalBerkshireCode, notification.HospitalDetails.TBServiceCode);
            Assert.Equal("Dr McGown", notification.HospitalDetails.Consultant);

            Assert.Equal(HIVTestStatus.HIVStatusKnown, notification.ClinicalDetails.HIVTestState);
            Assert.Equal(Status.Yes, notification.ClinicalDetails.HomeVisitCarriedOut);
            Assert.Equal(new DateTime(2015, 03, 25, 00, 00, 00), notification.ClinicalDetails.FirstHomeVisitDate);
            Assert.Equal("Patient did not begin course of treatment under DOT", notification.ClinicalDetails.Notes);

            Assert.Equal(Status.No, notification.SocialRiskFactors.RiskFactorDrugs.Status);
            Assert.Null(notification.SocialRiskFactors.RiskFactorDrugs.IsCurrent);
            Assert.Null(notification.SocialRiskFactors.RiskFactorDrugs.InPastFiveYears);
            Assert.Null(notification.SocialRiskFactors.RiskFactorDrugs.MoreThanFiveYearsAgo);
            Assert.Null(notification.SocialRiskFactors.RiskFactorHomelessness.Status);
            Assert.Null(notification.SocialRiskFactors.RiskFactorHomelessness.IsCurrent);
            Assert.Null(notification.SocialRiskFactors.RiskFactorHomelessness.InPastFiveYears);
            Assert.Null(notification.SocialRiskFactors.RiskFactorHomelessness.MoreThanFiveYearsAgo);
            Assert.Equal(Status.Yes, notification.SocialRiskFactors.RiskFactorImprisonment.Status);
            Assert.False(notification.SocialRiskFactors.RiskFactorImprisonment.IsCurrent);
            Assert.True(notification.SocialRiskFactors.RiskFactorImprisonment.InPastFiveYears);
            Assert.False(notification.SocialRiskFactors.RiskFactorImprisonment.MoreThanFiveYearsAgo);

            Assert.Equal(2, notification.TravelDetails.TotalNumberOfCountries);
            Assert.Equal(Status.Yes, notification.TravelDetails.HasTravel);
            Assert.Equal(1, notification.TravelDetails.Country1Id);
            Assert.Equal(9, notification.TravelDetails.Country2Id);
            Assert.Null(notification.TravelDetails.Country3Id);
            Assert.Equal(1, notification.TravelDetails.StayLengthInMonths1);
            Assert.Equal(3, notification.TravelDetails.StayLengthInMonths2);
            Assert.Null(notification.TravelDetails.StayLengthInMonths3);

            Assert.False(notification.ShouldBeClosed());
            Assert.Equal(NotificationStatus.Notified, notification.NotificationStatus);

            Assert.Empty(validationErrors);
        }

        // Data for this has been based on real regression examples, but with care taken to anonymize it
        // This is based on NTBS-1594
        [Fact]
        public async Task correctlyMaps_ContactTracingNumbers()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "235676", "237137", "241256", "242084" };
            SetupNotificationsInGroups(("235676", "2"),
                ("237137", "3"),
                ("241256", "4"),
                ("242084", "5")
            );

            const string salfordRoyalCode = "TBS0193";
            const string leedsGeneralCode = "TBS0106";
            const string frimleyParkCode = "TBS0075";
            const string colchesterGeneralCode = "TBS0049";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("3ECAC202-C204-4384-B3F9-0D3FF412DC36"), new TBService {Code = salfordRoyalCode}},
                {new Guid("7E9C715D-0248-4D97-8F67-1134FC133588"), new TBService {Code = leedsGeneralCode}},
                {new Guid("44C3608F-231E-4DD7-963C-4492D804E894"), new TBService {Code = frimleyParkCode}},
                {new Guid("0EEE2EC2-1F3E-4175-BE90-85AA33F0686C"), new TBService {Code = colchesterGeneralCode}}
            };

            // Act
            var notifications = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .ToList();

            // Assert
            Assert.Equal(4, notifications.Count);

            var notification242084 = notifications.Find(n => n.ETSID == "242084").ContactTracing;
            Assert.Equal(2, notification242084.AdultsIdentified);
            Assert.Equal(2, notification242084.AdultsScreened);
            Assert.Equal(0, notification242084.AdultsActiveTB);
            Assert.Equal(0, notification242084.AdultsLatentTB);
            Assert.Null(notification242084.ChildrenIdentified);
            Assert.Null(notification242084.ChildrenScreened);
            Assert.Null(notification242084.ChildrenActiveTB);
            Assert.Null(notification242084.ChildrenLatentTB);

            var notification237137 = notifications.Find(n => n.ETSID == "237137").ContactTracing;
            Assert.Equal(3, notification237137.AdultsIdentified);
            Assert.Equal(3, notification237137.AdultsScreened);
            Assert.Equal(4, notification237137.ChildrenIdentified);
            Assert.Equal(4, notification237137.ChildrenScreened);

            var notification235676 = notifications.Find(n => n.ETSID == "235676").ContactTracing;
            Assert.Equal(1, notification235676.AdultsIdentified);
            Assert.Equal(1, notification235676.AdultsScreened);
            Assert.Equal(0, notification235676.AdultsActiveTB);
            Assert.Equal(0, notification235676.AdultsLatentTB);
            Assert.Equal(0, notification235676.AdultsStartedTreatment);
            Assert.Equal(0, notification235676.AdultsFinishedTreatment);
            Assert.Equal(0, notification235676.ChildrenIdentified);
            Assert.Equal(0, notification235676.ChildrenScreened);
            Assert.Equal(0, notification235676.ChildrenActiveTB);
            Assert.Equal(0, notification235676.ChildrenLatentTB);
            Assert.Equal(0, notification235676.ChildrenStartedTreatment);
            Assert.Equal(0, notification235676.ChildrenFinishedTreatment);

            var notification241256 = notifications.Find(n => n.ETSID == "241256").ContactTracing;
            Assert.Equal(0, notification241256.AdultsIdentified);
            Assert.Equal(0, notification241256.AdultsScreened);
            Assert.Equal(0, notification241256.AdultsActiveTB);
            Assert.Equal(0, notification241256.AdultsLatentTB);
            Assert.Equal(0, notification241256.AdultsStartedTreatment);
            Assert.Equal(0, notification241256.AdultsFinishedTreatment);
            Assert.Equal(0, notification241256.ChildrenIdentified);
            Assert.Equal(0, notification241256.ChildrenScreened);
            Assert.Equal(0, notification241256.ChildrenActiveTB);
            Assert.Equal(0, notification241256.ChildrenLatentTB);
            Assert.Equal(0, notification241256.ChildrenStartedTreatment);
            Assert.Equal(0, notification241256.ChildrenFinishedTreatment);
        }

        // This is based on NTBS-1650
        [Fact]
        public async Task correctlyCreates_PostMortemNotification()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "132465" };
            SetupNotificationsInGroups(("132465", "6"));

            const string colchesterGeneralCode = "TBS0049";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("0EEE2EC2-1F3E-4175-BE90-85AA33F0686C"), new TBService {Code = colchesterGeneralCode}}
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.Single(notification.TreatmentEvents);
            Assert.True(notification.ClinicalDetails.IsPostMortem);
            Assert.Equal(NotificationStatus.Closed, notification.NotificationStatus);
            // For post mortem cases we *only* want to import the single death event so outcomes reporting is correct
            Assert.Collection(notification.TreatmentEvents,
                te => Assert.Equal(TreatmentOutcomeType.Died, te.TreatmentOutcome.TreatmentOutcomeType));
        }

        // This is based on NTBS-2417
        [Fact]
        public async Task correctlyCreates_DeathEventWithoutPostmortem()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "132468" };
            SetupNotificationsInGroups(("132468", "6"));

            const string colchesterGeneralCode = "TBS0049";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("0EEE2EC2-1F3E-4175-BE90-85AA33F0686C"), new TBService {Code = colchesterGeneralCode}}
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.False(notification.ClinicalDetails.IsPostMortem);
            Assert.Collection(notification.TreatmentEvents,
                te => Assert.Equal(TreatmentEventType.DiagnosisMade, te.TreatmentEventType),
                te => Assert.Equal(TreatmentOutcomeStillOnTreatmentId, te.TreatmentOutcomeId),
                te => Assert.Equal(TreatmentOutcomeType.Died, te.TreatmentOutcome.TreatmentOutcomeType));
        }

        // This is based on NTBS-2417
        [Fact]
        public async Task doesNotAddDeathEvent_WhenOneAlreadyExists()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "132469" };
            SetupNotificationsInGroups(("132469", "6"));

            const string colchesterGeneralCode = "TBS0049";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("0EEE2EC2-1F3E-4175-BE90-85AA33F0686C"), new TBService {Code = colchesterGeneralCode}}
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.False(notification.ClinicalDetails.IsPostMortem);

            var treatmentEvents = notification.TreatmentEvents.ToList();
            Assert.Collection(treatmentEvents,
                te => Assert.Equal(TreatmentEventType.DiagnosisMade, te.TreatmentEventType),
                te => Assert.Equal(TreatmentOutcomeStillOnTreatmentId, te.TreatmentOutcomeId),
                te => Assert.Equal(TreatmentOutcomeTbCausedDeathId, te.TreatmentOutcomeId));
            Assert.Equal(DateTime.Parse("2019-12-01"), treatmentEvents[2].EventDate);
        }

        // Data for this was based on the test notification 130331, used in correctlyCreates_basicNotification
        // Additional fictional M. bovis data was added for this test.
        [Fact]
        public async Task correctlyMaps_MBovisAnimalAndKnownCaseExposures()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "131686" };
            SetupNotificationsInGroups(("131686", "7"));

            const string royalBerkshireCode = "TBS001";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("B8AA918D-233F-4C41-B9AE-BE8A8DC8BE7A"), new TBService {Code = royalBerkshireCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            // Notification 131686 has animal and known cases forms filled out, so their statuses should be yes.
            // There are no milk and occupation records, so their statuses should be no (for we do not know
            // whether there were definitely no exposures, or if the exposures status was not known, given the
            // absence of exposure records.
            Assert.Equal(Status.Yes, notification.MBovisDetails.AnimalExposureStatus);
            Assert.Equal(1, notification.MBovisDetails.MBovisAnimalExposures.Count);
            Assert.Collection(notification.MBovisDetails.MBovisAnimalExposures, exposure =>
            {
                Assert.Equal(2000, exposure.YearOfExposure);
                Assert.Equal(AnimalType.WildAnimal, exposure.AnimalType);
                Assert.Equal("Badger", exposure.Animal);
                Assert.Equal(AnimalTbStatus.Unknown, exposure.AnimalTbStatus);
                Assert.Equal(1, exposure.ExposureDuration);
                Assert.Equal(235, exposure.CountryId);
                Assert.Equal("Neighbourhood badger", exposure.OtherDetails);
            });
            Assert.Equal(Status.Yes, notification.MBovisDetails.ExposureToKnownCasesStatus);
            Assert.Equal(1, notification.MBovisDetails.MBovisExposureToKnownCases.Count);
            Assert.Collection(notification.MBovisDetails.MBovisExposureToKnownCases, exposure =>
            {
                Assert.Equal(2001, exposure.YearOfExposure);
                Assert.Equal(ExposureSetting.HealthcareHospital, exposure.ExposureSetting);
                Assert.Equal(Status.No, exposure.NotifiedToPheStatus);
                Assert.Equal("During annual checkup", exposure.OtherDetails);
            });
            Assert.Equal(Status.Unknown, notification.MBovisDetails.UnpasteurisedMilkConsumptionStatus);
            Assert.Empty(notification.MBovisDetails.MBovisUnpasteurisedMilkConsumptions);
            Assert.Equal(Status.Unknown, notification.MBovisDetails.OccupationExposureStatus);
            Assert.Empty(notification.MBovisDetails.MBovisOccupationExposures);
        }

        // Data for this was based on the test notification 130331, used in correctlyMaps_ContactTracingNumbers
        // Additional fictional M. bovis data was added for this test.
        [Fact]
        public async Task correctlyMaps_MBovisMilkAndOccupationExposures()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "131687" };
            SetupNotificationsInGroups(("131687", "8"));

            const string royalBerkshireCode = "TBS001";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("B8AA918D-233F-4C41-B9AE-BE8A8DC8BE7A"), new TBService {Code = royalBerkshireCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            // Notification 131687 has milk and occupation forms filled out, so their statuses should be yes.
            // There are no animal and known cases records, so their statuses should be no (for we do not know
            // whether there were definitely no exposures, or if the exposures status was not known, given the
            // absence of exposure records.
            Assert.Equal(Status.Unknown, notification.MBovisDetails.AnimalExposureStatus);
            Assert.Empty(notification.MBovisDetails.MBovisAnimalExposures);
            Assert.Equal(Status.Unknown, notification.MBovisDetails.ExposureToKnownCasesStatus);
            Assert.Empty(notification.MBovisDetails.MBovisExposureToKnownCases);
            Assert.Equal(Status.Yes, notification.MBovisDetails.UnpasteurisedMilkConsumptionStatus);
            Assert.Equal(1, notification.MBovisDetails.MBovisUnpasteurisedMilkConsumptions.Count);
            Assert.Collection(notification.MBovisDetails.MBovisUnpasteurisedMilkConsumptions, exposure =>
            {
                Assert.Equal(1999, exposure.YearOfConsumption);
                Assert.Equal(MilkProductType.Cheese, exposure.MilkProductType);
                Assert.Equal(ConsumptionFrequency.Occasionally, exposure.ConsumptionFrequency);
                Assert.Equal(235, exposure.CountryId);
                Assert.Equal("From the local farm shop", exposure.OtherDetails);
            });
            Assert.Equal(Status.Yes, notification.MBovisDetails.OccupationExposureStatus);
            Assert.Equal(1, notification.MBovisDetails.MBovisOccupationExposures.Count);
            Assert.Collection(notification.MBovisDetails.MBovisOccupationExposures, exposure =>
            {
                Assert.Equal(1998, exposure.YearOfExposure);
                Assert.Equal(OccupationSetting.Vet, exposure.OccupationSetting);
                Assert.Equal(3, exposure.OccupationDuration);
                Assert.Equal(235, exposure.CountryId);
                Assert.Equal("Worked at local veterinary surgery", exposure.OtherDetails);
            });
        }

        // This test uses test notification 237137, used in correctlyMaps_ContactTracingNumbers
        [Fact]
        public async Task correctlyMaps_NotStartedMBovisQuestionnaire()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "237137" };
            SetupNotificationsInGroups(("237137", "9"));

            const string leedsGeneralCode = "TBS0106";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("7E9C715D-0248-4D97-8F67-1134FC133588"), new TBService {Code = leedsGeneralCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            // Notification 237137 has no M. bovis forms filled out, so the statuses should be null
            Assert.Null(notification.MBovisDetails.AnimalExposureStatus);
            Assert.Empty(notification.MBovisDetails.MBovisAnimalExposures);
            Assert.Null(notification.MBovisDetails.ExposureToKnownCasesStatus);
            Assert.Empty(notification.MBovisDetails.MBovisExposureToKnownCases);
            Assert.Null(notification.MBovisDetails.UnpasteurisedMilkConsumptionStatus);
            Assert.Empty(notification.MBovisDetails.MBovisUnpasteurisedMilkConsumptions);
            Assert.Null(notification.MBovisDetails.OccupationExposureStatus);
            Assert.Empty(notification.MBovisDetails.MBovisOccupationExposures);
        }

        // This test uses test notification 237137, used in correctlyMaps_ContactTracingNumbers
        [Fact]
        public async Task correctlyMaps_DuplicateTravelCountries()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "237137" };
            SetupNotificationsInGroups(("237137", "9"));

            const string leedsGeneralCode = "TBS0106";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("7E9C715D-0248-4D97-8F67-1134FC133588"), new TBService {Code = leedsGeneralCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.Equal(3, notification.TravelDetails.TotalNumberOfCountries);
            Assert.Equal(Status.Yes, notification.TravelDetails.HasTravel);
            Assert.Equal(169, notification.TravelDetails.Country1Id);
            Assert.Equal(9, notification.TravelDetails.Country2Id);
            Assert.Null(notification.TravelDetails.Country3Id);
            Assert.Equal(5, notification.TravelDetails.StayLengthInMonths1);
            Assert.Equal(3, notification.TravelDetails.StayLengthInMonths2);
            Assert.Null(notification.TravelDetails.StayLengthInMonths3);
        }

        // This test uses test notification 237138, based on 237137 used in correctlyMaps_ContactTracingNumbers
        [Fact]
        public async Task correctlyWarns_WithInvalidPostcode()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "237138" };
            SetupNotificationsInGroups(("237138", "10"));

            const string leedsGeneralCode = "TBS0106";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("7E9C715D-0248-4D97-8F67-1134FC133588"), new TBService {Code = leedsGeneralCode}},
            };

            _postcodeService.Setup(service => service.FindPostcodeAsync("BF1"))
                .Returns(Task.FromResult<PostcodeLookup>(null));

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                        runId,
                        legacyIds))
                    .SelectMany(group => group)
                    .Single();

                // Assert
                Assert.Equal("BF1", notification.PatientDetails.Postcode);
                Assert.Null(notification.PatientDetails.PostcodeToLookup);

                _importLoggerMock.Verify(
                    s => s.LogNotificationWarning(null, runId, "237138", "invalid or unknown postcode"),
                    Times.Once);
        }


        // Data for this has been based on real regression examples, but with care taken to anonymize it
        // This is based on NTBS-2388
        [Fact]
        public async Task correctlyMaps_DenotifiedStatusIfOtherwiseWouldBeClosed()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "249398" };
            SetupNotificationsInGroups(("249398", "10"));

            const string malvernCode = "TBS0656";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("33464912-E5B1-4998-AFCA-083C3AE65A80"), new TBService {Code = malvernCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                        runId,
                        legacyIds))
                    .SelectMany(group => group)
                    .Single();

            // Assert
            Assert.Equal(NotificationStatus.Denotified ,notification.NotificationStatus);
        }

        // Data for this has been based on real examples, but with care taken to anonymize it
        [Fact]
        public async Task correctlyMaps_DenotificationValuesWithDenotificationReason()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "193300" };
            SetupNotificationsInGroups(("193300", "11"));

            const string malvernCode = "TBS0656";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("33464912-E5B1-4998-AFCA-083C3AE65A80"), new TBService {Code = malvernCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.Equal(NotificationStatus.Denotified, notification.NotificationStatus);
            Assert.Equal(DenotificationReason.NotTbAtypicalMyco, notification.DenotificationDetails.Reason);
            Assert.Equal(new DateTime(2019,06,01), notification.DenotificationDetails.DateOfDenotification);
        }

        [Fact]
        public async Task correctlyMaps_DenotificationValuesWhenNoDenotificationReason()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "193301" };
            SetupNotificationsInGroups(("193301", "11"));

            const string malvernCode = "TBS0656";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("33464912-E5B1-4998-AFCA-083C3AE65A80"), new TBService {Code = malvernCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.Equal(NotificationStatus.Denotified, notification.NotificationStatus);
            Assert.Equal(new DateTime(2019,06,01), notification.DenotificationDetails.DateOfDenotification);
            Assert.Equal(DenotificationReason.Other, notification.DenotificationDetails.Reason);
            Assert.Equal("Denotified in legacy system, with denotification date " + new DateTime(2019,06,01)
                , notification.DenotificationDetails.OtherDescription);
        }

        [Fact]
        public async Task correctlyMaps_TravelAndVisitorNumbersToNullWhenNoCountriesAddedWithStatusOfYes()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "130991" };
            SetupNotificationsInGroups(("130991", "12"));

            const string royalBerkshireCode = "TBS001";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("B8AA918D-233F-4C41-B9AE-BE8A8DC8BE7A"), new TBService {Code = royalBerkshireCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.Null(notification.TravelDetails.TotalNumberOfCountries);
            Assert.Null(notification.VisitorDetails.TotalNumberOfCountries);
        }

        // Data for this has been taken from an edited test notification, relating to NTBS-2478
        [Fact]
        public async Task whenLastTreatmentEventIsOutcomeStillOnTreatment_DoNotCloseNotificationDuringMapping()
        {
            // Arrange
            const int runId = 12345;
            var legacyIds = new List<string> { "300123" };
            SetupNotificationsInGroups(("300123", "12"));

            const string malvernCode = "TBS0656";
            _hospitalToTbServiceCodeDict = new Dictionary<Guid, TBService>
            {
                {new Guid("33464912-E5B1-4998-AFCA-083C3AE65A80"), new TBService {Code = malvernCode}},
            };

            // Act
            var notification = (await _notificationMapper.GetNotificationsGroupedByPatient(null,
                    runId,
                    legacyIds))
                .SelectMany(group => group)
                .Single();

            // Assert
            Assert.Equal(NotificationStatus.Notified, notification.NotificationStatus);
        }

        private void SetupNotificationsInGroups(params (string, string)[] legacyIdAndLegacyGroup)
        {
            var grouped = new List<(string notificationId, string groupId)>(legacyIdAndLegacyGroup)
                .GroupBy(
                    t => t.groupId,
                    t => t.notificationId
                )
                .ToList();
            _migrationRepository.GroupedNotificationsStub = grouped;
        }

        private static Dictionary<string, User> SetUserDict()
        {
            return new Dictionary<string, User>
            {
                { "Nancy.Pickering@ntbs.phe.com", new User{ Id = 1 } },
                { "Robert.Greene@ntbs.phe.com", new User { Id = 2 } }
            };
        }

        private class MigrationRepositoryStub : IMigrationRepository
        {
            public Task<IEnumerable<MigrationDbNotification>> GetNotificationsById(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbNotification>("notifications", legacyIds));

            }

            public Task<IEnumerable<MigrationDbSite>> GetNotificationSites(IEnumerable<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbSite>("sites", legacyIds));
            }

            public Task<IEnumerable<MigrationDbManualTest>> GetManualTestResults(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbManualTest>("manualTestResults", legacyIds));
            }

            public Task<IEnumerable<MigrationDbSocialContextVenue>> GetSocialContextVenues(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbSocialContextVenue>("socialContextVenues", legacyIds));
            }

            public Task<IEnumerable<MigrationDbSocialContextAddress>> GetSocialContextAddresses(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbSocialContextAddress>("socialContextAddresses", legacyIds));
            }

            public Task<IEnumerable<MigrationDbTransferEvent>> GetTransferEvents(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbTransferEvent>("transferEvents", legacyIds));
            }

            public Task<IEnumerable<MigrationDbOutcomeEvent>> GetOutcomeEvents(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbOutcomeEvent>("outcomeEvents", legacyIds));
            }

            public Task<IEnumerable<MigrationDbMBovisAnimal>> GetMigrationMBovisAnimalExposure(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbMBovisAnimal>("mBovisAnimalExposure", legacyIds));
            }

            public Task<IEnumerable<MigrationDbMBovisKnownCase>> GetMigrationMBovisExposureToKnownCase(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbMBovisKnownCase>("mBovisKnownCase", legacyIds));
            }

            public Task<IEnumerable<MigrationDbMBovisOccupation>> GetMigrationMBovisOccupationExposures(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbMBovisOccupation>("mBovisOccupationExposure", legacyIds));
            }

            public Task<IEnumerable<MigrationDbMBovisMilkConsumption>> GetMigrationMBovisUnpasteurisedMilkConsumption(List<string> legacyIds)
            {
                return Task.FromResult(CvsRecords<MigrationDbMBovisMilkConsumption>("mBovisMilkConsumption", legacyIds));
            }

            public Task<IEnumerable<IGrouping<string, string>>> GetGroupedNotificationIdsById(IEnumerable<string> legacyIds)
            {
                return Task.FromResult(GroupedNotificationsStub);
            }

            public Task<IEnumerable<IGrouping<string, string>>> GetGroupedNotificationIdsByDate(DateTime rangeStartDate, DateTime endStartDate)
            {
                throw new NotImplementedException();
            }

            public Task<MigrationLegacyUser> GetLegacyUserByUsername(string username)
            {
                return Task.FromResult(CvsUserRecords(username));
            }

            public Task<IEnumerable<MigrationLegacyUserHospital>> GetLegacyUserHospitalsByUsername(string username)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IGrouping<string, string>> GroupedNotificationsStub { get; set; }

            private static IEnumerable<T> CvsRecords<T>(string file, IEnumerable<string> legacyIds)
                where T : MigrationDbRecord
            {
                return CsvParser
                    .GetRecordsFromCsv<T>($"../../../TestData/MigrationDatabaseMock/{file}.csv")
                    .Where(record => legacyIds.Contains(record.OldNotificationId));
            }
            private static MigrationLegacyUser CvsUserRecords(string username)
            {
                return CsvParser
                    .GetRecordsFromCsv<MigrationLegacyUser>($"../../../TestData/MigrationDatabaseMock/legacyUsers.csv")
                    .SingleOrDefault(user => user.Username == username);
            }
        }
    }
}
