using System;

namespace RentalCarFront.Models.Input;

public class LoginUserRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
