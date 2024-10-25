using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarBack.Model;

[Table("MsCustomer", Schema = "dbo")]
public class MsCustomer
{
    [Key]
    [Column("Customer_id")]
    [MaxLength(36)]
    public string CustomerId { get; set; }

    [Column("email")]
    [MaxLength(100)]
    public string Email { get; set; }

    [Column("password")]
    [MaxLength(100)]
    public string Password { get; set; }

    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("phone_number")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
    [MaxLength(50)]
    public string PhoneNumber { get; set; }

    [Column("address")]
    [MaxLength(500)]
    public string Address { get; set; }

    [Column("driver_license_number")]
    [MaxLength(100)]
    public string DriverLicenseNumber { get; set; }

    public ICollection<TrRental> TrRentals{get; set; }
}
