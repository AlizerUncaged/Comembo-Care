namespace Hospital_Management.Models;

public class AppointmentPayment
{
    public int Id { get; set; }
    public Patient? Buyer { get; set; }
    
    
    public string? Information { get; set; }
    
    public double PricePaid { get; set; }
}