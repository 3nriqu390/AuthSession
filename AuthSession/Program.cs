using AuthSession.Handlers;
using AuthSession.Middleware;
using AuthSession.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddSingleton<IAuthorizationHandler, AgeAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AllowedIdAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ForbiddenAuthorizationResultHandler>();

var adminPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .RequireRole("admin", "reader")
    .Build();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = adminPolicy;
    
    options.AddPolicy("WeatherReader", policy =>
    {
        policy.RequireRole("reader");
    });

    options.AddPolicy("WeatherAdmin", policy =>
    {
        policy.RequireRole("admin");
        policy.AddRequirements(new AllowedIdRequirement(1001, 1002, 2001));
    });
    
    options.AddPolicy("AgeRequirement", policy =>
    {
        policy.AddRequirements(new AgeRequirement());
    });

});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        options.WithJavaScriptConfiguration("/config.js");
    }).AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();

app.Use(async (context, next) =>
{
    await next(context);
}); 

app.UseMiddleware<AuthorizationAuditMiddleware>();
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
    .RequireAuthorization("AgeRequirement")
    .WithName("GetWeatherForecast");

app.MapGet("/adminorReader", () =>
    Results.Ok("You are authorized as the demo admin or reader."))
    .RequireAuthorization(adminPolicy);

app.MapGet("/admin/{id:int}", (int id) =>
        Results.Ok($"You are authorized as the demo admin for ID {id}."))
    .RequireAuthorization("WeatherAdmin");

app.MapGet("/otherEndpoint", () =>
    Results.Ok($"You are authorized to access the other endpoint."));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .AllowAnonymous();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}