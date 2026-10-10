namespace DataStructures.Baseline;

public sealed class TaskExecutor
{
    private readonly Queue<string?> taskQueue = [];
    private readonly TextWriter output;
    public int PendingCount => taskQueue.Count;
    public int CompletedCount { get; private set; }

    public TaskExecutor(TextWriter? output = null) => this.output = output ?? Console.Out;

    public void AddTask(string? task) => taskQueue.Enqueue(task);

    public void ProcessTasks()
    {
        while (taskQueue.Count > 0)
        {
            string? task = taskQueue.Dequeue();
            output.WriteLine($"Processing task: {task}");
            ExecuteTask(task);
            CompletedCount++;
        }
    }

    private void ExecuteTask(string? task)
    {
        if (task is null) throw new Exception("Task is null");
        if (task.Contains("Fail")) throw new Exception("Task execution failed");
        output.WriteLine($"Task {task} completed successfully.");
    }
}
