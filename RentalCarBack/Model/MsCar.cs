using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarBack.Model;

[Table("MsCar", Schema = "dbo")]
public class MsCar
{
    [Key]
    [Column("Car_id")]
    [MaxLength(36)]
    public string CarId { get; set; }

    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("model")]
    [MaxLength(100)]
    public string Model { get; set; }

    [Column("year")]
    public int? Year { get; set; }

    [Column("license_plate")]
    [MaxLength(50)]
    public string LicensePlate { get; set; }

    [Column("number_of_car_seats")]
    public int? NumberOfCarSeats { get; set; } 

    [Column("transmission")]
    [MaxLength(100)]
    public string Transmission { get; set; }

    [Column("price_per_day")]
    public decimal? PricePerDay { get; set; } 

    [Column("status")]
    public bool? Status { get; set; }

    public MsCarImages CarImage { get; set; }

    public ICollection<TrRental> TrRentals{get; set; }
}
