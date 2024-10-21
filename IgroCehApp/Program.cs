using API.Configurations;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddOptionsConfiguration(configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(InfrastructureMapperProfile));
builder.Services.AddPersistance(configuration);
builder.Services.AddInfrastructure(configuration);
builder.Services.AddFastEndpoints()
    .AddSwaggerDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyHeader());
}

app.UseHttpsRedirection();
app.UseFastEndpoints()
    .UseSwaggerGen();

app.Run();
