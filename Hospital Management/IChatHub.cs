namespace Hospital_Management;

public interface IChatHub
{
    Task SendMessage(string type, string user, string message);
    Task SendNotification(string type, string user, string message);
}