using AutoMapper;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Mappings;

public class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => FormatUserName(src.CreatedBy)))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src =>
                src.AssignedTo != null ? FormatUserName(src.AssignedTo) : null));

        CreateMap<CreateTicketDto, Ticket>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TicketStatus.Open));

        CreateMap<UpdateTicketDto, Ticket>()
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore());
    }

    private static string FormatUserName(User user) =>
        $"{user.FirstName} {user.LastName}".Trim();
}
