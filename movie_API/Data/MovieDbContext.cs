using Microsoft.EntityFrameworkCore;
using movie_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace movie_API.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions options)
            : base(options)
        {}
        public virtual DbSet<Movie> Movies { get; set; }
        public virtual DbSet<Author> Authors { get; set; }

    }
}
