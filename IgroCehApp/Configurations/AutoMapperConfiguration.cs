using Application.DTO;
using AutoMapper;
using Infrastructure.DTO;

namespace API.Configurations
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<AuthorizationResult, GetAuthorizationTokenResult>()
                .ForMember(dest => dest.access_token, opt => opt.MapFrom(src => src.accessToken))
                .ForMember(dest => dest.refresh_token, opt => opt.MapFrom(src => src.refreshToken))
                .ForMember(dest => dest.expires_in, opt => opt.MapFrom(src => src.expiresIn))
                .ForMember(dest => dest.token_type, opt => opt.MapFrom(src => src.tokenType))
                .ReverseMap();
        }
    }
}
