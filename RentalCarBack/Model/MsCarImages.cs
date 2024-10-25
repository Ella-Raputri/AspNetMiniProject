using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarBack.Model;

[Table("MsCarImages", Schema = "dbo")]
public class MsCarImages
{
    [Key]
    [Column("Image_car_id")]
    [MaxLength(36)]
    public string ImageCarId { get; set; }

    [ForeignKey("MsCar")]
    [Column("Car_id")]
    [MaxLength(36)]
    public string CarId { get; set; }
    public MsCar MsCar { get; set; }

    [Column("image_link")]
    [MaxLength(2000)]
    public string ImageLink { get; set; }


}
