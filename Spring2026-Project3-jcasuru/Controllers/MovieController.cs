using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Spring2026_Project3_jcasuru.Data;
using Spring2026_Project3_jcasuru.Models;
using Spring2026_Project3_jcasuru.Models.ViewModels;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using VaderSharp2;

namespace Spring2026_Project3_jcasuru.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public MovieController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> Photo(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(s => s.Id == id);

            if (movie == null || movie.Movie_Poster == null)
            {
                return NotFound();
            }

            return File(movie.Movie_Poster, "image/jpg");
        }

        //GET Create View
        public async Task<IActionResult> Create()
        {
            return View();
        }

        //POST Create Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([BindAttribute("Id,Title,IMDB_Link,Genre,Release_Year,PhotoFile")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                if (movie.PhotoFile != null)
                {
                    using var memoryStream = new MemoryStream();
                    await movie.PhotoFile.CopyToAsync(memoryStream);
                    movie.Movie_Poster = memoryStream.ToArray();
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }


        private const string AiDeployment = "gpt-4.1-mini";
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var actors = await _context.ActorsMovies
                .Include(m => m.Actor)
                .Where(m => m.MovieId == id)
                .Select(m => m.Actor!)
                .ToListAsync();


            //GET Reviews AI

            var ApiEndpoint = new Uri("https://spring2026-project3-jcasuru-aaif.services.ai.azure.com/");
            var ApiCredential = new ApiKeyCredential(_config["AI_Credentials:API_Key"]!);

            ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

            string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy" };
            var messages = new ChatMessage[]
            {
            new SystemChatMessage($"You represent a group of {personas.Length} film critics who have the following personalities: {string.Join(",", personas)}. When you receive a question, respond as each member of the group with each response separated by a '|', but don't indicate which member you are."),
            new UserChatMessage($"How would you rate the movie {movie.Title} released in {movie.Release_Year} out of 10 in 150 words or less?")
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
            string[] reviews = result.Value.Content[0].Text.Split('|').Select(s => s.Trim()).ToArray();

            double[] ReviewSentiments = new double[reviews.Length];
            
            var analyzer = new SentimentIntensityAnalyzer();
            double sentimentTotal = 0;
            for (int i = 0; i < reviews.Length; i++)
            {
                string review = reviews[i];
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);
                sentimentTotal += sentiment.Compound;
                ReviewSentiments[i] = sentiment.Compound;

                //Console.WriteLine($"Review {i + 1} (sentiment {sentiment.Compound})");
                //Console.WriteLine(review);
                //Console.WriteLine();
            }

            double sentimentAverage = sentimentTotal / reviews.Length;
            var vm = new MovieDetailsViewModel()
            {
                Movie = movie,
                Actors = actors,
                MovieReviews = reviews,
                ReviewSentiments=ReviewSentiments,
                OverAllSentiment = sentimentAverage
            };
            //Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");
            return View(vm);
        }

        //GET Edit page
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,IMDB_Link,Genre,Release_Year, PhotoFile")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (movie.PhotoFile != null)
                    {
                        using var memoryStream = new MemoryStream();
                        await movie.PhotoFile.CopyToAsync(memoryStream);
                        movie.Movie_Poster = memoryStream.ToArray();
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)//Check if some one is delteing the nentry you're trying to edit
                {
                    if (!MovieExist(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        public bool MovieExist(int id) 
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        //GET Delet View
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
          
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            var actors = _context.ActorsMovies
                .Where(a => a.Id == id);
            foreach (ActorMovie am in actors)
            {
                _context.ActorsMovies.Remove(am);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
