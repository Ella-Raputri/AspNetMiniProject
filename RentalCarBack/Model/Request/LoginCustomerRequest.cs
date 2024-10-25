using System;
using System.ComponentModel.DataAnnotations;

namespace RentalCarBack.Model.Request;

public class LoginCustomerRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Password { get; set; }
}
