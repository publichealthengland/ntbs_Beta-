using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ntbs_service.Models.Enums 
{
    public enum AlertType 
    {
        [Display(Name = "Enhanced surveillance - MDR")]
        EnhancedSurveillanceMDR,
        EnhancedSurveillanceMBovis,
        MissingTreatmentOutcome,
        UnmatchedLabResult,
        [Display(Name = "Transfer Request")]
        TransferRequest,
        TransferRejected,
        DataQualityIssue,
        SocialContext,
        [Display(Name = "Test Alert")]
        Test
    }
}