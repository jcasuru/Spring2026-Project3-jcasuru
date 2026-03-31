using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_jcasuru.Models;

namespace Spring2026_Project3_jcasuru.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
        public DbSet<Actor> Actors { get; set; } = default!;
        public DbSet<Movie> Movies { get; set; } = default!;
        public DbSet<ActorMovie> ActorsMovies { get; set; } = default!;
    }
    
}
