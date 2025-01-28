using API.Authentication;
using API.Configurations;
using API.Helpers;
using Application.Configurations;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Configurations;
using Microsoft.Extensions.FileProviders;

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
builder.Services.AddAutoMapper(typeof(ApplicationMapperProfile));
builder.Services.AddHttpContextAccessor();
builder.Services.AddPersistance(configuration);
builder.Services.AddApplicationConfiguration();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddSingleton<WebSocketHelper>();
builder.Services
    .AddAuthentication(o =>
        o.AddScheme<CustomAuthenticationHandler>("customAuthenticationScheme", null));
builder.Services.AddAuthorization();
builder.Services
    .AddFastEndpoints()
    .AddSwaggerDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(x => x
        .WithOrigins(configuration.GetValue<string>("FrontendUrl") ?? "https://localhost:5173")
        .AllowAnyHeader()
        .AllowCredentials());
}

//app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Upload")),
    RequestPath = "/Upload",
    
});

app.UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints()
    .UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(15)
    })
    .UseSwaggerGen();

/*app.MapGet("/api/ws", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
    }

    return Results.NoContent();
});*/

app.Run();
