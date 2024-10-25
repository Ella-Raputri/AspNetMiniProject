using System;

namespace RentalCarBack.Model.Result;

public class GetCustomerInformation
{
    public string CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }

}
