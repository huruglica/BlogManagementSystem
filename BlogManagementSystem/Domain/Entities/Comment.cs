using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(20)]
        public string Username { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(60)]
        public string? Email { get; set; }
        public string Content { get; set; }
        public int? BlogId { get; set; }

        public int? CommentId { get; set; }
        public ICollection<Comment> Reply { get; set; } = new List<Comment>();
    }
}
