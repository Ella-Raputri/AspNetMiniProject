using System;
using System.ComponentModel.DataAnnotations;

namespace RentalCarBack.Model.Request;

public class CreateCustomerRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    public string Password { get; set; }

    [Required]
    [StringLength(100)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    [StringLength(50)]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(500)]
    public string Address { get; set; }


}
