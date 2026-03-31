using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Spring2026_Project3_jcasuru.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;
        [Required]
        public string Gender { get; set; } = default!;
        public int Age { get; set; }
        [Required]
        [Display(Name ="IMDB Page")]
        public string IMDB_Link { get; set; } = default!;
        public byte[]? Photo {  get; set; }
        [NotMapped]
        public IFormFile? PhotoFile { get; set; }  // only used by the form
    }
}
