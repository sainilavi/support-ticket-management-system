using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SupportTicketManagementSystem.API.Extensions;
using SupportTicketManagementSystem.Application.DependencyInjection;
using SupportTicketManagementSystem.Infrastructure.Data;
using SupportTicketManagementSystem.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Connection string is read from appsettings.json / appsettings.Development.json
// via builder.Configuration and registered in AddInfrastructure().
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Support Ticket Management System API",
        Version = "v1",
        Description = "ASP.NET Core 8 Web API for managing support tickets."
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.Migrate();
    }
}

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support Ticket Management System API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
