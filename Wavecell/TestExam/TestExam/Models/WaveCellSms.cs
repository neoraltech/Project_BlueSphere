using System.ComponentModel.DataAnnotations;

namespace TestExam.Models
{
    public enum SendStatus
    {
        Success,
        ErrorSource,
        ErrorDestination,
        ErrorBody,
        Failed
    }

    public class WaveCellSms
    {
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Source must be AlphaNumeric.")]
        [DataType(DataType.Text)]
        [Display(Name = "Source")]
        public string Source { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression("^[0-9\\+]*$", ErrorMessage ="Destination must be Numeric.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Destination")]
        public string Destination { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Body")]
        public string Body { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public string Date { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Time")]
        public string Time { get; set; }
    }

    public class WaveCellResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}