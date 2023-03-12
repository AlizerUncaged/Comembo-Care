using System.Diagnostics;
using System.Security.Claims;
using Hospital_Management.Data;
using Hospital_Management.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly RoleCreation _roleCreation;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly IHubContext<ChatHub> _hubContext;

    #region Contents

    public static List<dynamic> servicesData = new List<dynamic>()
    {
        new
        {
            Price = 250,
            Image = "https://upload.wikimedia.org/wikipedia/commons/c/cf/Complete_blood_count_and_differential.jpg",
            Name = "CBC (Complete Blood Count)",
            Description =
                @"A complete blood count, also known as a full blood count, is a set of medical laboratory tests that provide information about the cells in a person's blood. The CBC indicates the counts of white blood cells, red blood cells and platelets, the concentration of hemoglobin, and the hematocrit.
Fasting blood sugar (FBS) measures blood glucose after you have not eaten for at least 8 hours. It is often the first test done to check for prediabetes and diabetes."
        },
        new
        {
            Price = 150,
            Image = "https://labs.selfdecode.com/app/uploads/2019/12/Creatinine-Test-High-Low-Normal-Levels-1.jpg",
            Name = "CREA (Creatine Test)",
            Description = @"What is a creatinine test?
This test measures creatinine levels in blood and/or urine. Creatinine is a waste product made by your muscles as part of regular, everyday activity. Normally, your kidneys filter creatinine from your blood and send it out of the body in your urine. If there is a problem with your kidneys, creatinine can build up in the blood and less will be released in urine. If blood and/or urine creatinine levels are not normal, it can be a sign of kidney disease."
        },
        new
        {
            Price = 250,
            Image =
                "https://bookmerilab.com/tests/wp-content/uploads/2022/05/Liver-damage-such-as-Cirrhosis-and-Fibrosis.-1.png",
            Name = "SGPT (serum glutamate-pyruvate transaminase)",
            Description =
                @"serum glutamate-pyruvate transaminase or serum glutamic-pyruvic transaminase. An enzyme found in the liver and other tissues. A high level of SGPT released into the blood may be a sign of liver damage, cancer, or other diseases."
        },
        new
        {
            Price = 70,
            Image =
                "https://www.news-medical.net/image.axd?picture=2016%2F6%2FUrine_analysis_shutterstock_146356910.jpg",
            Name = "Urine Test",
            Description =
                @"A urinalysis is a test of your urine. It's used to detect and manage a wide range of disorders, such as urinary tract infections, kidney disease and diabetes. A urinalysis involves checking the appearance, concentration and content of urine."
        },
        new
        {
            Price = 150,
            Image =
                "https://post.medicalnewstoday.com/wp-content/uploads/sites/3/2022/05/stool_sample_test_732x549_thumb-732x549.jpg",
            Name = "Stool Exam",
            Description =
                @"A stool test examines a sample of faeces (poo) in the laboratory. There are many different types of stool tests, to check for bowel cancer, gastrointestinal infections and other health conditions."
        },
        new
        {
            Price = 250,
            Image = "https://medlineplus.gov/images/Xray_share.jpg",
            Name = "X-ray",
            Description =
                @"A form of high energy electromagnetic radiation that can pass through most objects, including the body. X-rays travel through the body and strike an x-ray detector (such as radiographic film, or a digital x-ray detector) on the other side of the patient, forming an image that represents the “shadows” of objects inside the body."
        },
        new
        {
            Price = 250,
            Name = "Ultrasound Pelvic",
            Description =
                @"Neuropsychology is concerned with relationships between the brain and behavior. Neuropsychologists conduct evaluations to characterize behavioral and cognitive changes resulting from central nervous system disease or injury, like Parkinson's disease or another movement disorder."
        },
    };

    #endregion

    public HomeController(ILogger<HomeController> logger, RoleCreation roleCreation, ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager, IWebHostEnvironment environment, IHubContext<ChatHub> hubContext)
    {
        _logger = logger;
        _roleCreation = roleCreation;
        _dbContext = dbContext;
        _userManager = userManager;
        _environment = environment;
        _hubContext = hubContext;
    }

    [HttpGet("/tos")]
    public async Task<IActionResult> Tos() => View();
    [HttpGet("/covid")]
    public async Task<IActionResult> Covid() => View();


    [HttpGet("/medicine/view")]
    public async Task<IActionResult> MedicineList() => View();


    [HttpGet("/medicine/update")]
    [HttpGet("/medicine/update/{id}")]
    public async Task<IActionResult> RegisterMedicine(long? id)
    {
        return View(await _dbContext.Medicines.FirstOrDefaultAsync(x => x.Id == id));
    }


    [HttpGet("/admin/appointments")]
    public async Task<IActionResult> AdminAppointments() =>
        View();

    [HttpGet("/clearCookies")]
    public async Task<IActionResult> ClearCookies()
    {
        foreach (var cookie in HttpContext.Request.Cookies)
        {
            HttpContext.Response.Cookies.Delete(cookie.Key);
        }

        return Content("Cookies Cleared");
    }


    [HttpGet("/doctor-invoice")]
    [HttpGet("/check-invoice/{id}")]
    public async Task<IActionResult> Invoicing(long? id)
    {
        if (id is { })
        {
            return View(await _dbContext.Appointments.Include(x => x.Doctor)
                .Include(x => x.Patient).FirstOrDefaultAsync(x => x.AppointmentId == id));
        }

        var currentUser = await _userManager.GetUserAsync(User);

        var currentDoctor =
            await _dbContext.Doctors.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

        var currentAppointment =
            await _dbContext.Appointments.Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Where(x => currentUser.Id == x.Doctor.Id).ToListAsync();

        var currentActiveAppointment = currentAppointment.FirstOrDefault(x =>
        {
            var difference = DateTime.Now - x.Date;
            if (difference?.TotalMinutes <= 30 && difference?.TotalMinutes >= -30)
                return true;

            return false;
        });

        return View(currentActiveAppointment);
    }

    [HttpGet("/patients/edit/{id}")]
    public async Task<IActionResult> PatientEditor(string id)
    {
        ViewData["Patient"] =
            await _dbContext.Patients.Include(x => x.Appointments).FirstOrDefaultAsync(x => x.Id == id);

        return View(ViewData["Patient"]);
    }

    [HttpPost("/patients/edit")]
    public async Task<IActionResult> PatientEditor(Patient patient, [FromForm] IFormFile? image,
        [FromForm] IFormFile? prescriptionImage)
    {
        var samePatient = await _dbContext.Patients.FirstOrDefaultAsync(x => x.Id == patient.Id);
        samePatient.Name = patient.Name;
        samePatient.UserName = patient.Name;
        samePatient.Address = patient.Address;
        samePatient.Birthdate = patient.Birthdate;
        samePatient.CellphoneNumber = patient.CellphoneNumber;
        samePatient.Email = patient.Email;
        samePatient.Guardian = patient.Guardian;
        samePatient.PulseRate = patient.PulseRate;
        samePatient.BloodPressure = patient.BloodPressure;
        samePatient.Allergy = patient.Allergy;

        if (image is { })
        {
            samePatient.Image = $"/images/clients/{image.FileName}";

            {
                var path = Path.Combine(_environment.WebRootPath, "images", "clients");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, image.FileName);

                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                    stream.Close();
                }
            }
        }

        if (prescriptionImage is { })
        {
            samePatient.PrescriptionImage = $"/images/clients/{prescriptionImage.FileName}";

            {
                var path = Path.Combine(_environment.WebRootPath, "images", "clients");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, prescriptionImage.FileName);

                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await prescriptionImage.CopyToAsync(stream);
                    stream.Close();
                }
            }
        }


        _dbContext.Patients.Update(samePatient);
        await _dbContext.SaveChangesAsync();

        return Redirect("/doctor-patients");
    }

    [HttpGet("/doctor-patients")]
    public async Task<IActionResult> DoctorPatientList()
    {
        return View();
    }

    [HttpGet("/register")]
    public async Task<IActionResult> RegisterDoctor()
    {
        return View();
    }

    [HttpGet("/smile-makeovers")]
    public async Task<IActionResult> SmileMakeovers()
    {
        return View();
    }

    [HttpGet("/invoice")]
    public async Task<IActionResult> Receipt([FromQuery] int appointmentId)
    {
        // var user = await _userManager.GetUserAsync(User);
        // var firstName = user.UserName;
        // var change = paid - 600;
        //
        // ViewData["FName"] = firstName;
        // ViewData["Change"] = change;
        // ViewData["Paid"] = paid;

        return View(await _dbContext.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId));
    }


    [HttpGet("/aboutus")]
    public async Task<IActionResult> AboutUs()
    {
        return View();
    }

    [HttpPost("/appointments/add/medicine/{appointmentId}")]
    public async Task<IActionResult> AddMedicine(int appointmentId, [FromForm] string medicineName,
        [FromForm] long medicineQuantity)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        var services = appointment
            .Medicines.Split(",").ToList();

        services.Add(medicineName + ":" + medicineQuantity);

        appointment.Medicines = string.Join(",", services);

        _dbContext.Appointments.Update(appointment);

        await _dbContext.SaveChangesAsync();

        return Redirect("/doctor-invoice");
    }


    [HttpPost("/appointments/add/service/{appointmentId}")]
    public async Task<IActionResult> AddAppointment(int appointmentId, [FromForm] string serviceData,
        [FromForm] string serviceresult)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        var services = appointment
            .Services.Split(",").ToList();

        services.Add(serviceData + ":" + serviceresult.Replace(":", "<dand>"));

        appointment.Services = string.Join(",", services);

        _dbContext.Appointments.Update(appointment);

        await _dbContext.SaveChangesAsync();

        return Redirect("/doctor-invoice");
    }

    [HttpPost("/appointments/done/{appointmentId}")]
    public async Task<IActionResult> DoneAppointment(int appointmentId)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        appointment.IsDone = true;

        _dbContext.Appointments.Update(appointment);

        await _dbContext.SaveChangesAsync();

        return Redirect("/Doctor-dashboard");
    }

    [HttpGet("/patients")]
    public async Task<IActionResult> PatientList()
    {
        return View();
    }

    [HttpGet("/Doctors")]
    public async Task<IActionResult> DoctorList()
    {
        return View();
    }

    [HttpGet("/patient-messages/")]
    [HttpGet("/patient-messages/{targetId}")]
    public async Task<IActionResult> PatientChatList(string? targetId = null)
    {
        ViewData["Target"] = targetId;

        var targetUser = await _userManager.FindByIdAsync(targetId);
        return View(targetUser);
    }

    [HttpGet("/messages")]
    public async Task<IActionResult> AdminMessaging()
    {
        return View();
    }

    [HttpGet("/services/view/{name}")]
    public async Task<IActionResult> ServiceCheck(string name)
    {
        var data = servicesData.FirstOrDefault(x =>
            (x.Name as string).Equals(name, StringComparison.InvariantCultureIgnoreCase));
        return View(data);
    }

    [HttpGet("/services")]
    public async Task<IActionResult> Services()
    {
        return View(servicesData);
    }

    [HttpGet("/registerPatient")]
    public async Task<IActionResult> RegisterPatient()
    {
        return View();
    }

    [HttpGet("/appointments/edit/{id}")]
    [HttpGet("/registerAppointment")]
    public async Task<IActionResult> Appointment(int? id)
    {
        var existingAppointment = await _dbContext.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);
        return View(existingAppointment);
    }

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        return View();
    }

    [HttpGet("/Doctor-dashboard")]
    public async Task<IActionResult> DoctorAppointment()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var currentDoctor =
            await _dbContext.Doctors.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

        ViewData["Doctor"] = currentDoctor;

        return View();
    }

    // [HttpGet("/Doctor-notification")]
    // public async Task<IActionResult> DoctorNotification()
    // {
    //     return View();
    // }

    [HttpPost("/Doctor-notification")]
    public async Task<IActionResult> DoctorNotificationPost([FromForm] string notificationText,
        [FromForm] string patientId)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", patientId, notificationText);
        return Redirect("/doctor-patients");
    }

    [HttpGet("/patientdashboard")]
    public async Task<IActionResult> Patient()
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var currentPatient =
                await _dbContext.Patients.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

            ViewData["Patient"] = currentPatient;
            return View();
        }
        catch
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return Content("Refresh the page...");
        }
    }

    public async Task<bool> IsUserValid(ClaimsPrincipal user)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
            return false;

        // Get the user's email from their claims
        var emailClaim = user.FindFirst(ClaimTypes.Email);
        if (emailClaim == null)
        {
            return false;
        }

        // Look up the user in the database by their email
        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == emailClaim.Value);
        if (userInDb == null)
        {
            return false;
        }

        // The user was found in the database, so they are valid
        return true;
    }

    [HttpGet("/install")]
    public async Task<IActionResult> Install()
    {
        await _roleCreation.CreateRolesAsync();

        return Content("Database Reset!");
    }

    public async Task<IActionResult> Index()
    {
        await _roleCreation.CreateRolesAsync();

        if (!await IsUserValid(User))
        {
            await HttpContext.SignOutAsync();
            return View();
        }

        // Send to dashboards.
        if (User.IsInRole("Doctor"))
        {
            return Redirect("/Doctor-dashboard");
        }

        if (User.IsInRole("Patient"))
        {
            return Redirect("/patientdashboard");
        }

        if (User.IsInRole("Admin"))
        {
            return Redirect("/dashboard");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}