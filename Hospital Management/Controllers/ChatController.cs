using System.Diagnostics;
using Hospital_Management.Data;
using Hospital_Management.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management.Controllers;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, ILogger<ChatController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("/chat/{patientId}")]
    public async Task<IActionResult> OneToOneChat(string patientId)
    {
        var patientToChat = await _userManager.FindByIdAsync(patientId);
        return View(patientToChat);
    }

    [HttpPost("/chat/{userId}")]
    public async Task<IActionResult> RecordChat([FromQuery] string message, string userId)
    {
        var currentSignedInUser = await _userManager.GetUserAsync(User);
        var targetUser = await _userManager.FindByIdAsync(userId);

        await _dbContext.Chats.AddAsync(new Chat()
        {
            To = targetUser, From = currentSignedInUser, Content = message 
        });

        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation($"Chat to {targetUser.UserName}: {message}");
        
        return Content("OK!");
    }
}