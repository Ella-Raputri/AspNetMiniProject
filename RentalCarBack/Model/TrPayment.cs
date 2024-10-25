using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarBack.Model;

[Table("TrPayment", Schema = "dbo")]
public class TrPayment
{
    [Key]
    [Column("Payment_id")]
    [MaxLength(36)]
    public string PaymentId { get; set; }

    [Column("payment_date")]
    public DateTime? PaymentDate { get; set; } 

    [Column("amount")]
    public decimal? Amount { get; set; } 

    [Column("payment_method")]
    [MaxLength(100)]
    public string PaymentMethod { get; set; } 

    [ForeignKey("TrRental")]
    [Column("rental_id")]
    [MaxLength(36)]
    public string RentalId { get; set; }
    public TrRental TrRental { get; set; } 
}

