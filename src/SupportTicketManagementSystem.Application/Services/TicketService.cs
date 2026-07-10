using AutoMapper;
using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Extensions;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Exceptions;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Application.Interfaces.Services;
using SupportTicketManagementSystem.Domain.Entities;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTicketDto> _createValidator;
    private readonly IValidator<UpdateTicketDto> _updateValidator;

    public TicketService(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateTicketDto> createValidator,
        IValidator<UpdateTicketDto> updateValidator)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<TicketDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetAllWithUsersAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TicketDto>>(tickets);
    }

    public async Task<TicketDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdWithUsersAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), id);

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto> CreateAsync(CreateTicketDto dto, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateDtoAsync(dto, cancellationToken);

        var ticket = _mapper.Map<Ticket>(dto);
        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdTicket = await _ticketRepository.GetByIdWithUsersAsync(ticket.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), ticket.Id);

        return _mapper.Map<TicketDto>(createdTicket);
    }

    public async Task<TicketDto> UpdateAsync(int id, UpdateTicketDto dto, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateDtoAsync(dto, cancellationToken);

        var ticket = await _ticketRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), id);

        EnsureValidStatusTransition(ticket.Status, dto.Status);

        _mapper.Map(dto, ticket);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTicket = await _ticketRepository.GetByIdWithUsersAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), id);

        return _mapper.Map<TicketDto>(updatedTicket);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), id);

        await _ticketRepository.DeleteAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureValidStatusTransition(TicketStatus currentStatus, TicketStatus newStatus)
    {
        if (TicketStatusWorkflow.CanTransition(currentStatus, newStatus))
        {
            return;
        }

        var allowedTransitions = TicketStatusWorkflow.GetAllowedTransitions(currentStatus);
        var allowedMessage = allowedTransitions.Count == 0
            ? "none (terminal status)"
            : string.Join(", ", allowedTransitions);

        throw new Exceptions.ValidationException(
            "Status",
            $"Cannot transition from '{currentStatus}' to '{newStatus}'. Allowed transitions: {allowedMessage}.");
    }
}
