using System.Dynamic;
using Hospital_Management.Models;
using Microsoft.AspNetCore.Identity;

namespace Hospital_Management.Data;

public class RoleCreation
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

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

    public const string DoctorEmail = "Doctor@Doctor.com", PatientEmail = "patient@patient.com";

    public RoleCreation(IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public static bool HasProperty(dynamic obj, string name)
    {
        Type objType = obj.GetType();

        if (objType == typeof(ExpandoObject))
        {
            return ((IDictionary<string, object>)obj).ContainsKey(name);
        }

        return objType.GetProperty(name) != null;
    }

    public async Task CreateRolesAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();


        //initializing custom roles 
        string[] roleNames = { "Admin", "Doctor", "Patient" };

        if (await _roleManager.RoleExistsAsync(roleNames
                .First())) // if a certain role already exists then everything is created
        {
            return;
        }

        foreach (dynamic i in servicesData)
        {
            await _dbContext.Services.AddAsync(new Service()
            {
                Name = i.Name,
                Description = HasProperty(i, "Description") ? i.Description : null,
                Image = HasProperty(i, "Image") ? i.Image : null,
                Price = i.Price
            });
        }
        
        await _dbContext.SaveChangesAsync();

        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                //create the roles and seed them to the database: Question 1
                roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Register sample patient.
        {
            var patient = new Patient()
            {
                Name = "patient", Address = "patient", Gender = "patient", Birthdate = DateTime.Now.ToString(),
                CellphoneNumber = "123",
                Guardian = "patient", UserName = "patient", Email = PatientEmail
            };

            var newEntity = await _dbContext.Patients.AddAsync(patient);

            var registerResult = await _userManager.CreateAsync(patient, "pass123");

            await _userManager.AddToRoleAsync(newEntity.Entity, "Patient");

            await _dbContext.SaveChangesAsync();
        }

        // Register sample doctor.
        {
            var Doctor = new Doctor()
            {
                Name = "Doctor", Address = "Doctor", Gender = "Doctor", Birthdate = DateTime.Now.ToString(),
                CellphoneNumber = "123", Email = DoctorEmail,
                LicenseNumber = "Doctor", UserName = "Doctor", DoctorImage = string.Empty
            };

            var newEntity = await _dbContext.Doctors.AddAsync(Doctor);

            var registerResult = await _userManager.CreateAsync(Doctor, "pass123");

            await _userManager.AddToRoleAsync(newEntity.Entity, "Doctor");

            await _dbContext.SaveChangesAsync();
        }

        // Create admin if not exist yet.
        var powerhouse = new IdentityUser
        {
            UserName = "Admin",
            Email = "Admin@admin.com",
            EmailConfirmed = true,
            TwoFactorEnabled = false
        };

        const string userPwd = "password123";
        var user = await _userManager.FindByEmailAsync("Admin@admin.com");

        if (user == null)
        {
            var createPowerUser = await _userManager.CreateAsync(powerhouse, userPwd);
            if (createPowerUser.Succeeded)
            {
                await _userManager.AddToRoleAsync(powerhouse, "Admin");
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}