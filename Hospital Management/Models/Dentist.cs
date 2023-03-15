using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Hospital_Management.Models
{
    [Table("DoctorsTable")]
    public class Doctor : IdentityUser
    {
        [Display(Name = "Doctor Name")] 
        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? Birthdate { get; set; }

        public string? Gender { get; set; }

        public string? CellphoneNumber { get; set; }

        public string? LicenseNumber { get; set; }

        [Display(Name = "Doctor Name")] public string? DoctorName => UserName;

        public string? DoctorImage { get; set; }

        public List<AppointmentModel> DeclinedAppointments { get; set; } = new();

        public List<Service> AllowedServices { get; set; } = new();
    }
}