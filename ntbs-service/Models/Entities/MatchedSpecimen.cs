using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ntbs_service.Models.Entities
{
    [NotMapped]
    public class MatchedSpecimen : SpecimenBase
    {
        public int NotificationId { get; set; }

        [Display(Name = "Isoniazid")]
        public string Isoniazid { get; set; }

        [Display(Name = "Rifampicin")]
        public string Rifampicin { get; set; }

        [Display(Name = "Pyrazinamide")]
        public string Pyrazinamide { get; set; }

        [Display(Name = "Ethambutol")]
        public string Ethambutol { get; set; }

        [Display(Name = "Aminoglycoside")]
        public string Aminoglycoside { get; set; }

        [Display(Name = "Quinolone")]
        public string Quinolone { get; set; }

        [Display(Name = "MDR")]
        public string MDR { get; set; }

        [Display(Name = "XDR")]
        public string XDR { get; set; }

        [Display(Name = "Match Method")]
        public string MatchMethod { get; set; }
    }
}
