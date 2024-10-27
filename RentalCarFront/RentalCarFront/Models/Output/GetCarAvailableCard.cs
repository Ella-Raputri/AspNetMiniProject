using System;

namespace RentalCarFront.Models.Output;

public class GetCarAvailableCard
{
    public string CarId { get; set; }
    public string Name { get; set; }
    public string CarImageLink { get; set; }
    public decimal PricePerDay { get; set; } 
}
