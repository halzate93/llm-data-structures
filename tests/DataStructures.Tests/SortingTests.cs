using DataStructures.Optimized;

namespace DataStructures.Tests;

public sealed class SortingTests
{
    public static IEnumerable<object[]> Inputs()
    {
        yield return [Array.Empty<int>()];
        yield return [new[] { 7 }];
        yield return [new[] { int.MaxValue, 0, int.MinValue, 0, -7, int.MaxValue }];
        yield return [Enumerable.Range(-100, 200).ToArray()];
        yield return [Enumerable.Range(-100, 200).Reverse().ToArray()];
        yield return [Enumerable.Repeat(1, 256).ToArray()];
        var random = new Random(42);
        foreach (int count in new[] { 31, 32, 33, Sorting.ParallelThreshold - 1,
            Sorting.ParallelThreshold, Sorting.ParallelThreshold + 1, 65537 })
            yield return [Enumerable.Range(0, count).Select(_ => random.Next(-1000, 1000)).ToArray()];
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void OptimizedSortsMatchReference(int[] input)
    {
        int[] expected = (int[])input.Clone();
        Array.Sort(expected);
        foreach (Action<int[]> sort in new Action<int[]>[]
        {
            Sorting.HeapSort, values => Sorting.MergeSort(values),
            values => Sorting.MergeSort(values, parallel: true)
        })
        {
            int[] actual = (int[])input.Clone();
            sort(actual);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void BubbleSortMatchesReferenceForSmallInputs()
    {
        foreach (object[] item in Inputs().Take(9))
        {
            var actual = (int[])((int[])item[0]).Clone();
            int[] expected = actual.Order().ToArray();
            DataStructures.Baseline.Sorting.BubbleSort(actual);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void OptimizedSortsRejectNullExplicitly()
    {
        Assert.Throws<ArgumentNullException>(() => Sorting.HeapSort(null!));
        Assert.Throws<ArgumentNullException>(() => Sorting.MergeSort(null!));
        Assert.Throws<ArgumentNullException>(() => Sorting.MergeSort(null!, true));
    }
}
