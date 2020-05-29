﻿using System.ComponentModel.DataAnnotations;
using EFAuditer;
using Microsoft.EntityFrameworkCore;
using ntbs_service.Models.Enums;

namespace ntbs_service.Models.Entities
{
    [Owned]
    public class ComorbidityDetails : ModelBase, IOwnedEntityForAuditing
    {
        [Display(Name = "Does the patient have diabetes?")]
        public Status? DiabetesStatus { get; set; }
        [Display(Name = "Does the patient have Hepatitis B?")]
        public Status? HepatitisBStatus { get; set; }
        [Display(Name = "Does the patient have Hepatitis C?")]
        public Status? HepatitisCStatus { get; set; }
        [Display(Name = "Does the patient have chronic liver disease?")]
        public Status? LiverDiseaseStatus { get; set; }
        [Display(Name = "Does the patient have chronic renal disease?")]
        public Status? RenalDiseaseStatus { get; set; }

        string IOwnedEntityForAuditing.RootEntityType => RootEntities.Notification;
    }
}
