using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace ImageOperation.DB
{
    public class UserWithImage
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public byte[]? Image { get; set; }
        public string ImagePath { get; set; }
        [ForeignKey("stateId")]
        public int? StateId { get; set; }
        public virtual State State { get; set; }
        [ForeignKey("cityId")]
        public int? CityId{ get; set; }
        public virtual City City { get; set; }

    }
}
