using DataStructures.Optimized;

namespace Demo;

internal static class Activity2
{
    public static void Run()
    {
        var queue = new ApiRequestQueue();
        queue.EnqueueBatch([new("/auth", 1), new("/data", 3), new("/healthcheck", 2), new("/auth-next", 1)]);
        var endpoints = new List<string>();
        while (queue.Dequeue() is { } request) endpoints.Add(request.Endpoint);
        Console.WriteLine("Activity 2: " + string.Join(" -> ", endpoints));
        if (!endpoints.SequenceEqual(new[] { "/auth", "/auth-next", "/healthcheck", "/data" }))
            throw new InvalidOperationException("Scheduling demonstration failed.");
    }
}
