using ChatApp.Api.Hubs;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Config
var config = builder.Configuration;
var jwtKey = config["Jwt:Key"];
var jwtIssuer = config["Jwt:Issuer"];
var jwtAudience = config["Jwt:Audience"];
var mongoConn = config["MongoSettings:ConnectionString"];

// Services
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",                       // local dev
                "https://chatapp-frontend.vercel.app",         // your prod frontend
                "https://chatapp-jzrd.onrender.com")           // your backend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // must be present when using SignalR + JWT
    });
});
builder.Services.AddSignalR();

builder.Services.AddSingleton(new MongoService(mongoConn, config["MongoSettings:Database"]!));
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };

        // SignalR WebSocket authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chat").RequireAuthorization();

app.Run();
