using WebAppDotnet7.Models;

namespace WebAppDotnet7.MinimalEndpoints
{
    public static class UnitTestableEndpoints
    {
        public static IResult GetPerson(IHttpContextAccessor httpContextAccessor)
        {
            return Results.Ok(new Person
            {
                FirstName = httpContextAccessor.HttpContext.Items["firstname"].ToString(),
                LastName = httpContextAccessor.HttpContext.Items["lastname"].ToString()
            });
        }
    }
}
