namespace Hospital_Management.Models;

public class Service
{
    public long Id { get; set; }
    public string? Image { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? MoreInfo { get; set; }

    public double Price { get; set; }
    
    public List<Service> ServicesOffered { get; set; } = new();
}