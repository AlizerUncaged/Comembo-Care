using Hospital_Management.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management.Controllers;

public class AppointmentController : Controller
{
    private readonly ILogger<AppointmentController> _logger;
    private readonly RoleCreation _roleCreation;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public AppointmentController(ILogger<AppointmentController> logger, RoleCreation roleCreation, ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _roleCreation = roleCreation;
        _dbContext = dbContext;
        _userManager = userManager;
    }

    
    [HttpGet("/delete/{appointmentId}")]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        var appointment = await _dbContext.Appointments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        _dbContext.Appointments.Remove(appointment);

        await _dbContext.SaveChangesAsync();

        return Redirect("/");
    }

    [HttpGet("/cancel/{appointmentId}/{reason}")]
    public async Task<IActionResult> CancelAppointment(int appointmentId, string reason)
    {
        var appointment = await _dbContext.Appointments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        appointment.IsCancelled = true;

        _dbContext.Appointments.Update(appointment);

        await _dbContext.SaveChangesAsync();

        return Redirect("/Doctor-dashboard");
    }

    [HttpGet("/approve/{appointmentId}")]
    public async Task<IActionResult> AcceptAppointment(int appointmentId)
    {
        var appointment = await _dbContext.Appointments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        appointment.IsCancelled = false;

        var currentUser = await _userManager.GetUserAsync(User);

        appointment.Doctor = await _dbContext.Doctors.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

        _dbContext.Appointments.Update(appointment);

        await _dbContext.SaveChangesAsync();

        return Redirect("/Doctor-dashboard");
    }
}