using Microsoft.AspNetCore.Identity;

namespace Hospital_Management.Models;

public class Chat
{
    public long Id { get; set; }

    public IdentityUser? From { get; set; }

    public IdentityUser? To { get; set; }

    public DateTime SentTime { get; set; } = DateTime.Now;

    public string? Content { get; set; }
}