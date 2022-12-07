using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;

var channel = GrpcChannel.ForAddress("https://localhost:7092");
var client = new Health.HealthClient(channel);

var response = await client.CheckAsync(new HealthCheckRequest());

Console.WriteLine($"Check unary {response.Status}");

using var streaming = client.Watch(new HealthCheckRequest());
await foreach (var streamedResponse in streaming.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"Watch streamed {response.Status}"); // When status changes only
}

Console.ReadLine();
