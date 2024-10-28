using System;
using System.ComponentModel.DataAnnotations;

namespace RentalCarBack.Model.Request;

public class CreatePaymentRequest
{
    [Required]
    public DateTime PaymentDate { get; set; } 

    [Required]
    public decimal Amount { get; set; } 

    [Required]
    public string PaymentMethod { get; set; } 

    [Required]
    [StringLength(36)]
    public string RentalId { get; set; } 
}
