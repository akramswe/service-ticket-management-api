using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ServiceTicketManagement.Api.Data;
using ServiceTicketManagement.Api.Infrastructure;
using ServiceTicketManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "Service Ticket Management API",
    version = "1.0.0",
    endpoints = "/api/tickets",
    openApi = "/openapi/v1.json"
})).ExcludeFromDescription();

await DbInitializer.InitializeAsync(app.Services);

app.Run();

public partial class Program { }
