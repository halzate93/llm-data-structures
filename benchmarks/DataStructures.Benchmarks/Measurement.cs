using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace DataStructures.Benchmarks;

internal sealed class Measurement(int activity)
{
    private const string Header = "activity,workload,implementation,n,operations,sample,elapsed_ms,allocated_bytes";
    private readonly List<string> rows =
        [Header];

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
#if DEBUG
        string configuration = "Debug";
#else
        string configuration = "Release";
#endif
        Directory.CreateDirectory("outputs");
        File.WriteAllLines(Path.Combine("outputs", $"activity-{activity}-samples.csv"), rows, Encoding.UTF8);
        File.WriteAllText(Path.Combine("outputs", $"activity-{activity}-environment.txt"),
            $"Runtime: {RuntimeInformation.FrameworkDescription}\nOS: {RuntimeInformation.OSDescription}\n"
            + $"Architecture: {RuntimeInformation.ProcessArchitecture}\nLogical processors: {Environment.ProcessorCount}\n"
            + $"Configuration: {configuration}\nSeed: 42\nWarmups: 2\nMeasured samples: 7\n"
            + "Elapsed time: Stopwatch; allocation: process-wide GC.GetTotalAllocatedBytes(true).\n"
            + "Setup and validation excluded; concurrent runtime allocations may contribute.\n");
    }

    public static void Consolidate()
    {
        var combined = new List<string> { Header };
        foreach (string path in Directory.EnumerateFiles("outputs", "activity-*-samples.csv").Order())
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0 || lines[0] != Header)
                throw new InvalidDataException($"Unexpected benchmark schema: {path}");
            combined.AddRange(lines.Skip(1));
        }
        if (combined.Count == 1) throw new InvalidOperationException("No benchmark samples to consolidate.");
        File.WriteAllLines(Path.Combine("outputs", "benchmark-results.csv"), combined, Encoding.UTF8);
    }
}
