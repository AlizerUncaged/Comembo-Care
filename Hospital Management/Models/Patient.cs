using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class Patient : IdentityUser
    {
        public bool Blocked { get; set; }

        public virtual List<AppointmentModel> Appointments { get; set; } = new List<AppointmentModel>();

        [FromForm()] public string? Name { get; set; }

        public string? FullName => UserName;

        [FromForm()] public string? Address { get; set; }

        // [DataType(DataType.Date)]
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString =  "{0:yyyy-MM-ddTHH:mm}")]
        [FromForm()]
        public string? Birthdate { get; set; }
        
        [FromForm()] 
        public string? RefererDoctor { get; set; }
        

        [FromForm()] public string? Gender { get; set; }

        [FromForm()] public string? Guardian { get; set; }

        [Display(Name = "Cellphone Number")] public string? CellphoneNumber { get; set; }

        [FromForm()] public string? Tag { get; set; }

        [Display(Name = "Pulse Rate")] public string? PulseRate { get; set; }

        [Display(Name = "Blood Pressure")] public string? BloodPressure { get; set; }
        public string? Allergy { get; set; }
        public string? Image { get; set; }

        [Display(Name = "Prescription Image")] public string? PrescriptionImage { get; set; }

        public List<Chat> Chats { get; set; } = new();

        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
}