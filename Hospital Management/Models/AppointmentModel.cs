using System.ComponentModel.DataAnnotations;

namespace Hospital_Management.Models
{
    public class AppointmentModel
    {
        [Key]
        [Display(Name = "Appointment ID")]
        public int? AppointmentId { get; set; }

        public DateTime? DateCreated { get; set; } = DateTime.Now;

        [DataType(DataType.Date)] 
        [Display(Name = "Appointment Date")]
        public DateTime? Date { get; set; }

        [Display(Name = "Expires At")]
        public DateTime? ExpiresAt
        {
            get
            {
                var toAdd = Date;
                var addMinutes = toAdd?.AddMinutes(30);
                return addMinutes;
            }
        }

        public Doctor? Doctor { get; set; } = null;

        public Patient? Patient { get; set; }

        public string MessageToPatient { get; set; } = string.Empty;

        public double? TotalPrice { get; set; }

        /// <summary>
        /// Comma delimited services.
        /// </summary>
        public string Services { get; set; } = string.Empty;

        public string? Note { get; set; }
        
        public bool IsDone { get; set; } = false;
        public bool IsCancelled { get; set; } = false;
    }
}