using Hospital_Management.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Patient> Patients { get; set; }

    public DbSet<Doctor> Doctors { get; set; }

    public DbSet<Chat> Chats { get; set; }
    
    public DbSet<Medicine> Medicines { get; set; }

    public DbSet<AppointmentPayment> MedicinePayments { get; set; }

    public DbSet<AppointmentModel> Appointments { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}