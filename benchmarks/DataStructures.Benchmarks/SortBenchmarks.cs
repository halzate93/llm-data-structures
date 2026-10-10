using DataStructures.Optimized;

namespace DataStructures.Benchmarks;

internal static class SortBenchmarks
{
    public static void Run()
    {
        var measurement = new Measurement(3);
        foreach (int n in new[] { 256, 4096, 32768, 262144 })
        foreach (string order in new[] { "random", "reverse" })
        {
            var random = new Random(42);
            int[] values = order == "random"
                ? Enumerable.Range(0, n).Select(_ => random.Next()).ToArray()
                : Enumerable.Range(0, n).Reverse().ToArray();
            int[] expected = values.Order().ToArray();
            if (n <= 32768)
                Measure("bubble", DataStructures.Baseline.Sorting.BubbleSort);
            Measure("heap", Sorting.HeapSort);
            Measure("merge", input => Sorting.MergeSort(input));
            Measure("parallel-merge", input => Sorting.MergeSort(input, parallel: true));

            void Measure(string implementation, Action<int[]> sort) =>
                measurement.Run(order, implementation, n, n, () => (int[])values.Clone(), sort,
                    actual => { if (!expected.SequenceEqual(actual)) throw new InvalidOperationException("Incorrect sorting output."); });
        }
        measurement.Save();
    }
}
