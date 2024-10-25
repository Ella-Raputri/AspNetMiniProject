using System;

namespace RentalCarBack.Model.Result;

public class GetCarInformation
{
    public string CarId { get; set; }
    public string CarImageLink { get; set; }
    public string Model { get; set; }
    public string CarName { get; set; }
    public string Transmission { get; set; }
    public int NumberOfCarSeats { get; set; }
    public decimal PricePerDay { get; set; } 
    public string CustomerName { get; set; }
    public DateTime dateStart { get; set; }
    public DateTime dateEnd { get; set; }
    public decimal totalPrice { get; set; }

}
