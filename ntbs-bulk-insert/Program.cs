﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using ntbs_service.DataAccess;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using ntbs_service.Models.ReferenceEntities;

namespace ConsoleApp2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool addTreatmentEvents = args[0] != "--withDqAlerts";
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: false, reloadOnChange: true);

            Directory.SetCurrentDirectory("./bin/debug/netcoreapp2.2");
            builder.SetBasePath(Directory.GetCurrentDirectory());
                
            var configuration = builder.Build();

            var connectionString = configuration.GetConnectionString("ntbsContext");
            
            var options = new DbContextOptionsBuilder<NtbsContext>()
                .UseSqlServer(connectionString)
                .Options;
            
            using (var context = new NtbsContext(options))
            {
                if (!context.Database.GetService<IRelationalDatabaseCreator>().Exists())
                {
                    Console.WriteLine("Database does not exist, aborting bulk insert");
                    return;
                }
                Console.WriteLine("Starting generation of notifications");
                await GenerateNotifications(context, addTreatmentEvents);
                Console.WriteLine("Finished generation of notifications");
            }
        }


        static async Task GenerateNotifications(NtbsContext context, bool addTreatmentEvents = false)
        {
            var numberOfNotifications = 3000;
            var rand = new Random();
            var tbServices = (await context.TbService.ToListAsync()).Select(t => t.Code).ToList();
            var hospitals = (await context.Hospital.ToListAsync());
            
            var notificationsOperable = Builder<Notification>.CreateListOfSize(numberOfNotifications)
                .All()
                .With(n => n.NotificationId = 0)
                .With(n => n.NotificationStatus = NotificationStatus.Notified)
                .With(n => n.PatientDetails.GivenName = Faker.Name.First())
                .With(n => n.PatientDetails.FamilyName = Faker.Name.Last())
                .With(n => n.PatientDetails.NoFixedAbode = true)
                .With(n => n.TestData.HasTestCarriedOut = false)
                .With(n => n.ClinicalDetails.Notes = "UniqueBulkInsert")
                .With(n => n.GroupId = null)
                // Add randomised fields that Faker cannot generate
                .With(n => n.PatientDetails.Dob = AddRandomDateTimeBetween1950And2000(rand))
                .With(n => n.PatientDetails.SexId = rand.Next(1, 3))
                .With(n => n.PatientDetails.EthnicityId = rand.Next(1, 10))
                .With(n => n.PatientDetails.CountryId = rand.Next(1, 200))
                .With(n => n.NotificationDate = AddRandomDateTimeBetween2014And2017(rand))
                .With(n => n.HospitalDetails.TBServiceCode = AddRandomTbService(rand, tbServices))
                .With(n => n.HospitalDetails.HospitalId = hospitals.FirstOrDefault(h => h.TBServiceCode == n.HospitalDetails.TBServiceCode)?.HospitalId)
                .With(n => n.PatientDetails.NhsNumber = AddRandomTestNhsNumber(rand))
                .With(n => n.NotificationSites = new List<NotificationSite>
                {
                    new NotificationSite
                    {
                        SiteId = 1
                        
                    }
                })
                .With(n => n.ClinicalDetails.DiagnosisDate = new DateTime(2014, 1, 1));
            

            if (addTreatmentEvents)
            {
                notificationsOperable = await AddDataQualityTreatmentEvents(notificationsOperable, context);
            }
            
            var notifications= notificationsOperable.Build();

            context.AddRange(notifications);
            await context.SaveChangesAsync();
        }

        private static DateTime AddRandomDateTimeBetween2014And2017(Random rand)
        {
            var days = rand.Next(1, 1000);
            return new DateTime(2014, 1, 1).AddDays(days);
        }
        
        private static DateTime AddRandomDateTimeBetween1950And2000(Random rand)
        {
            var days = rand.Next(1, 18262);
            return new DateTime(1950, 1, 1).AddDays(days);
        }

        private static string AddRandomTbService(Random rand, IList<string> tbServices)
        {
            var tbServiceIndex = rand.Next(0, tbServices.Count - 1);
            return tbServices[tbServiceIndex];
        }

        private static string AddRandomTestNhsNumber(Random rand)
        {
            var nhsNumberString = new StringBuilder("9", 10);
            for (var i = 0; i < 9; i++)
            {
                nhsNumberString.Append(rand.Next(1, 9).ToString());
            }
            return nhsNumberString.ToString();
        }

        private static async Task<IOperable<Notification>> AddDataQualityTreatmentEvents(IOperable<Notification> notificationsOperable, NtbsContext context)
        {
            var completedOutcome = await context.TreatmentOutcome.Where(t => t.TreatmentOutcomeId == 1).FirstOrDefaultAsync();
            var notificationsOperableWithTreatmentEvents = notificationsOperable
                .With(n => n.TreatmentEvents = new List<TreatmentEvent>
                {
                    new TreatmentEvent
                    {
                        TreatmentEventType = TreatmentEventType.TreatmentStart,
                        EventDate = new DateTime(2017, 1, 1)
                    },
                    new TreatmentEvent
                    {
                        TreatmentEventType = TreatmentEventType.TreatmentOutcome,
                        EventDate = new DateTime(2017, 2, 1),
                        TreatmentOutcomeId = completedOutcome.TreatmentOutcomeId,
                        TreatmentOutcome = completedOutcome
                    }
                });
            return notificationsOperableWithTreatmentEvents;
        }
        
    }
}
