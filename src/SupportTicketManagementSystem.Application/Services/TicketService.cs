using AutoMapper;
using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Extensions;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Exceptions;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Application.Interfaces.Services;
using SupportTicketManagementSystem.Domain.Entities;

namespace SupportTicketManagementSystem.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTicketDto> _createValidator;
    private readonly IValidator<UpdateTicketRequest> _updateRequestValidator;
    private readonly IValidator<TicketQueryDto> _queryValidator;

    public TicketService(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateTicketDto> createValidator,
        IValidator<UpdateTicketRequest> updateRequestValidator,
        IValidator<TicketQueryDto> queryValidator)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateRequestValidator = updateRequestValidator;
        _queryValidator = queryValidator;
    }

    public async Task<PagedResult<TicketDto>> SearchAsync(
        TicketQueryDto query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await _queryValidator.ValidateDtoAsync(query, cancellationToken);

        var (tickets, totalCount) = await _ticketRepository.SearchAsync(query, cancellationToken);
        var ticketDtos = _mapper.Map<IReadOnlyList<TicketDto>>(tickets);

        return PagedResult<TicketDto>.Create(
            ticketDtos,
            totalCount,
            query.PageNumber,
            query.PageSize);
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
        await _ticketRepository.LoadUsersAsync(ticket, cancellationToken);

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto> UpdateAsync(int id, UpdateTicketDto dto, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), id);

        var request = new UpdateTicketRequest
        {
            TicketId = id,
            Dto = dto,
            CurrentStatus = ticket.Status
        };

        await _updateRequestValidator.ValidateDtoAsync(request, cancellationToken);

        _mapper.Map(dto, ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _ticketRepository.LoadUsersAsync(ticket, cancellationToken);

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), id);

        await _ticketRepository.DeleteAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
