using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_jcasuru.Data;
using Spring2026_Project3_jcasuru.Models;

namespace Spring2026_Project3_jcasuru.Controllers
{
    public class ActorMovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET Create View
        public async Task<IActionResult> Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name");
            return View();
        }

        //POST Create Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, ActorId, MovieId")] ActorMovie actor_movie)
        {
            if (ModelState.IsValid)
            {
                if(!ActorMovieExist(actor_movie.ActorId, actor_movie.MovieId))
                {
                    _context.Add(actor_movie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
               
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actor_movie.MovieId);
            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actor_movie.ActorId);
            return View(actor_movie);
        }

        public bool ActorMovieExist(int Actor_Id, int Movie_Id)
        {
            return _context.ActorsMovies.Any(e => e.ActorId == Actor_Id && e.MovieId == Movie_Id);
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ActorsMovies.Include(c => c.Actor).Include(c => c.Movie);
            return View(await applicationDbContext.ToListAsync());
      
        }

        //GET Details View
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorsMovies
                .Include(c => c.Movie)
                .Include(c => c.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        //GET Edit View
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var actorMovie = await _context.ActorsMovies
                .FindAsync(id);
            if(actorMovie == null)
            {
                return NotFound();
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);
            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            return View(actorMovie);
        }

        //POST Edit Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, ActorId, MovieId")] ActorMovie actor_movie)
        {
            if (id != actor_movie.Id) {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor_movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorMovieRowExist(actor_movie.Id))
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
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actor_movie.MovieId);
            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actor_movie.ActorId);
            return View(actor_movie);
        }

        public bool ActorMovieRowExist(int? id)
        {
            return _context.ActorsMovies.Any(e=>e.Id == id);
        }

        //GET Delete View
        public async Task<IActionResult> Delete(int? id) { 
            
            if(id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorsMovies
                .Include(e=>e.Movie)
                .Include(e=>e.Actor)
                .FirstOrDefaultAsync(e => e.Id == id);
            if(actorMovie == null)
            {
                return NotFound();
            }
            return View(actorMovie);
        
        }

        //POST DELETE Action
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorsMovies
                .FindAsync(id);
            if (actorMovie != null)
            {
                _context.ActorsMovies.Remove(actorMovie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
