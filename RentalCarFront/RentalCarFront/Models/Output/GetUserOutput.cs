using System;

namespace RentalCarFront.Models.Output;

public class GetUserOutput
{
    public string CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}
