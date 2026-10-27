using AuthSession.Handlers;
using AuthSession.Requirements;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddSingleton<IAuthorizationHandler, UserNameAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AllowedIdAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ForbiddenAuthorizationResultHandler>();

var authenticated = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", authenticated);
    options.FallbackPolicy = authenticated;
    
    options.AddPolicy("WeatherReader", policy =>
    {
        policy.AddRequirements(new UserNameRequirement(@"MASTER\TDPJDR"));
    });

    options.AddPolicy("WeatherAdmin", policy =>
    {
        policy.AddRequirements(new UserNameRequirement(@"MASTER\TDPJDR_LA"));
        policy.AddRequirements(new AllowedIdRequirement(1001, 1002, 2001));
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        options.WithJavaScriptConfiguration("/config.js");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (HttpContext httpContext) =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .RequireAuthorization("WeatherReader")
    .WithName("GetWeatherForecast");

app.MapGet("/admin/{id:int}", (int id) =>
        Results.Ok($"You are authorized as the demo admin for ID {id}."))
    .RequireAuthorization("WeatherAdmin");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .AllowAnonymous();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}