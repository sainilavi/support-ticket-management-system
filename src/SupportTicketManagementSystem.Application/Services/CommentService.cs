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
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCommentDto> _createValidator;

    public CommentService(
        ICommentRepository commentRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCommentDto> createValidator)
    {
        _commentRepository = commentRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<IReadOnlyList<CommentDto>> GetByTicketIdAsync(
        int ticketId,
        CancellationToken cancellationToken = default)
    {
        await EnsureTicketExistsAsync(ticketId, cancellationToken);

        var comments = await _commentRepository.GetByTicketIdWithUserAsync(ticketId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CommentDto>>(comments);
    }

    public async Task<CommentDto> CreateAsync(
        int ticketId,
        CreateCommentDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureTicketExistsAsync(ticketId, cancellationToken);
        await _createValidator.ValidateDtoAsync(dto, cancellationToken);

        var comment = _mapper.Map<Comment>(dto);
        comment.TicketId = ticketId;

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdComment = await _commentRepository.GetByIdWithUserAsync(comment.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Comment), comment.Id);

        return _mapper.Map<CommentDto>(createdComment);
    }

    private async Task EnsureTicketExistsAsync(int ticketId, CancellationToken cancellationToken)
    {
        if (!await _ticketRepository.ExistsAsync(ticketId, cancellationToken))
        {
            throw new NotFoundException(nameof(Ticket), ticketId);
        }
    }
}
