using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarBack.Model;

[Table("TrRental", Schema = "dbo")]
public class TrRental
{
    [Key]
    [Column("Rental_id")]
    [MaxLength(36)]
    public string RentalId { get; set; }

    [Column("rental_date")]
    public DateTime RentalDate { get; set; } 

    [Column("return_date")]
    public DateTime? ReturnDate { get; set; } 

    [Column("total_price")]
    public decimal? TotalPrice { get; set; } 

    [Column("payment_status")]
    public bool? PaymentStatus { get; set; } 

    [ForeignKey("MsCustomer")]
    [Column("customer_id")]
    [MaxLength(36)]
    public string CustomerId { get; set; }
    public MsCustomer MsCustomer { get; set; } 

    [ForeignKey("MsCar")]
    [Column("car_id")]
    [MaxLength(36)]
    public string CarId { get; set; }
    public MsCar MsCar { get; set; } 
}