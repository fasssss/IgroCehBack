using Application.DTO;
using AutoMapper;
using Infrastructure.DTO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class InfrastructureMapperProfile: Profile
    {
        public InfrastructureMapperProfile()
        {
            CreateMap<AuthorizationTokens, GetAuthorizationTokenResult>()
                .ForMember(dest => dest.access_token, opt => opt.MapFrom(src => src.AccessToken))
                .ForMember(dest => dest.refresh_token, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.expires_in, opt => opt.MapFrom(src => src.ExpiresIn))
                .ForMember(dest => dest.token_type, opt => opt.MapFrom(src => src.TokenType))
                .ReverseMap();

            CreateMap<UserObject, DiscordUserObject>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.discriminator, opt => opt.MapFrom(src => src.Discriminator))
                .ForMember(dest => dest.global_name, opt => opt.MapFrom(src => src.GlobalName))
                .ForMember(dest => dest.avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.bot, opt => opt.MapFrom(src => src.Bot))
                .ForMember(dest => dest.system, opt => opt.MapFrom(src => src.System))
                .ForMember(dest => dest.mfa_enabled, opt => opt.MapFrom(src => src.MfaEnabled))
                .ForMember(dest => dest.banner, opt => opt.MapFrom(src => src.Banner))
                .ForMember(dest => dest.accent_color, opt => opt.MapFrom(src => src.AccentColor))
                .ForMember(dest => dest.locale, opt => opt.MapFrom(src => src.Locale))
                .ForMember(dest => dest.verified, opt => opt.MapFrom(src => src.Verified))
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.verified, opt => opt.MapFrom(src => src.Verified))
                .ForMember(dest => dest.flags, opt => opt.MapFrom(src => src.Flags))
                .ForMember(dest => dest.premium_type, opt => opt.MapFrom(src => src.PremiumType))
                .ForMember(dest => dest.public_flags, opt => opt.MapFrom(src => src.PublicFlags))
                .ReverseMap()
                .ForPath(s => s.Id, opt => opt.MapFrom(src => long.Parse(src.id)));

            CreateMap<GuildObject, DiscordGuildObject>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.icon, opt => opt.MapFrom(src => src.Icon))
                .ForMember(dest => dest.banner, opt => opt.MapFrom(src => src.Banner))
                .ForMember(dest => dest.owner, opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.owner_id, opt => opt.MapFrom(src => src.OwnerId))
                .ForMember(dest => dest.permissions, opt => opt.MapFrom(src => src.Permissions))
                .ForMember(dest => dest.features, opt => opt.MapFrom(src => src.Features))
                .ForMember(dest => dest.approximate_member_count, opt => opt.MapFrom(src => src.ApproximateMemberCount))
                .ForMember(dest => dest.approximate_presence_count, opt => opt.MapFrom(src => src.ApproximatePresenceCount))
                .ReverseMap()
                .ForPath(s => s.Id, opt => opt.MapFrom(src => long.Parse(src.id)));
        }
    }
}
