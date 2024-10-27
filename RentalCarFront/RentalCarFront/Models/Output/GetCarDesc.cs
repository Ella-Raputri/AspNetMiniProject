using System;

namespace RentalCarFront.Models.Output;

public class GetCarDesc
{
    public string CarId { get; set; }
    public string CarImageLink { get; set; }
    public string Model { get; set; }
    public string CarName { get; set; }
    public string Transmission { get; set; }
    public int NumberOfCarSeats { get; set; }
    public decimal PricePerDay { get; set; } 
    public string CustomerName { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public decimal TotalPrice { get; set; }
}
