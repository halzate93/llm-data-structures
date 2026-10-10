using DataStructures.Optimized;

namespace Demo;

internal static class Activity3
{
    public static void Run()
    {
        int[] values = [50, 20, 40, 10, 30];
        Sorting.HeapSort(values);
        Console.WriteLine("Activity 3: heapsort = " + string.Join(", ", values));
        var random = new Random(42);
        int[] large = Enumerable.Range(0, 32768).Select(_ => random.Next()).ToArray();
        int[] expected = large.Order().ToArray();
        Sorting.MergeSort(large, parallel: true);
        if (!values.SequenceEqual(new[] { 10, 20, 30, 40, 50 }) || !large.SequenceEqual(expected))
            throw new InvalidOperationException("Sorting demonstration failed.");
        Console.WriteLine($"Parallel merge sort verified on {large.Length} values.");
    }
}
