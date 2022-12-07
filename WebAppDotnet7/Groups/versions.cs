namespace WebAppDotnet7.Groups
{
    public static class versions
    {
        public static RouteGroupBuilder V1Api(this RouteGroupBuilder group)
        {
            group.MapGet("/", () => Results.Ok(new List<int> {1,2,3}));
            group.MapGet("/{id}", (int id) => Results.Ok(id));

            return group;
        }

        public static RouteGroupBuilder V2Api(this RouteGroupBuilder group)
        {
            group.MapGet("/", () => Results.Ok(new List<int> { 4, 5, 6 }));
            group.MapGet("/{id}", (int id) => Results.Ok(id + 1));

            return group;
        }
    }
}
