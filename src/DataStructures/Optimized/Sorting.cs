namespace DataStructures.Optimized;

public static class Sorting
{
    public const int ParallelThreshold = 16384;

    public static void HeapSort(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        // Copilot-assisted in-place max-heap avoids bubble sort's quadratic comparisons.
        for (int index = values.Length / 2 - 1; index >= 0; index--)
            SiftDown(values, index, values.Length);
        for (int end = values.Length - 1; end > 0; end--)
        {
            (values[0], values[end]) = (values[end], values[0]);
            SiftDown(values, 0, end);
        }
    }

    private static void SiftDown(int[] values, int index, int count)
    {
        while (index < count / 2)
        {
            int child = 2 * index + 1;
            if (child + 1 < count && values[child + 1] > values[child]) child++;
            if (values[index] >= values[child]) break;
            (values[index], values[child]) = (values[child], values[index]);
            index = child;
        }
    }

    public static void MergeSort(int[] values, bool parallel = false)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length < 2) return;
        var buffer = new int[values.Length];
        int depth = parallel ? Math.Min(4, (int)Math.Log2(Environment.ProcessorCount)) : 0;
        MergeSort(values, buffer, 0, values.Length, depth);
    }

    private static void MergeSort(int[] values, int[] buffer, int start, int end, int parallelDepth)
    {
        if (end - start <= 32)
        {
            for (int i = start + 1; i < end; i++)
            {
                int value = values[i];
                int j = i - 1;
                while (j >= start && values[j] > value)
                {
                    values[j + 1] = values[j];
                    j--;
                }
                values[j + 1] = value;
            }
            return;
        }

        int middle = start + (end - start) / 2;
        // Copilot refinement: disjoint ranges and bounded depth prevent unbounded task creation.
        if (parallelDepth > 0 && end - start >= ParallelThreshold)
        {
            Parallel.Invoke(
                () => MergeSort(values, buffer, start, middle, parallelDepth - 1),
                () => MergeSort(values, buffer, middle, end, parallelDepth - 1));
        }
        else
        {
            MergeSort(values, buffer, start, middle, 0);
            MergeSort(values, buffer, middle, end, 0);
        }

        int left = start, right = middle, output = start;
        while (left < middle && right < end)
            buffer[output++] = values[left] <= values[right] ? values[left++] : values[right++];
        while (left < middle) buffer[output++] = values[left++];
        while (right < end) buffer[output++] = values[right++];
        Array.Copy(buffer, start, values, start, end - start);
    }
}
