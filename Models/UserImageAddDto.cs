using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace ImageOperation.Models
{
    public class UserImageAddDto
    {

        public string Name { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        public string? ImagePath { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }

        public List<SelectListItem> States { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    }
}
