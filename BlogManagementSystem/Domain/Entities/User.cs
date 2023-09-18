using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class User
    {
        public string Id { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(15)]
        public string Name { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(15)]
        public string Surname { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(15)]
        public string Username { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(120)]
        public string? Email { get; set; }
        public byte[] Key { get; set; }
        public byte[] PasswordHash { get; set; }
        [Column(TypeName = "varchar")]
        [MaxLength(10)]
        public string Role { get; set; }
    }
}
