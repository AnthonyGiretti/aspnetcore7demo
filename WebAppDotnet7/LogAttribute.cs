namespace WebAppDotnet7
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class LogAttribute : Attribute
    {
        public LogAttribute(string endpointName, params string[] parameters)
        {
            Console.WriteLine();
            Console.WriteLine($"Endpoint {endpointName} takes the following parameters: {string.Join(", ", parameters)}");
        }
    }
}
