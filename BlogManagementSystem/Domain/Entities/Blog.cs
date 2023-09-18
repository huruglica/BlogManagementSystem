using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Blog
    {
        public int Id { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(20)]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublicationDate { get; set;}
        public byte[] Image { get; set;}
        public string UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public virtual ICollection<Likes> Likes { get; set; } = new List<Likes>();
    }
}
