using System.Globalization;
using System.Web;
using Hospital_Management.Data;
using Hospital_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management.Controllers;

public class RegisterController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public RegisterController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager,
        ApplicationDbContext dbContext, SignInManager<IdentityUser> signInManager, IWebHostEnvironment environment)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _signInManager = signInManager;
        _environment = environment;
    }

    [HttpPost("/registerDoctor")]
    public async Task<IActionResult> Register([FromForm] string name, [FromForm] string email,
        [FromForm] string address,
        [FromForm] string birthdate, [FromForm] string gender,
        [FromForm] string cellphoneNumber, [FromForm] string licenseNumber, [FromForm] string password,
        [FromForm] IFormFile? image)
    {
        if (await _userManager.FindByEmailAsync(email) is { } user)
        {
            return Redirect($"/registerDoctor?error={HttpUtility.UrlEncode("Email already registered!")}");
        }

        var DoctorImage = string.Empty;
        if (image is { })
        {
            var path = Path.Combine(_environment.WebRootPath, "images", "doctors");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, image.FileName);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await image.CopyToAsync(stream);
                stream.Close();
            }

            DoctorImage = $"/images/doctors/{image.FileName}";
        }

        var Doctor = new Doctor()
        {
            Name = name, Address = address, Gender = gender, Birthdate = birthdate, CellphoneNumber = cellphoneNumber,
            LicenseNumber = licenseNumber, UserName = name, DoctorImage = DoctorImage, Email = email
        };

        var newEntity = await _dbContext.Doctors.AddAsync(Doctor);

        var registerResult = await _userManager.CreateAsync(Doctor, password);

        if (!registerResult.Succeeded)
            return Content(registerResult.Errors.FirstOrDefault().Description);
        await _userManager.AddToRoleAsync(newEntity.Entity, "Doctor");

        await _dbContext.SaveChangesAsync();

        await _signInManager.SignInAsync(newEntity.Entity, true);

        return RedirectPermanent("/");
    }

    [HttpPost("/registerPatient")]
    public async Task<IActionResult> RegisterPatient([FromForm] string name, [FromForm] string address,
        [FromForm] string birthdate, [FromForm] string gender, [FromForm] string email,
        [FromForm] string cellphoneNumber, [FromForm] string? guardian, [FromForm] string password,
        [FromForm] string? referer)
    {
        if (string.IsNullOrWhiteSpace(guardian))
            guardian = string.Empty;

        if (await _userManager.FindByEmailAsync(email) is { } user)
        {
            return Redirect($"/registerPatient?error={HttpUtility.UrlEncode("Email already registered!")}");
        }

        var Doctor = new Patient()
        {
            Name = name, Address = address, Gender = gender, Birthdate = birthdate, CellphoneNumber = cellphoneNumber,
            Guardian = guardian, UserName = name, Email = email, RefererDoctor = referer
        };

        var newEntity = await _dbContext.Patients.AddAsync(Doctor);

        var registerResult = await _userManager.CreateAsync(Doctor, password);

        if (!registerResult.Succeeded)
            return Content(registerResult.Errors.FirstOrDefault().Description);

        await _userManager.AddToRoleAsync(newEntity.Entity, "Patient");

        await _dbContext.SaveChangesAsync();

        await _signInManager.SignInAsync(newEntity.Entity, true);

        return Redirect("/");
    }

    [HttpPost("/newAppointment")]
    public async Task<IActionResult> RegisterPatient([FromForm] string description, [FromForm] double paid,
        [FromForm] DateTime? date, [FromForm] string[] concerns, [FromForm] int? id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var appointments = await _dbContext.Appointments.ToListAsync();

        foreach (var appointment in appointments)
        {
            var start = appointment.Date;
            var end = start?.AddHours(1);

            if (date >= start && date <= end)
            {
                // There's a current appointment in this time.
                return Redirect(
                    $"/registerAppointment?error=There's an ongoing appointment ongoing at that time until {(end?.ToString("h:mm tt", CultureInfo.InvariantCulture))}!");
            }
        }

        var currentPatient =
            await _dbContext.Patients.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

        var Doctor = new AppointmentModel()
        {
            Note = description, Patient = currentPatient,
            Services = string.Join(",", concerns),
            Date = date, TotalPrice = paid
        };

        if (id is { })
        {
            Doctor.AppointmentId = id;
            _dbContext.Appointments.Update(Doctor);
        }
        else
        {
            Doctor = (await _dbContext.Appointments.AddAsync(Doctor)).Entity;
        }

        await _dbContext.SaveChangesAsync();

        if (User.IsInRole("Admin"))
            return Redirect("/admin/appointments");

        return Redirect($"/Invoice?appointmentId={Doctor.AppointmentId}");
    }
}