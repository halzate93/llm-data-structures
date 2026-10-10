using BaselineQueue = DataStructures.Baseline.ApiRequestQueue;
using BaselineRequest = DataStructures.Baseline.ApiRequest;
using HeapQueue = DataStructures.Optimized.ApiRequestQueue;
using HeapRequest = DataStructures.Optimized.ApiRequest;

namespace DataStructures.Benchmarks;

internal static class QueueBenchmarks
{
    public static void Run()
    {
        var measurement = new Measurement(2);
        foreach (int n in new[] { 256, 2048 })
        {
            var random = new Random(42);
            int[] priorities = Enumerable.Range(0, n).Select(_ => random.Next(100)).ToArray();
            BaselineRequest[] baseline = priorities.Select((priority, i) => new BaselineRequest($"/{i}", priority)).ToArray();
            HeapRequest[] optimized = priorities.Select((priority, i) => new HeapRequest($"/{i}", priority)).ToArray();
            int[] expected = priorities.Order().ToArray();
            measurement.Run("enqueue", "list-sort", n, n, () => new BaselineQueue(),
                queue => { foreach (var request in baseline) queue.Enqueue(request); },
                queue => Validate(expected, () => queue.Dequeue()?.Priority));
            measurement.Run("enqueue", "heap", n, n, () => new HeapQueue(),
                queue => { foreach (var request in optimized) queue.Enqueue(request); },
                queue => Validate(expected, () => queue.Dequeue()?.Priority));
            measurement.Run("enqueue", "heap-batch", n, n, () => new HeapQueue(),
                queue => queue.EnqueueBatch(optimized),
                queue => Validate(expected, () => queue.Dequeue()?.Priority));
            measurement.Run("drain", "list-sort", n, n,
                () => { var queue = new BaselineQueue(); foreach (var request in baseline) queue.Enqueue(request); return (Queue: queue, Results: new int[n]); },
                state => { for (int i = 0; i < n; i++) state.Results[i] = state.Queue.Dequeue()?.Priority ?? throw new InvalidOperationException("Queue lost request."); },
                state => { if (!expected.SequenceEqual(state.Results) || state.Queue.Count != 0) throw new InvalidOperationException("Incorrect drain."); });
            measurement.Run("drain", "heap", n, n,
                () => { var queue = new HeapQueue(); queue.EnqueueBatch(optimized); return (Queue: queue, Results: new int[n]); },
                state => { for (int i = 0; i < n; i++) state.Results[i] = state.Queue.Dequeue()?.Priority ?? throw new InvalidOperationException("Queue lost request."); },
                state => { if (!expected.SequenceEqual(state.Results) || state.Queue.Count != 0) throw new InvalidOperationException("Incorrect drain."); });
        }
        measurement.Save();
    }

    private static void Validate(int[] expected, Func<int?> dequeue)
    {
        foreach (int priority in expected)
            if (dequeue() != priority) throw new InvalidOperationException("Incorrect queue order.");
        if (dequeue() is not null) throw new InvalidOperationException("Queue retained requests.");
    }
}
