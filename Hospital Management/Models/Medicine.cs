namespace Hospital_Management.Models;

public class Medicine
{
    public long Id { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public double Price { get; set; } = 0;

    public DateTime DateAdded { get; set; } = DateTime.Now;
    
    public DateTime ExpiryDate { get; set; }

    public string? Image { get; set; }

    public long Quantity { get; set; } = 0;
}