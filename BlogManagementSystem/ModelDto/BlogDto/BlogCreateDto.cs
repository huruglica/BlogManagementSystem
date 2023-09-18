using Microsoft.AspNetCore.Http;

namespace ModelDto.BlogDto
{
    public class BlogCreateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile FormFile { get; set; }
    }
}
