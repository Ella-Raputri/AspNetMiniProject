using System;

namespace RentalCarBack.Model.Result;

public class LoginResponse
{
    public string Message { get; set; }
    public GetCustomerInformation UserData { get; set; } // Adjust according to your user data structure


}