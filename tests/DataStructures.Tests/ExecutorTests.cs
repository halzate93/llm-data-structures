using DataStructures.Optimized;

namespace DataStructures.Tests;

public sealed class ExecutorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InvalidTasksAreLoggedAndRejected(string? task)
    {
        var logs = new List<ExecutionLog>();
        var executor = new TaskExecutor(logger: logs.Add);
        Assert.Throws<ArgumentException>(() => executor.AddTask(task));
        Assert.Equal(0, executor.PendingCount);
        var log = Assert.Single(logs);
        Assert.Equal(ExecutionEvent.ValidationRejected, log.Event);
        Assert.NotNull(log.Error);
    }

    [Fact]
    public void PermanentFailureDoesNotStopLaterTasksOrRetry()
    {
        var logs = new List<ExecutionLog>();
        var executor = new TaskExecutor(logger: logs.Add);
        executor.AddTask("Task 1");
        executor.AddTask("Fail Task");
        executor.AddTask("Task 2");
        var results = executor.ProcessTasks();
        Assert.Equal(new[] { true, false, true }, results.Select(result => result.Successful));
        Assert.All(results, result => Assert.Equal(1, result.Attempts));
        Assert.NotNull(results[1].Error);
        Assert.Single(logs.Where(log => log.Event == ExecutionEvent.Failed));
        Assert.Empty(logs.Where(log => log.Event == ExecutionEvent.Retrying));
        Assert.Equal(0, executor.PendingCount);
        Assert.Empty(executor.ProcessTasks());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TransientFailuresRecoverWithinBound(int failures)
    {
        int calls = 0;
        var logs = new List<ExecutionLog>();
        var executor = new TaskExecutor(_ =>
        {
            if (++calls <= failures) throw new TransientTaskException("Try again.");
        }, logs.Add);
        executor.AddTask("Task");
        var result = Assert.Single(executor.ProcessTasks());
        Assert.True(result.Successful);
        Assert.Equal(failures + 1, result.Attempts);
        Assert.Equal(failures, logs.Count(log => log.Event == ExecutionEvent.Retrying));
        Assert.Null(result.Error);
    }

    [Fact]
    public void RetryExhaustionRecordsFailureAndContinues()
    {
        int calls = 0;
        var logs = new List<ExecutionLog>();
        var executor = new TaskExecutor(task =>
        {
            if (task == "Transient")
            {
                calls++;
                throw new TransientTaskException("Still unavailable.");
            }
        }, logs.Add);
        executor.AddTask("Transient");
        executor.AddTask("Next");
        var results = executor.ProcessTasks();
        Assert.Equal(TaskExecutor.MaximumAttempts, calls);
        Assert.False(results[0].Successful);
        Assert.Equal(3, results[0].Attempts);
        Assert.True(results[1].Successful);
        Assert.Equal(2, logs.Count(log => log.Event == ExecutionEvent.Retrying));
        Assert.Single(logs.Where(log => log.Event == ExecutionEvent.Failed));
    }

    [Fact]
    public void UnexpectedHandlerErrorsAreNotSwallowed()
    {
        var logs = new List<ExecutionLog>();
        var executor = new TaskExecutor(_ => throw new InvalidOperationException("Programming error."), logs.Add);
        executor.AddTask("First");
        executor.AddTask("Second");
        Assert.Throws<InvalidOperationException>(() => executor.ProcessTasks());
        Assert.Equal(1, executor.PendingCount);
        Assert.DoesNotContain(logs, log => log.Event is ExecutionEvent.Completed or ExecutionEvent.Failed);
    }

    [Fact]
    public void LoggingFailureIsNotSilentlyIgnored()
    {
        var executor = new TaskExecutor(logger: _ => throw new IOException("Logging unavailable."));
        Assert.Throws<IOException>(() => executor.AddTask(null));
        Assert.Equal(0, executor.PendingCount);
    }

    [Fact]
    public void BaselineReproducesOriginalAbort()
    {
        var executor = new DataStructures.Baseline.TaskExecutor(TextWriter.Null);
        executor.AddTask("Task 1");
        executor.AddTask(null);
        executor.AddTask("Fail Task");
        executor.AddTask("Task 2");
        var error = Assert.Throws<Exception>(() => executor.ProcessTasks());
        Assert.Equal("Task is null", error.Message);
        Assert.Equal(1, executor.CompletedCount);
        Assert.Equal(2, executor.PendingCount);
    }

    [Fact]
    public void BaselinePermanentFailureAlsoAborts()
    {
        var executor = new DataStructures.Baseline.TaskExecutor(TextWriter.Null);
        executor.AddTask("Fail Task");
        executor.AddTask("Task 2");
        var error = Assert.Throws<Exception>(() => executor.ProcessTasks());
        Assert.Equal("Task execution failed", error.Message);
        Assert.Equal(1, executor.PendingCount);
    }
}
