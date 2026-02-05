using AutoMapper;
using PointAtlas.Application.DTOs;
using PointAtlas.Core.Entities;

namespace PointAtlas.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ConstructUsing((src, ctx) => new UserDto(
                src.Id,
                src.Email ?? string.Empty,
                src.DisplayName,
                new List<string>() // Roles will be set by the service
            ));
    }
}
