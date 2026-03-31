using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_jcasuru.Data;
using Spring2026_Project3_jcasuru.Models;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using VaderSharp2;
using Spring2026_Project3_jcasuru.Models.ViewModels;

namespace Spring2026_Project3_jcasuru.Controllers
{
    public class ActorController : Controller
    {

        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public ActorController(ApplicationDbContext context, IConfiguration config)
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

            var actor = await _context.Actors.FirstOrDefaultAsync(s => s.Id == id);

            if (actor == null || actor.Photo == null)
            {
                return NotFound();
            }

            return File(actor.Photo, "image/jpg");
        }

        //GET Request Create
        public IActionResult Create() {
            return View();
        }

        //POST Request Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Age,Gender,IMDB_Link,PhotoFile")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                if (actor.PhotoFile != null)
                {
                    using var memoryStream = new MemoryStream();
                    await actor.PhotoFile.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        public async Task<IActionResult> Index() {
            return View(await _context.Actors.ToListAsync());
        }

        //GET Edit page
        public async Task<IActionResult> Edit(int? id) {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            
            return View(actor);
        }

        //POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Age,Gender,IMDB_Link,PhotoFile")] Actor actor)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (actor.PhotoFile != null)
                    {
                        using var memoryStream = new MemoryStream();
                        await actor.PhotoFile.CopyToAsync(memoryStream);
                        actor.Photo = memoryStream.ToArray();
                    }
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)//Check if some one is delteing the nentry you're trying to edit
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }

        //GET Delete View
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        //POST Delete
        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
            }
            var movies = _context.ActorsMovies
                .Where(a => a.ActorId == id);
            foreach (ActorMovie am in movies)
            {
                _context.ActorsMovies.Remove(am);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        //get actual one ftrom Azure and put credentials in secret manager
        private const string AiDeployment = "gpt-4.1-mini";
        private record class Tweet(string Username, string Text);
        private record class Tweets(Tweet[] Items);
        public async Task<IActionResult> Details(int? id) {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var movies = await _context.ActorsMovies
                .Include(m => m.Movie)
                .Where(m => m.ActorId == id)
                .Select(m => m.Movie!)
                .ToListAsync();



            //Get Tweets from AI
    
            var ApiEndpoint = new Uri("https://spring2026-project3-jcasuru-aaif.services.ai.azure.com/");
            var ApiCredential = new ApiKeyCredential(_config["AI_Credentials:API_Key"]!);
            


            ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

            var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            //string responseSchema = @"
            //{
            //    ""type"": ""object"",
            //    ""properties"": {
            //        ""Items"": {
            //            ""type"": ""array"",
            //            ""items"": {
            //                ""type"": ""object"",
            //                ""properties"": {
            //                    ""Username"": {
            //                        ""type"": ""string"",
            //                        ""description"": ""The username of the tweeter""
            //                    },
            //                    ""Text"": {
            //                        ""type"": ""string"",
            //                        ""description"": ""The content of the tweet""
            //                    }
            //                },
            //                ""required"": [""Username"", ""Text""],
            //                ""additionalProperties"": false
            //            }
            //        }
            //    },
            //    ""required"": [""Items""],
            //    ""additionalProperties"": false
            //}";

            JsonNode schema = options.GetJsonSchemaAsNode(typeof(Tweets), new()
            {
                TreatNullObliviousAsNonNullable = true,
            });

            var chatCompletionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("XTwitterApiJson", BinaryData.FromString(schema.ToString()), jsonSchemaIsStrict: true),
            };
            var messages = new ChatMessage[]
            {
            new SystemChatMessage($"You represent the X/Twitter social media platform API that returns JSON data."),
            new UserChatMessage($"Generate 10 tweets from a variety of users about the actor {actor.Name}.")
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages, chatCompletionOptions);

            string jsonString = result.Value.Content.FirstOrDefault()?.Text ?? @"{""Items"":[]}";
            Tweets tweets = JsonSerializer.Deserialize<Tweets>(jsonString) ?? new([]);

            var analyzer = new SentimentIntensityAnalyzer();
            double sentimentTotal = 0;
            string[] tweetsForDetailsView = new string[tweets.Items.Length];
            double[] sentimentsForDetailsView = new double[tweets.Items.Length];
            int i = 0;
            foreach (var tweet in tweets.Items)
            {
                if (i >= 10) break;
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(tweet.Text);
                sentimentTotal += sentiment.Compound;

                Console.WriteLine($"{tweet.Username}: \"{tweet.Text}\" (sentiment {sentiment.Compound})\n");
                tweetsForDetailsView[i] = $"{tweet.Username}: \"{tweet.Text}\"";
                sentimentsForDetailsView[i] = sentiment.Compound;
                i++;
            }
            double sentimentAverage = sentimentTotal / tweets.Items.Length;
            var vm = new ActorDetailsViewModel()
            {
                Actor = actor,
                Movies = movies,
                Tweets = tweetsForDetailsView,
                TweetSentiments = sentimentsForDetailsView,
                OverAllActorSentiment = sentimentAverage
            };

            return View(vm);
        }

    }
}
