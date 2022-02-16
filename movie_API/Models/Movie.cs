using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace movie_API.Models
{
    public class Movie
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required,MaxLength(50)]
        public string Title { get; set; }

        [Required,MaxLength(200)]
        public string Discription { get; set; }

        [MaxLength(200)]
        public string Image { get; set; }

        [MaxLength(200)]
        public string Video { get; set; }

        [Required,ForeignKey(nameof(Author))]
        public int AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; }
    }
}
