using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace DataStructures.Benchmarks;

internal sealed class Measurement(int activity)
{
    private readonly List<string> rows =
        ["activity,workload,implementation,n,operations,sample,elapsed_ms,allocated_bytes"];

    public void Run<T>(string workload, string implementation, int n, int operations,
        Func<T> setup, Action<T> execute, Action<T> validate)
    {
        for (int sample = -2; sample < 7; sample++)
        {
            T state = setup();
            long before = GC.GetTotalAllocatedBytes(true);
            long start = Stopwatch.GetTimestamp();
            execute(state);
            double milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            long allocated = GC.GetTotalAllocatedBytes(true) - before;
            validate(state);
            if (sample >= 0)
                rows.Add(FormattableString.Invariant(
                    $"{activity},{workload},{implementation},{n},{operations},{sample},{milliseconds:F6},{allocated}"));
        }
        var recent = rows.TakeLast(7).Select(row =>
            double.Parse(row.Split(',')[6], CultureInfo.InvariantCulture)).Order().ToArray();
        Console.WriteLine($"{activity}: {workload}, {implementation}, n={n}, median={recent[3]:F3} ms");
    }

    public void Save()
    {
        Directory.CreateDirectory("outputs");
        File.WriteAllLines(Path.Combine("outputs", $"activity-{activity}-samples.csv"), rows, Encoding.UTF8);
        File.WriteAllText(Path.Combine("outputs", $"activity-{activity}-environment.txt"),
            $"Runtime: {RuntimeInformation.FrameworkDescription}\nOS: {RuntimeInformation.OSDescription}\n"
            + $"Architecture: {RuntimeInformation.ProcessArchitecture}\nLogical processors: {Environment.ProcessorCount}\n"
            + $"Configuration: Release\nSeed: 42\nWarmups: 2\nMeasured samples: 7\n"
            + "Elapsed time: Stopwatch; allocation: process-wide GC.GetTotalAllocatedBytes(true).\n"
            + "Setup and validation excluded; concurrent runtime allocations may contribute.\n");
    }
}
