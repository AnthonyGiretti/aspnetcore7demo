using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using WebAppDotnet7;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Formats.Jpeg;
using WebAppDotnet7.MinimalEndpoints;
using WebAppDotnet7.Groups;
using WebAppDotnet7.EndpointFilters;

var builder = WebApplication.CreateBuilder(args);

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
            //options.QueueLimit = 1; //--> bug here, not working
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
    var logger = efiContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
    logger.LogDebug("Before filter");
    var result = await next(efiContext);
    logger.LogDebug("After filter");
    return result;
}).AddEndpointFilter<MyEndpointFilter>(); // Demo custom filters

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

app.Run();