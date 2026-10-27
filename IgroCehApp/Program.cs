using API.Authentication;
using API.Authorization;
using API.Configurations;
using API.Helpers;
using Application.Configurations;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Authorization;
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
// protects the state of the sign-in through Steam, see SteamLinkState
builder.Services.AddDataProtection();
builder.Services.AddSingleton<SteamLinkState>();
builder.Services
    .AddAuthentication(o =>
        o.AddScheme<CustomAuthenticationHandler>("customAuthenticationScheme", null));
builder.Services.AddAuthorization(o =>
    o.AddPolicy(AppAdminPolicy.Name, policy => policy.AddRequirements(new AppAdminRequirement())));
builder.Services.AddScoped<IAuthorizationHandler, AppAdminHandler>();
builder.Services
    .AddFastEndpoints()
    .AddSwaggerDocument();

var app = builder.Build();

await app.ApplyMigrationsAsync();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(x => x
        .WithOrigins(configuration.GetValue<string>("FrontendUrl")!)
        .AllowAnyHeader()
        .AllowCredentials());
}
else
{
    app.UseCors(x => x
        .WithOrigins(configuration.GetValue<string>("FrontendUrl")!)
        .AllowAnyHeader()
        .AllowCredentials());
}

//app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Upload")),
    RequestPath = "/Upload",
    
});

// websockets middleware has to be registered before endpoints that accept websocket requests
app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(5)
    })
    .UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints()
    .UseSwaggerGen();

app.Run();
