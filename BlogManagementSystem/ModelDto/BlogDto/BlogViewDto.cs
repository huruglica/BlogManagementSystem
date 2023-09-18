using ModelDto.CommentDto;
using ModelDto.LikesDto;
using ModelDto.SearchAndPagination;
using ModelDto.TagDto;
using ModelDto.UserDto;

namespace ModelDto.BlogDto
{
    public class BlogViewDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublicationDate { get; set; }
        public byte[] Images { get; set; }
        public PagedInfo<CommentViewDto>? CommentsPaged { get; set; }

        public virtual UserViewDto User { get; set; }
        public virtual ICollection<TagViewDto> Tags { get; set; } = new List<TagViewDto>();
        public virtual ICollection<LikesViewDto> Likes { get; set; } = new List<LikesViewDto>();
    }
}
