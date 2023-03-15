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
        [FromForm] IFormFile? image, [FromForm] string[] concerns)
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

        var doctor = new Doctor()
        {
            Name = name, Address = address, Gender = gender, Birthdate = birthdate, CellphoneNumber = cellphoneNumber,
            LicenseNumber = licenseNumber, UserName = name, DoctorImage = DoctorImage, Email = email
        };

        var services = await _dbContext.Services.ToListAsync();

        foreach (var concern in concerns)
        {
            doctor.AllowedServices.Add(services.FirstOrDefault(x => x.Id == long.Parse(concern)));
        }

        var newEntity = await _dbContext.Doctors.AddAsync(doctor);

        var registerResult = await _userManager.CreateAsync(doctor, password);

        if (!registerResult.Succeeded)
            return Content(registerResult.Errors.FirstOrDefault().Description);
        await _userManager.AddToRoleAsync(newEntity.Entity, "Doctor");

        await _dbContext.SaveChangesAsync();


        return RedirectPermanent("/");
    }

    [HttpPost("/registerMedicine")]
    public async Task<IActionResult> Register(
        [FromForm] string? name,
        [FromForm] long? id,
        [FromForm] long? quantity,
        [FromForm] double? price,
        [FromForm] IFormFile? image,
        [FromForm] DateTime? expiry
    )
    {
        string medicineImage = null;
        if (image is { })
        {
            var path = Path.Combine(_environment.WebRootPath, "images", "med");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, image.FileName);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await image.CopyToAsync(stream);
                stream.Close();
            }

            medicineImage = $"/images/med/{image.FileName}";
        }

        var medicine = await _dbContext.Medicines.FirstOrDefaultAsync(x => x.Id == id);

        if (medicine is { }) // update
        {
            if (name is { })
                medicine.MedicineName = name;

            if (quantity is { })
                medicine.Quantity = quantity.Value;

            if (price is { })
                medicine.Price = price.Value;

            if (image is { })
                medicine.Image = medicineImage;

            if (expiry is { })
                medicine.ExpiryDate = expiry.GetValueOrDefault();

            _dbContext.Medicines.Update(medicine);
        }
        else
        {
            medicine ??= new Medicine();

            if (name is { })
                medicine.MedicineName = name;

            if (quantity is { })
                medicine.Quantity = quantity.Value;

            if (price is { })
                medicine.Price = price.Value;

            if (image is { })
                medicine.Image = medicineImage;

            if (expiry is { })
                medicine.ExpiryDate = expiry.GetValueOrDefault();

            await _dbContext.Medicines.AddAsync(medicine);
        }


        await _dbContext.SaveChangesAsync();

        return RedirectPermanent("/medicine/view");
    }

    [HttpPost("/registerService")]
    public async Task<IActionResult> Register(
        [FromForm] string? name,
        [FromForm] string? moreInfo,
        [FromForm] long? id,
        [FromForm] string? description,
        [FromForm] double? price
    )
    {
        var medicine = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);

        if (medicine is { }) // update
        {
            if (name is { })
                medicine.Name = name;

            if (description is { })
                medicine.Description = description;

            if (price is { })
                medicine.Price = price.Value;

            if (moreInfo is { })
                medicine.MoreInfo = moreInfo;

            _dbContext.Services.Update(medicine);
        }
        else
        {
            medicine ??= new Service();

            if (name is { })
                medicine.Name = name;

            if (description is { })
                medicine.Description = description;

            if (price is { })
                medicine.Price = price.Value;

            if (moreInfo is { })
                medicine.MoreInfo = moreInfo;

            await _dbContext.Services.AddAsync(medicine);
        }


        await _dbContext.SaveChangesAsync();

        return RedirectPermanent("/services/view");
    }

    [HttpPost("/registerPatient")]
    public async Task<IActionResult> RegisterPatient([FromForm] string name, [FromForm] string address,
        [FromForm] string birthdate, [FromForm] string gender, [FromForm] string email,
        [FromForm] string cellphoneNumber, [FromForm] string? guardian, [FromForm] string password,
        [FromForm] string? referer)
    {
        if (string.IsNullOrWhiteSpace(guardian))
            guardian = string.Empty;
        
        if (!name.Contains(" "))
            return Redirect($"/registerPatient?error={HttpUtility.UrlEncode("Full name required.")}");

        if (cellphoneNumber.Length != 11)
            return Redirect($"/registerPatient?error={HttpUtility.UrlEncode("Phone number requires 11 characters.")}");


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


        return Redirect("/");
    }

    [HttpPost("/newAppointment")]
    public async Task<IActionResult> RegisterPatient([FromForm] string description, [FromForm] double paid,
        [FromForm] DateTime? date, [FromForm] string[] concerns, [FromForm] int? id, [FromForm] string? doctorId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        TimeSpan startTime =  TimeSpan.Parse($"{8}:00"); 
        TimeSpan endEnd =  TimeSpan.Parse($"{12 + 5}:00"); 
        
        if ((date?.TimeOfDay > startTime) && (date?.TimeOfDay < endEnd))
        {          return Redirect(
            $"/registerAppointment?error=You can only register an appointment at 8AM till 5PM");

        }
        
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

        var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(x => x.Id == doctorId);

        var currentPatient =
            await _dbContext.Patients.FirstOrDefaultAsync(x => x.Id == currentUser.Id);
        var allServices = await _dbContext.Services.ToListAsync();

        var appointmentModel = new AppointmentModel()
        {
            Note = description, Patient = currentPatient,
            Services = string.Join(",",
                concerns.Select(x => $"{x}:{allServices.FirstOrDefault(y => y.Name == x).Price}:OK")),
            Date = date,
            TotalPrice = paid,
            Doctor = doctor
        };

        if (id is { })
        {
            appointmentModel.AppointmentId = id;
            _dbContext.Appointments.Update(appointmentModel);
        }
        else
        {
            appointmentModel = (await _dbContext.Appointments.AddAsync(appointmentModel)).Entity;
        }

        await _dbContext.SaveChangesAsync();

        if (User.IsInRole("Admin"))
            return Redirect("/admin/appointments");

        return Redirect($"/Invoice?appointmentId={appointmentModel.AppointmentId}");
    }
}