

namespace Spring2026_Project3_jcasuru.Models.ViewModels
{
    public class ActorDetailsViewModel
    {
        public required Actor Actor {  get; set; }
        public required IEnumerable<Movie> Movies { get; set; }
        public required string[] Tweets { get; set; }
        public required double[] TweetSentiments { get; set; }
        public required double OverAllActorSentiment { get; set; }
    }
}
