using System.ComponentModel.DataAnnotations;

namespace AbsoluteCinema.API.Models
{
    public class MovieSubImg
    {
        public int Id { get; set; }

        public string Img { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
