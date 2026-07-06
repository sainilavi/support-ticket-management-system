using AutoMapper;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Mappings;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => FormatUserName(src.User)));

        CreateMap<CreateCommentDto, Comment>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }

    private static string FormatUserName(User user) =>
        $"{user.FirstName} {user.LastName}".Trim();
}
