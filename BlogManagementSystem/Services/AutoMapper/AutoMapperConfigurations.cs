using AutoMapper;
using Domain.Entities;
using ModelDto.BlogDto;
using ModelDto.CommentDto;
using ModelDto.LikesDto;
using ModelDto.TagDto;
using ModelDto.UserDto;

namespace Services.AutoMapper
{
    public class AutoMapperConfigurations : Profile
    {
        public AutoMapperConfigurations()
        {
            CreateMap<User, UserCreateDto>().ReverseMap();
            CreateMap<User, UserUpdateDto>().ReverseMap();
            CreateMap<User, UserViewDto>().ReverseMap();
            CreateMap<User, UserTagDto>().ReverseMap();

            CreateMap<Comment, CommentCreateDto>().ReverseMap();
            CreateMap<Comment, CommentUpdateDto>().ReverseMap();
            CreateMap<Comment, CommentViewDto>().ReverseMap();

            CreateMap<Tag, TagViewDto>().ReverseMap()
                .ForMember(x => x.User, y => y.MapFrom(z => z.User));

            CreateMap<Likes, LikesViewDto>().ReverseMap()
                .ForMember(x => x.User, y => y.MapFrom(z => z.User));

            CreateMap<Blog, BlogCreateDto>().ReverseMap();
            CreateMap<Blog, BlogUpdateDto>().ReverseMap();
            CreateMap<Blog, BlogViewDto>().ReverseMap()
                .ForMember(x => x.Tags, y => y.MapFrom(z => z.Tags))
                .ForMember(x => x.Likes, y => y.MapFrom(z => z.Likes));
        }
    }
}
