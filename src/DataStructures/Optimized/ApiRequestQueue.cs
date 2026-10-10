namespace DataStructures.Optimized;

public sealed class ApiRequest
{
    public string Endpoint { get; }
    public int Priority { get; }

    public ApiRequest(string endpoint, int priority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        Endpoint = endpoint;
        Priority = priority;
    }
}

public sealed class ApiRequestQueue
{
    private readonly record struct Entry(ApiRequest Request, long Sequence);
    private readonly List<Entry> heap = [];
    private readonly object gate = new();
    private long nextSequence;

    public int Count { get { lock (gate) return heap.Count; } }

    public void Enqueue(ApiRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (gate)
        {
            long following = checked(nextSequence + 1);
            heap.Add(new Entry(request, nextSequence));
            nextSequence = following;
            int index = heap.Count - 1;
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (!Less(heap[index], heap[parent])) break;
                (heap[index], heap[parent]) = (heap[parent], heap[index]);
                index = parent;
            }
        }
    }

    public void EnqueueBatch(IEnumerable<ApiRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);
        // Copilot refinement: enumerate/validate before locking so invalid batches cannot partly enqueue.
        ApiRequest[] batch = requests.ToArray();
        foreach (var request in batch) ArgumentNullException.ThrowIfNull(request);
        if (batch.Length == 0) return;
        lock (gate)
        {
            long following = checked(nextSequence + batch.Length);
            heap.EnsureCapacity(checked(heap.Count + batch.Length));
            foreach (var request in batch) heap.Add(new Entry(request, nextSequence++));
            nextSequence = following;
            // Bottom-up heapify costs O(n + k), rather than sorting after every addition.
            for (int index = heap.Count / 2 - 1; index >= 0; index--) SiftDown(index);
        }
    }

    public ApiRequest? Dequeue()
    {
        lock (gate)
        {
            if (heap.Count == 0) return null;
            ApiRequest request = heap[0].Request;
            heap[0] = heap[^1];
            heap.RemoveAt(heap.Count - 1);
            if (heap.Count > 0) SiftDown(0);
            return request;
        }
    }

    private static bool Less(Entry a, Entry b) =>
        a.Request.Priority < b.Request.Priority
        || (a.Request.Priority == b.Request.Priority && a.Sequence < b.Sequence);

    private void SiftDown(int index)
    {
        while (index < heap.Count / 2)
        {
            int child = 2 * index + 1;
            if (child + 1 < heap.Count && Less(heap[child + 1], heap[child])) child++;
            if (!Less(heap[child], heap[index])) break;
            (heap[index], heap[child]) = (heap[child], heap[index]);
            index = child;
        }
    }
}
