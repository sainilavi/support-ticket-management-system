using System.Net;
using SupportTicketManagementSystem.API.Models;
using SupportTicketManagementSystem.Application.Common.Models;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.IntegrationTests;

public class TicketsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

  public TicketsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTicket_ReturnsCreatedTicket()
    {
        var request = new CreateTicketDto
        {
            Title = "Cannot login to portal",
            Description = "User receives a 401 error when signing in.",
            Priority = TicketPriority.High,
            CreatedByUserId = 3,
            AssignedToUserId = 2
        };

        var response = await _client.PostJsonAsync("/api/tickets", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.ReadAsJsonAsync<ApiResponse<TicketDto>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.Id > 0);
        Assert.Equal(request.Title, body.Data.Title);
        Assert.Equal(request.Description, body.Data.Description);
        Assert.Equal(TicketStatus.Open, body.Data.Status);
        Assert.Equal(request.Priority, body.Data.Priority);
        Assert.Equal(3, body.Data.CreatedByUserId);
        Assert.Equal(2, body.Data.AssignedToUserId);
    }

    [Fact]
    public async Task UpdateTicket_ReturnsUpdatedTicket()
    {
        var created = await CreateTicketAsync("Printer not working", "Office printer is offline.");

        var updateRequest = new UpdateTicketDto
        {
            Title = "Printer not working - escalated",
            Description = "Replaced toner and restarted the device.",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Medium,
            AssignedToUserId = 2
        };

        var response = await _client.PutJsonAsync($"/api/tickets/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.ReadAsJsonAsync<ApiResponse<TicketDto>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Equal(updateRequest.Title, body.Data.Title);
        Assert.Equal(updateRequest.Description, body.Data.Description);
        Assert.Equal(TicketStatus.InProgress, body.Data.Status);
        Assert.Equal(TicketPriority.Medium, body.Data.Priority);
    }

    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Open, TicketStatus.Cancelled)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Cancelled)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed)]
    public async Task UpdateTicket_AllowsValidStatusTransitions(TicketStatus fromStatus, TicketStatus toStatus)
    {
        var ticket = await CreateTicketAsync($"Transition {fromStatus}", "Status transition test ticket.");
        ticket = await SetTicketStatusAsync(ticket, fromStatus);

        var response = await _client.PutJsonAsync($"/api/tickets/{ticket.Id}", BuildUpdateDto(ticket, toStatus));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.ReadAsJsonAsync<ApiResponse<TicketDto>>();
        Assert.NotNull(body?.Data);
        Assert.Equal(toStatus, body.Data.Status);
    }

    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Open, TicketStatus.Closed)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Open)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Cancelled)]
    [InlineData(TicketStatus.Closed, TicketStatus.Open)]
    public async Task UpdateTicket_RejectsInvalidStatusTransitions(TicketStatus fromStatus, TicketStatus toStatus)
    {
        var ticket = await CreateTicketAsync($"Invalid transition {fromStatus}", "Invalid status transition test.");
        ticket = await SetTicketStatusAsync(ticket, fromStatus);

        var response = await _client.PutJsonAsync($"/api/tickets/{ticket.Id}", BuildUpdateDto(ticket, toStatus));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.ReadAsJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal(400, body.StatusCode);
        Assert.NotNull(body.Errors);
        Assert.True(body.Errors.ContainsKey("status"));
    }

    [Fact]
    public async Task SearchTickets_FiltersByKeywordStatusAndPagination()
    {
        await CreateTicketAsync("Login failure on portal", "User cannot login with valid credentials.");
        await CreateTicketAsync("Another login issue", "Login page returns an error.");
        await CreateTicketAsync("Printer jam", "Paper jam on level 2 printer.");

        var keywordResponse = await _client.GetAsync("/api/tickets?keyword=login&pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, keywordResponse.StatusCode);

        var keywordBody = await keywordResponse.ReadAsJsonAsync<ApiResponse<PagedResult<TicketDto>>>();
        Assert.NotNull(keywordBody?.Data);
        Assert.True(keywordBody.Data.TotalCount >= 2);
        Assert.All(keywordBody.Data.Items, ticket =>
            Assert.Contains("login", $"{ticket.Title} {ticket.Description}", StringComparison.OrdinalIgnoreCase));

        var statusResponse = await _client.GetAsync("/api/tickets?status=Open&pageNumber=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var statusBody = await statusResponse.ReadAsJsonAsync<ApiResponse<PagedResult<TicketDto>>>();
        Assert.NotNull(statusBody?.Data);
        Assert.Equal(1, statusBody.Data.PageSize);
        Assert.True(statusBody.Data.TotalCount >= 3);
        Assert.Single(statusBody.Data.Items);
        Assert.Equal(TicketStatus.Open, statusBody.Data.Items[0].Status);
        Assert.True(statusBody.Data.HasNextPage);
    }

    private async Task<TicketDto> CreateTicketAsync(string title, string description)
    {
        var request = new CreateTicketDto
        {
            Title = title,
            Description = description,
            Priority = TicketPriority.Medium,
            CreatedByUserId = 3,
            AssignedToUserId = 2
        };

        var response = await _client.PostJsonAsync("/api/tickets", request);
        response.EnsureSuccessStatusCode();

        var body = await response.ReadAsJsonAsync<ApiResponse<TicketDto>>();
        return body!.Data!;
    }

    private async Task<TicketDto> SetTicketStatusAsync(TicketDto ticket, TicketStatus targetStatus)
    {
        var currentStatus = ticket.Status;

        if (currentStatus == targetStatus)
        {
            return ticket;
        }

        foreach (var step in BuildStatusPath(currentStatus, targetStatus))
        {
            var response = await _client.PutJsonAsync(
                $"/api/tickets/{ticket.Id}",
                BuildUpdateDto(ticket, step));

            response.EnsureSuccessStatusCode();

            var body = await response.ReadAsJsonAsync<ApiResponse<TicketDto>>();
            ticket = body!.Data!;
        }

        return ticket;
    }

    private static IEnumerable<TicketStatus> BuildStatusPath(TicketStatus from, TicketStatus to)
    {
        if (from == to)
        {
            yield break;
        }

        var paths = new Dictionary<TicketStatus, TicketStatus[]>
        {
            [TicketStatus.Open] = [TicketStatus.InProgress, TicketStatus.Cancelled],
            [TicketStatus.InProgress] = [TicketStatus.Resolved, TicketStatus.Cancelled],
            [TicketStatus.Resolved] = [TicketStatus.Closed]
        };

        var queue = new Queue<List<TicketStatus>>();
        queue.Enqueue([from]);

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var last = path[^1];

            if (last == to)
            {
                foreach (var step in path.Skip(1))
                {
                    yield return step;
                }

                yield break;
            }

            if (!paths.TryGetValue(last, out var nextStatuses))
            {
                continue;
            }

            foreach (var next in nextStatuses)
            {
                if (path.Contains(next))
                {
                    continue;
                }

                var newPath = new List<TicketStatus>(path) { next };
                queue.Enqueue(newPath);
            }
        }

        throw new InvalidOperationException($"Unable to build status path from {from} to {to}.");
    }

    private static UpdateTicketDto BuildUpdateDto(TicketDto ticket, TicketStatus status) => new()
    {
        Title = ticket.Title,
        Description = ticket.Description,
        Priority = ticket.Priority,
        AssignedToUserId = ticket.AssignedToUserId,
        Status = status
    };
}
