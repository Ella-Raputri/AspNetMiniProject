using System;

namespace RentalCarFront.Models.Output;

public class GetRiwayatUser
{
    public string RentalDate { get; set; }
    public string CarName { get; set; }
    public decimal PricePerDay { get; set; } 
    public int TotalDays { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Status { get; set; }
}
