using AutoMapper;
using NetTopologySuite.Geometries;
using PointAtlas.Application.DTOs;
using PointAtlas.Core.Entities;

namespace PointAtlas.Application.Mappings;

public class MarkerProfile : Profile
{
    public MarkerProfile()
    {
        CreateMap<Marker, MarkerDto>()
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Y))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.X))
            .ForMember(dest => dest.CreatedByDisplayName, opt => opt.MapFrom(src => src.CreatedBy.DisplayName));

        CreateMap<CreateMarkerRequest, Marker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore()) // Set manually in service
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateMarkerRequest, Marker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore()) // Set manually in service
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
