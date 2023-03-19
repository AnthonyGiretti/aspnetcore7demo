namespace WebAppDotnet7.EndpointFilters;

public class MyEndpointFilter : IEndpointFilter
{
    private readonly ILogger _logger;

    public MyEndpointFilter(ILogger<MyEndpointFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var firstname = context.HttpContext.GetRouteData().Values["firstname"];
        _logger.LogInformation($"AddEndpointFilter before filter using parameter firstname: {firstname}");
        var result = await next(context);
        _logger.LogInformation($"AddEndpointFilter after filter using parameter firstname: {firstname}");
        return result;
    }
}