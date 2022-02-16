using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_API.Data;
using movie_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace movie_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        public MovieController(MovieDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> SetImage(int movieId, IFormFile file)
        {
            string[] Exts = { ".png", ".jpg", ".jpeg", ".jfif" };
            var movie = await _dbContext.Movies
                    .Include(p => p.Author)
                    .FirstOrDefaultAsync(p => p.Id == movieId);
            if (movie is null || file is null)
                return NoContent();
            string fileExt = Path.GetExtension(file.FileName).ToLower();
            string fileName = "Images/" + Guid.NewGuid() + fileExt;
            string path = Path.Combine(_env.WebRootPath, $"{fileName}");
            // is not valid format image
            if (!Exts.Contains(fileExt))
                return BadRequest(@"{""ishkal"" : ""rasm formati noto'g'ri.""}");

            FileStream fileStream = System.IO.File.Open(path, FileMode.Create);
            await file.OpenReadStream().CopyToAsync(fileStream);
            //delete image
            if (!string.IsNullOrEmpty(movie.Image))
                System.IO.File.Delete(Path.Combine(_env.WebRootPath, movie.Image));

            await fileStream.FlushAsync();
            fileStream.Close();
            movie.Image = fileName;
            await _dbContext.SaveChangesAsync();
            return Ok(movie);
        }

        [HttpGet]
        public async Task<IActionResult> GetImageById(int movieId)
        {
            var movie = await _dbContext.Movies
                    .FirstOrDefaultAsync(p => p.Id == movieId);
            if (movie is null)
                return NotFound(@"{'ishkal':'Rasm topilmadi!'}");
            string path = Path.Combine(_env.WebRootPath, movie.Image);
            byte[] file = await System.IO.File.ReadAllBytesAsync(path);
            return File(file, "octet/stream", movie.Image);
        }

        [HttpPost]
        public async Task<IActionResult> SetVideo(int movieId, IFormFile file)
        {
            string[] Exts = { ".mp4", ".mov", ".wmv", ".avi" };
            var movie = await _dbContext.Movies
                .Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == movieId);
            if (movie is null || file is null)
                return NotFound();
            string fileExt = Path.GetExtension(file.FileName).ToLower();
            if (!Exts.Contains(fileExt))
                return BadRequest("Video format not is valid!\n" + String.Join(", ", Exts));
            string fileName = "Videos/" + Guid.NewGuid() + fileExt;
            string path = Path.Combine(_env.WebRootPath, fileName);
            FileStream fileStream = System.IO.File.Open(path, FileMode.Create);
            await file.OpenReadStream().CopyToAsync(fileStream);
            if (!string.IsNullOrEmpty(movie.Video))
                System.IO.File.Delete(Path.Combine(_env.WebRootPath, movie.Video));
            await fileStream.FlushAsync();
            fileStream.Close();
            movie.Video = fileName;
            await _dbContext.SaveChangesAsync();
            return Ok(movie);
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetVideoById(int movieId)
        {
            var movie = await _dbContext.Movies.FirstOrDefaultAsync(p => p.Id == movieId);
            if (movie is null)
                return NotFound("ishkal: Video topilmadi");
            string path = Path.Combine(_env.WebRootPath, movie.Video);
            byte[] file = await System.IO.File.ReadAllBytesAsync(path);
            return File(file, "octet/stream", movie.Video);
        }

        [HttpPost]
        public async Task<IActionResult> SetMovie([FromBody] Movie movie)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is not valid");
            var entry = await _dbContext.AddAsync(movie);
            await _dbContext.SaveChangesAsync();
            return Ok(true);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            return Ok(await _dbContext.Movies
                    .Include(p => p.Author)
                    .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMoviesIsNotNull()
        {
            var movies = await _dbContext.Movies.Include(m => m.Author).Where(mov =>
                  !(string.IsNullOrEmpty(mov.Image) || string.IsNullOrEmpty(mov.Video)
                  || mov.Id == 0 || mov.Id == null)
            ).ToListAsync();
            return Ok(movies);
        }

        [HttpGet]
        public async Task<IActionResult> GetMovieById(int movieId)
        {
            var movie = await _dbContext.Movies
                .Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == movieId);
            if (movie is null)
                return NotFound();
            return Ok(movie);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMovieById(int movieId)
        {
            var movie = await _dbContext.Movies.FirstOrDefaultAsync(p => p.Id == movieId);
            if (movie is null)
                return NotFound("Bunday Movie mavjud emas!");
            _dbContext.Remove(movie);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuthors()
        {
            return Ok(await _dbContext.Authors.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthorById(int authorId)
        {
            var author = await _dbContext.Authors
                .FirstOrDefaultAsync(p => p.Id == authorId);
            if (author is null)
                return NotFound("Bunday Author yoq!");
            return Ok(author);
        }

        [HttpPost]
        public async Task<IActionResult> SetAuthor([FromBody] Author author)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is not valid");
            var entry = await _dbContext.AddAsync(author);
            await _dbContext.SaveChangesAsync();
            return Ok(entry.Entity);
        }

        [HttpDelete("{authorId}")]
        public async Task<IActionResult> DeleteAuthorById(int authorId)
        {
            var author = await _dbContext.Authors.FirstOrDefaultAsync(p => p.Id == authorId);
            if (author is null)
                return NotFound("Bunday Author mavjud emas!");
            var movies = await _dbContext.Movies
                .Where(x => x.AuthorId == authorId).ToListAsync();
            movies.ForEach(mov =>
                _dbContext.Movies.Remove(mov));
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAuthorById(int authorId, [FromBody] Author author)
        {
            var a = await _dbContext.Authors.FirstOrDefaultAsync(p => p.Id == authorId);
            if (a is null)
                return NotFound();
            a.LastName = author.LastName;
            a.FirstName = author.FirstName;
            await _dbContext.SaveChangesAsync();
            return Ok(a);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMovieById(int movieId, [FromBody] Movie movie)
        {
            var m = await _dbContext.Movies
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == movieId);
            if (m is null)
                return NotFound();
            m.Title = movie.Title;
            m.Discription = movie.Discription;
            m.AuthorId = movie.AuthorId;
            await _dbContext.SaveChangesAsync();
            return Ok(m);
        }
    }
}
