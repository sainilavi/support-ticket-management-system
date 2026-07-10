using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SupportTicketManagementSystem.API.Swagger.Filters;

public class SwaggerExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ApplyRequestExamples(operation, context);
        ApplyResponseExamples(operation, context);
    }

    private static void ApplyRequestExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.MethodInfo.DeclaringType?.Name;
        var action = context.MethodInfo.Name;
        var key = $"{controller}.{action}";

        var requestJson = key switch
        {
            "TicketsController.Create" => SwaggerExamples.ToJson(SwaggerExamples.SampleCreateTicketRequest),
            "TicketsController.Update" => SwaggerExamples.ToJson(SwaggerExamples.SampleUpdateTicketRequest),
            "CommentsController.Create" => SwaggerExamples.ToJson(SwaggerExamples.SampleCreateCommentRequest),
            _ => null
        };

        if (requestJson is null || operation.RequestBody?.Content is null)
        {
            return;
        }

        if (operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = CreateOpenApiExample(requestJson);
        }
    }

    private static void ApplyResponseExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.MethodInfo.DeclaringType?.Name;
        var action = context.MethodInfo.Name;
        var key = $"{controller}.{action}";

        foreach (var (statusCode, exampleJson) in GetResponseExamples(key))
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                continue;
            }

            response.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"] = new OpenApiMediaType();
            }

            response.Content["application/json"].Example = CreateOpenApiExample(exampleJson);
        }
    }

    private static IEnumerable<(string StatusCode, string ExampleJson)> GetResponseExamples(string key) =>
        key switch
        {
            "TicketsController.Search" =>
            [
                ("200", SwaggerExamples.PagedTicketsSuccessResponse()),
                ("400", SwaggerExamples.ToJson(SwaggerExamples.SampleValidationError))
            ],
            "TicketsController.GetById" =>
            [
                ("200", SwaggerExamples.TicketSuccessResponse(SwaggerExamples.SampleTicket)),
                ("404", SwaggerExamples.ToJson(SwaggerExamples.SampleNotFoundError))
            ],
            "TicketsController.Create" =>
            [
                ("201", SwaggerExamples.TicketSuccessResponse(SwaggerExamples.SampleTicket, "Ticket created successfully.")),
                ("400", SwaggerExamples.ToJson(SwaggerExamples.SampleValidationError))
            ],
            "TicketsController.Update" =>
            [
                ("200", SwaggerExamples.TicketSuccessResponse(SwaggerExamples.SampleTicket, "Ticket updated successfully.")),
                ("400", SwaggerExamples.ToJson(SwaggerExamples.SampleValidationError)),
                ("404", SwaggerExamples.ToJson(SwaggerExamples.SampleNotFoundError))
            ],
            "TicketsController.Delete" =>
            [
                ("200", SwaggerExamples.DeleteSuccessResponse()),
                ("404", SwaggerExamples.ToJson(SwaggerExamples.SampleNotFoundError))
            ],
            "CommentsController.GetByTicketId" =>
            [
                ("200", SwaggerExamples.CommentsListSuccessResponse()),
                ("400", SwaggerExamples.ToJson(SwaggerExamples.SampleValidationError))
            ],
            "CommentsController.Create" =>
            [
                ("201", SwaggerExamples.CommentSuccessResponse(SwaggerExamples.SampleComment, "Comment added successfully.")),
                ("400", SwaggerExamples.ToJson(SwaggerExamples.SampleValidationError))
            ],
            _ => []
        };

    private static IOpenApiAny CreateOpenApiExample(string json) =>
        OpenApiAnyFactory.CreateFromJson(json);
}
