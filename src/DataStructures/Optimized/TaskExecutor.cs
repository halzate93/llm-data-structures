namespace DataStructures.Optimized;

public sealed class TransientTaskException(string message) : Exception(message);
public sealed class PermanentTaskException(string message) : Exception(message);

public enum ExecutionEvent { ValidationRejected, Processing, Retrying, Completed, Failed }
public sealed record ExecutionLog(ExecutionEvent Event, string? Task, int Attempt, string? Error);
public sealed record ExecutionResult(string Task, bool Successful, int Attempts, string? Error);

public sealed class TaskExecutor
{
    public const int MaximumAttempts = 3;
    private readonly Queue<string> taskQueue = [];
    private readonly Action<string> handler;
    private readonly Action<ExecutionLog> logger;
    public int PendingCount => taskQueue.Count;

    public TaskExecutor(Action<string>? handler = null, Action<ExecutionLog>? logger = null)
    {
        this.handler = handler ?? ExecuteTask;
        this.logger = logger ?? (entry => Console.WriteLine(
            $"[{entry.Event}] task={entry.Task}, attempt={entry.Attempt}, error={entry.Error}"));
    }

    public void AddTask(string? task)
    {
        // Copilot-assisted validation rejects malformed work before it can poison the queue.
        if (string.IsNullOrWhiteSpace(task))
        {
            logger(new(ExecutionEvent.ValidationRejected, task, 0, "Task must not be null or blank."));
            throw new ArgumentException("Task must not be null or blank.", nameof(task));
        }
        taskQueue.Enqueue(task);
    }

    public IReadOnlyList<ExecutionResult> ProcessTasks()
    {
        var results = new List<ExecutionResult>();
        while (taskQueue.Count > 0)
        {
            string task = taskQueue.Dequeue();
            for (int attempt = 1; attempt <= MaximumAttempts; attempt++)
            {
                logger(new(ExecutionEvent.Processing, task, attempt, null));
                try
                {
                    handler(task);
                }
                // Copilot refinement: retry only declared transient failures, never arbitrary exceptions.
                catch (TransientTaskException error)
                {
                    if (attempt < MaximumAttempts)
                    {
                        logger(new(ExecutionEvent.Retrying, task, attempt, error.Message));
                        continue;
                    }
                    RecordFailure(task, attempt, error.Message, results);
                    break;
                }
                catch (PermanentTaskException error)
                {
                    RecordFailure(task, attempt, error.Message, results);
                    break;
                }
                logger(new(ExecutionEvent.Completed, task, attempt, null));
                results.Add(new(task, true, attempt, null));
                break;
            }
        }
        return results;
    }

    private void RecordFailure(string task, int attempt, string error, List<ExecutionResult> results)
    {
        logger(new(ExecutionEvent.Failed, task, attempt, error));
        results.Add(new(task, false, attempt, error));
    }

    private static void ExecuteTask(string task)
    {
        if (task.Contains("Fail", StringComparison.Ordinal))
            throw new PermanentTaskException("Task execution failed");
    }
}
