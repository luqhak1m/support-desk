
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api;
using SupportDesk.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Send enums as strings ("InProgress") rather than numbers, so the Angular client
// and Swagger both see meaningful values.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SupportDeskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupportDesk")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using(IServiceScope scope=app.Services.CreateScope())
{
    SupportDeskDbContext db=scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
    DbSeeder.Seed(db);
}

app.UseExceptionHandler();

app.UseCors("Angular");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
