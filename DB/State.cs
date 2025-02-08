using System.ComponentModel.DataAnnotations;

namespace ImageOperation.DB
{
    public class State
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
