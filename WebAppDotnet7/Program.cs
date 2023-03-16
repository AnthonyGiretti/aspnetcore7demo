using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using WebAppDotnet7;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using WebAppDotnet7.MinimalEndpoints;
using WebAppDotnet7.Groups;

var builder = WebApplication.CreateBuilder(args);
var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddSeq();
}).CreateLogger("Program");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.DisableImplicitFromServicesParameters = true;
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "LimitPolicy",
        options =>
        {
            options.PermitLimit = 1;
            options.Window = TimeSpan.FromSeconds(10);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 1;
        });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
    };
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseRateLimiter();

// Demo swagger and demo nameof extended scope
app.MapGet("/Country/{country}/Language/{language}", [Log("country", nameof(country), nameof(language))] (string country, string language) => 
    $"I Live in {country} speak {language}!").WithOpenApi(op =>
{
    op.Parameters[0].Description = "Name of the country";
    op.Parameters[1].Description = "Name of the language";
    return op;
}).AddEndpointFilter(async (efiContext, next) => // Demo Filters
{
    app.Logger.LogDebug("Before filter");
    var result = await next(efiContext);
    app.Logger.LogDebug("After filter");
    return result;
});

// Demo rate limiter
app.MapGet("/Testlimit", [EnableRateLimiting("LimitPolicy")] () => "I'm reachable!");

// Demo upload file
app.MapPost("/upload", async (IFormFile file) =>
{
    //Do something with the file
    using var stream = File.OpenWrite(file.FileName);
    await file.CopyToAsync(stream);

    return Results.Ok(file.FileName);
});

// Ids?id=1&id=2&id=3
app.MapGet("/Ids",(int[] id) => $"Ids are {string.Join(",", id)}");

// Languages?lan=fr&lan=en
app.MapGet("/Languages", (StringValues lan) => $"Languages are {string.Join(",", lan.ToArray())}");

// From headers
app.MapGet("/IdsFromHeader", ([FromHeader(Name = "id")] int[] id) => $"Ids are {string.Join(",", id.ToArray())}");

// Demo upload & new Results.Stream overload
app.MapPost("/turnToJpeg", (IFormFile file) =>
{
    return Results.Stream(async stream =>
    {
        using var image = await Image.LoadAsync(file.OpenReadStream());
        await image.SaveAsync(stream, JpegFormat.Instance);
    }, "image/jpeg");
});

// Demo unit test
app.MapGet("/UnitTestable", (IHttpContextAccessor httpContextAccessor) => UnitTestableEndpoints.GetPerson);

// Demo group endpoints
app.MapGroup("/v1")
    .GroupOne()
    .RequireCors("CorsPolicy1")
    .AllowAnonymous()
    .WithTags("v1");

app.MapGroup("/v2")
    .GroupTwo()
    .RequireCors("CorsPolicy2")
    .RequireAuthorization()
    .RequireRateLimiting("LimitPolicy")
    .WithTags("v2");

// Demo group endpoints parameters
var baseUri = app.MapGroup("group");
var country = baseUri.MapGroup("Country/{country}");
var language = country.MapGroup("Language/{language}");
language.MapGet("", (string country, string language) => $"I Live in {country} speak {language}!");
country.MapGet("Timezone/{timezone}", (string country, string timezone) => $"I Live in {country} the timezone is {timezone}!");

app.MapGet("/logging", () =>
{
    using (logger.BeginScope("Start processing for Id: {id}", 10))
    {
        using (logger.BeginScope("Beginning action 1"))
        {
            logger.LogInformation("Action 1 done");
            logger.LogInformation("SubAction 1 done, data retrieved {data}", new { Result = "Ok" });
        }

        using (logger.BeginScope("Beginning action 2"))
        {
            logger.LogInformation("Action 2 done, with status: {Status}", new { Status = "Processed", Duration = TimeSpan.FromSeconds(10) });
        }
    }

    return Results.Ok();
});

app.Run();