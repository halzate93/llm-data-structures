using DataStructures.Optimized;

namespace Demo;

internal static class Activity4
{
    public static void Run()
    {
        int transientAttempts = 0;
        var executor = new TaskExecutor(task =>
        {
            if (task.Contains("Fail", StringComparison.Ordinal))
                throw new PermanentTaskException("Demonstration permanent failure.");
            if (task == "Transient Task" && ++transientAttempts < 3)
                throw new TransientTaskException("Demonstration transient failure.");
        });
        executor.AddTask("Task 1");
        try { executor.AddTask(null); }
        catch (ArgumentException error) { Console.WriteLine("Expected rejection: " + error.Message); }
        executor.AddTask("Fail Task");
        executor.AddTask("Transient Task");
        executor.AddTask("Task 2");
        var results = executor.ProcessTasks();
        Console.WriteLine($"Activity 4: {results.Count(result => result.Successful)} completed, "
            + $"{results.Count(result => !result.Successful)} failed, {executor.PendingCount} pending.");
        if (results.Count != 4 || !results[^1].Successful || results[2].Attempts != 3)
            throw new InvalidOperationException("Execution demonstration failed.");
    }
}
