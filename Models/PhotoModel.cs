using System.ComponentModel.DataAnnotations;

namespace BamukoyaPhotoApp.Models
{
    public class PhotoModel
    {
        [Required]  
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
