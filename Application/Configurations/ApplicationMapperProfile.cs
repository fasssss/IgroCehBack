using Application.DTO;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Configurations
{
    public class ApplicationMapperProfile: Profile
    {
        public ApplicationMapperProfile() 
        {
            CreateMap<Event, EventObject>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.StatusId))
                .ForMember(dest => dest.EventRecordObjects, opt => opt.MapFrom(src => src.EventRecords))
                .ForMember(dest => dest.EventCreatorId, opt => opt.MapFrom(src => src.CreatorId))
                .ReverseMap();

            CreateMap<EventRecord, EventRecordObject>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Participant, opt => opt.MapFrom(src => src.Participant))
                .ForMember(dest => dest.ToUser, opt => opt.MapFrom(src => src.ToUser))
                .ReverseMap();

            CreateMap<User, UserObject>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ReverseMap();
            
            CreateMap<Game, GameObject>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SteamUrl, opt => opt.MapFrom(src => src.SteamUrl))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ReverseMap();
        }
    }
}
