using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spring2026_Project3_jcasuru.Models
{
    public class ActorMovie
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Actor")]
        [Display(Name ="Actor Id")]
        public int  ActorId { get; set; }
        public Actor? Actor { get; set; }
        [ForeignKey("Movie")]
        [Display(Name = "Movie Id")]
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
    }
}
