using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Spring2026_Project3_jcasuru.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = default!;
        [Required]
        [Display(Name = "IMDB Page")]
        public string IMDB_Link { get; set; } = default!;
        [Required]
        public string Genre { get; set; } = default!;
        [Display(Name = "Release Year")]
        public int Release_Year { get; set; }
        [Display(Name = "Movie Poster")]
        public byte[]? Movie_Poster { get; set; }
        [NotMapped]
        public IFormFile? PhotoFile { get; set; }  // only used by the form
    }
}
