using DataStructures.Optimized;
using BaselineExecutor = DataStructures.Baseline.TaskExecutor;

namespace DataStructures.Benchmarks;

internal static class ExecutorBenchmarks
{
    private sealed class State(TaskExecutor executor)
    {
        public TaskExecutor Executor { get; } = executor;
        public IReadOnlyList<ExecutionResult> Results { get; set; } = [];
    }

    public static void Run()
    {
        var measurement = new Measurement(4);
        foreach (int n in new[] { 256, 8192 })
        {
            string[] tasks = Enumerable.Range(0, n).Select(i => $"Task {i}").ToArray();
            measurement.Run("valid-process", "baseline", n, n,
                () => { var executor = new BaselineExecutor(TextWriter.Null); foreach (string task in tasks) executor.AddTask(task); return executor; },
                executor => executor.ProcessTasks(),
                executor => { if (executor.CompletedCount != n || executor.PendingCount != 0) throw new InvalidOperationException("Baseline lost work."); });
            measurement.Run("valid-process", "optimized", n, n,
                () => { var executor = new TaskExecutor(logger: DiscardFormattedLog); foreach (string task in tasks) executor.AddTask(task); return new State(executor); },
                state => state.Results = state.Executor.ProcessTasks(),
                state => { if (state.Results.Count != n || state.Results.Any(result => !result.Successful || result.Attempts != 1) || state.Executor.PendingCount != 0) throw new InvalidOperationException("Executor lost work."); });
        }
        measurement.Save();
        RecordReliability();
    }

    private static void DiscardFormattedLog(ExecutionLog entry) => TextWriter.Null.WriteLine(
        $"[{entry.Event}] task={entry.Task}, attempt={entry.Attempt}, error={entry.Error}");

    private static void RecordReliability()
    {
        string?[] tasks = ["Task 1", null, "Fail Task", "Task 2"];
        var baseline = new BaselineExecutor(TextWriter.Null);
        foreach (string? task in tasks) baseline.AddTask(task);
        int aborted = 0;
        try { baseline.ProcessTasks(); }
        catch (Exception error) when (error.GetType() == typeof(Exception) && error.Message == "Task is null")
        {
            // Only the exact intentional baseline defect is caught, not unexpected benchmark failures.
            aborted++;
        }
        int rejected = 0;
        var optimized = new TaskExecutor(logger: DiscardFormattedLog);
        foreach (string? task in tasks)
        {
            try { optimized.AddTask(task); }
            catch (ArgumentException error) when (error.ParamName == "task") { rejected++; }
        }
        var results = optimized.ProcessTasks();
        int completed = results.Count(result => result.Successful);
        int failed = results.Count(result => !result.Successful);
        if (baseline.CompletedCount != 1 || aborted != 1 || baseline.PendingCount != 2
            || rejected != 1 || completed != 2 || failed != 1 || optimized.PendingCount != 0)
            throw new InvalidOperationException("Reliability comparison did not reproduce expected outcomes.");
        File.WriteAllLines(Path.Combine("outputs", "activity-4-reliability.csv"),
        [
            "implementation,submitted,rejected,completed,failed,aborted,pending",
            $"baseline,4,0,{baseline.CompletedCount},0,{aborted},{baseline.PendingCount}",
            $"optimized,4,{rejected},{completed},{failed},0,{optimized.PendingCount}"
        ]);
        Console.WriteLine("Activity 4 reliability: baseline aborted with 2 pending; optimized completed 2, rejected 1, failed 1, pending 0.");
    }
}
