namespace DataStructures.Baseline;

public sealed class ApiRequest(string endpoint, int priority)
{
    public string Endpoint { get; set; } = endpoint;
    public int Priority { get; set; } = priority;
}

public sealed class ApiRequestQueue
{
    private readonly List<ApiRequest> requests = [];
    public int Count => requests.Count;

    public void Enqueue(ApiRequest request)
    {
        requests.Add(request);
        requests.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public ApiRequest? Dequeue()
    {
        if (requests.Count == 0) return null;
        ApiRequest next = requests[0];
        requests.RemoveAt(0);
        return next;
    }
}
