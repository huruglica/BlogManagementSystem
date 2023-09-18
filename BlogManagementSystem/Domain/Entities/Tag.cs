namespace Domain.Entities
{
    public class Tag
    {
        public int BlogId { get; set; }
        public string UserId { get; set; }

        public virtual Blog Blog { get; set; }
        public virtual User User { get; set; }
    }
}
