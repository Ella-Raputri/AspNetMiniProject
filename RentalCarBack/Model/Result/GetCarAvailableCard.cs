using System;

namespace RentalCarBack.Model.Result;

public class GetCarAvailableCard
{
    public string CarId { get; set; }
    public string Name { get; set; }
    public string CarImageLink { get; set; }
    public decimal PricePerDay { get; set; } 
}
