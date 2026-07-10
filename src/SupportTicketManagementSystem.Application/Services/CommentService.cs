using AutoMapper;
using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Extensions;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Exceptions;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Application.Interfaces.Services;
using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCommentRequest> _createRequestValidator;
    private readonly IValidator<GetCommentsByTicketRequest> _getByTicketRequestValidator;

    public CommentService(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCommentRequest> createRequestValidator,
        IValidator<GetCommentsByTicketRequest> getByTicketRequestValidator)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createRequestValidator = createRequestValidator;
        _getByTicketRequestValidator = getByTicketRequestValidator;
    }

    public async Task<IReadOnlyList<CommentDto>> GetByTicketIdAsync(
        int ticketId,
        CancellationToken cancellationToken = default)
    {
        var request = new GetCommentsByTicketRequest { TicketId = ticketId };
        await _getByTicketRequestValidator.ValidateDtoAsync(request, cancellationToken);

        var comments = await _commentRepository.GetByTicketIdWithUserAsync(ticketId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CommentDto>>(comments);
    }

    public async Task<CommentDto> CreateAsync(
        int ticketId,
        CreateCommentDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateCommentRequest
        {
            TicketId = ticketId,
            Dto = dto
        };

        await _createRequestValidator.ValidateDtoAsync(request, cancellationToken);

        var comment = _mapper.Map<Comment>(dto);
        comment.TicketId = ticketId;

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdComment = await _commentRepository.GetByIdWithUserAsync(comment.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Comment), comment.Id);

        return _mapper.Map<CommentDto>(createdComment);
    }
}
