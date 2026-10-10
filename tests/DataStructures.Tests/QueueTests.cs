using System.Collections.Concurrent;
using DataStructures.Optimized;

namespace DataStructures.Tests;

public sealed class QueueTests
{
    [Fact]
    public void OrdersPrioritiesAndPreservesFifoTies()
    {
        var queue = new ApiRequestQueue();
        queue.Enqueue(new("/first", 1));
        queue.EnqueueBatch([new("/max", int.MaxValue), new("/second", 1), new("/min", int.MinValue)]);
        queue.Enqueue(new("/third", 1));
        Assert.Equal(new[] { "/min", "/first", "/second", "/third", "/max" },
            Drain(queue).Select(request => request.Endpoint));
        Assert.Null(queue.Dequeue());
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void BatchAndIndividualInsertionAreEquivalent()
    {
        var random = new Random(42);
        var requests = Enumerable.Range(0, 2000).Select(i => new ApiRequest($"/{i}", random.Next(-20, 20))).ToArray();
        var individual = new ApiRequestQueue();
        var batch = new ApiRequestQueue();
        foreach (var request in requests.Take(37))
        {
            individual.Enqueue(request);
            batch.Enqueue(request);
        }
        foreach (var request in requests.Skip(37)) individual.Enqueue(request);
        batch.EnqueueBatch(requests.Skip(37));
        Assert.Equal(requests.OrderBy(request => request.Priority), Drain(batch));
        Assert.Equal(requests.OrderBy(request => request.Priority), Drain(individual));
    }

    [Fact]
    public void InvalidBatchDoesNotMutateQueue()
    {
        var queue = new ApiRequestQueue();
        var original = new ApiRequest("/original", 1);
        queue.Enqueue(original);
        Assert.Throws<ArgumentNullException>(() => queue.Enqueue(null!));
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueBatch(null!));
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueBatch([new("/valid", 0), null!]));
        Assert.Throws<ArgumentException>(() => queue.EnqueueBatch(ThrowDuringEnumeration()));
        queue.EnqueueBatch([]);
        Assert.Equal(1, queue.Count);
        Assert.Same(original, queue.Dequeue());
        Assert.Throws<ArgumentNullException>(() => new ApiRequest(null!, 1));
        Assert.Throws<ArgumentException>(() => new ApiRequest(" ", 1));
    }

    private static IEnumerable<ApiRequest> ThrowDuringEnumeration()
    {
        yield return new ApiRequest("/valid", 0);
        throw new ArgumentException("Malformed batch source.");
    }

    [Fact]
    public async Task ConcurrentProducersAndConsumersLoseNoRequests()
    {
        var queue = new ApiRequestQueue();
        var seen = new ConcurrentBag<string>();
        int finished = 0;
        Task[] producers = Enumerable.Range(0, 4).Select(producer => Task.Run(() =>
        {
            for (int i = 0; i < 500; i += 10)
            {
                if (i % 20 == 0)
                    queue.EnqueueBatch(Enumerable.Range(i, 10).Select(j => new ApiRequest($"/{producer}/{j}", j % 7)));
                else
                    for (int j = i; j < i + 10; j++) queue.Enqueue(new ApiRequest($"/{producer}/{j}", j % 7));
            }
            Interlocked.Increment(ref finished);
        })).ToArray();
        Task[] consumers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            while (Volatile.Read(ref finished) < 4 || queue.Count > 0)
            {
                if (queue.Dequeue() is { } request) seen.Add(request.Endpoint);
                else Thread.Yield();
            }
        })).ToArray();
        await Task.WhenAll(producers.Concat(consumers)).WaitAsync(TimeSpan.FromSeconds(20));
        Assert.Equal(2000, seen.Count);
        Assert.Equal(2000, seen.Distinct().Count());
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void BaselineRetainsPriorityOrderingAndEmptyResult()
    {
        var queue = new DataStructures.Baseline.ApiRequestQueue();
        Assert.Null(queue.Dequeue());
        queue.Enqueue(new("/data", 3));
        queue.Enqueue(new("/auth", 1));
        Assert.Equal("/auth", queue.Dequeue()?.Endpoint);
        Assert.Equal("/data", queue.Dequeue()?.Endpoint);
        Assert.Null(queue.Dequeue());
    }

    private static List<ApiRequest> Drain(ApiRequestQueue queue)
    {
        var result = new List<ApiRequest>();
        while (queue.Dequeue() is { } request) result.Add(request);
        return result;
    }
}
