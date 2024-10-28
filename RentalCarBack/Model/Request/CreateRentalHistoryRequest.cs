using System;
using System.ComponentModel.DataAnnotations;

namespace RentalCarBack.Model.Request;

public class CreateRentalHistoryRequest
{
    [Required]
    public DateTime RentalDate { get; set; } 

    [Required]
    public DateTime ReturnDate { get; set; } 

    [Required]
    public decimal TotalPrice { get; set; } 

    [Required]
    public bool? PaymentStatus { get; set; } 

    [Required]
    [StringLength(36)]
    public string CustomerId { get; set; }

    [Required]
    [StringLength(36)]
    public string CarId { get; set; } 
}
