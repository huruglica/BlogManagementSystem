namespace ModelDto.CommentDto
{
    public class CommentViewDto
    {
        public string Username { get; set; }
        public string? Email { get; set; }
        public string Content { get; set; }
        public List<CommentViewDto> Reply { get; set; }
    }
}
