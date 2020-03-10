﻿using System;
using System.Collections.Generic;
using System.Linq;
using ntbs_service.Models.Entities;
using ntbs_service.Models.Enums;
using ntbs_service.Models.ReferenceEntities;

namespace ntbs_service.Services
{
    public static class TreatmentOutcomesHelper
    {
        public static TreatmentOutcome GetTreatmentOutcomeAtXYears(Notification notification, int yearsAfterTreatmentStartDate)
        {
            // If a treatment outcome is missing at X years then one must not already exist
            if (IsTreatmentOutcomeMissingAtXYears(notification, yearsAfterTreatmentStartDate))
            {
                return null;
            }
            
            // If a treatment outcome is not missing that is because one either exists as the last event of the 1 year period
            // or one is not needed and so is null
            return GetOrderedTreatmentEventsInWindowXtoXMinus1Years(notification, yearsAfterTreatmentStartDate)
                ?.LastOrDefault(x => x.TreatmentOutcome != null)
                ?.TreatmentOutcome;
        }

        public static bool IsTreatmentOutcomeMissingAtXYears(Notification notification, int yearsAfterTreatmentStartDate)
        {
            for (var i = yearsAfterTreatmentStartDate; i >= 1; i--)
            {
                var lastTreatmentEventsBetweenIAndIMinusOneYears = GetOrderedTreatmentEventsInWindowXtoXMinus1Years(notification, i)?.LastOrDefault();
                
                // Check if any events have happened in this year window, look back a year if none exist
                if (lastTreatmentEventsBetweenIAndIMinusOneYears == null)
                {
                    continue;
                }
                
                // If the last event was not a treatment outcome event a treatment outcome event is missing
                if (!lastTreatmentEventsBetweenIAndIMinusOneYears.TreatmentEventTypeIsOutcome)
                {
                    return true;
                }
                
                // If a previous year has a treatment outcome of not evaluated this is not an ending treatment outcome
                // so a new treatment outcome will be needed for this 12 month period
                if (i < yearsAfterTreatmentStartDate &&
                    lastTreatmentEventsBetweenIAndIMinusOneYears.TreatmentOutcome?.TreatmentOutcomeSubType ==
                    TreatmentOutcomeSubType.StillOnTreatment)
                {
                    return true;
                }
                
                // If a treatment outcome event exists then a new one is not needed
                if (lastTreatmentEventsBetweenIAndIMinusOneYears.TreatmentEventTypeIsOutcome)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsTreatmentOutcomeExpectedAtXYears(Notification notification, int yearsAfterTreatmentStartDate)
        {
            if (notification.NotificationDate == null || DateTime.Now < (notification.ClinicalDetails.TreatmentStartDate ?? notification.NotificationDate).Value.AddYears(yearsAfterTreatmentStartDate))
            {
                return false;
            }
            
            if (GetOrderedTreatmentEventsInWindowXtoXMinus1Years(notification, yearsAfterTreatmentStartDate)
                    ?.LastOrDefault(x => x.TreatmentOutcome != null) != null)
            {
                return true;
            }
            return IsTreatmentOutcomeMissingAtXYears(notification, yearsAfterTreatmentStartDate);
        }
        
        private static IEnumerable<TreatmentEvent> GetOrderedTreatmentEventsInWindowXtoXMinus1Years(Notification notification, int numberOfYears)
        {
            return notification.TreatmentEvents?.Where(t =>
                    t.EventDate < (notification.ClinicalDetails.TreatmentStartDate ?? notification.NotificationDate)?.AddYears(numberOfYears)
                    && t.EventDate >= (notification.ClinicalDetails.TreatmentStartDate ?? notification.NotificationDate)?.AddYears(numberOfYears - 1))
                .OrderBy(t => t.EventDate)
                .ThenByDescending(t => t.TreatmentEventTypeIsOutcome);
        }
    }
}
