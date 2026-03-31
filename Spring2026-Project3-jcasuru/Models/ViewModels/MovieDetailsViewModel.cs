namespace Spring2026_Project3_jcasuru.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public required Movie Movie { get; set; }
        public required IEnumerable<Actor> Actors { get; set; }
        public required string[] MovieReviews { get; set; }
        public required double[] ReviewSentiments { get; set; }
        public  required double OverAllSentiment { get; set; }
    }
}
