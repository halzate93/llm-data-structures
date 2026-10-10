using DataStructures.Baseline;
using DataStructures.Optimized;

namespace DataStructures.Benchmarks;

internal static class TreeBenchmarks
{
    public static void Run()
    {
        var measurement = new Measurement(1);
        foreach (int n in new[] { 256, 2048 })
        foreach (string order in new[] { "ordered", "random" })
        {
            int[] values = Enumerable.Range(0, n).ToArray();
            if (order == "random") new Random(42).Shuffle(values);
            measurement.Run($"build-{order}", "unbalanced", n, n, () => new BinaryTree(),
                tree => { foreach (int value in values) tree.Insert(value); },
                tree => { if (!tree.Contains(n - 1)) throw new InvalidOperationException("Baseline build failed."); });
            measurement.Run($"build-{order}", "avl", n, n, () => new AvlTree(),
                tree => { foreach (int value in values) tree.Insert(value); },
                tree => { if (!tree.ValidateInvariants() || tree.Count != n) throw new InvalidOperationException("AVL build failed."); });
            measurement.Run($"search-{order}", "unbalanced", n, n,
                () => { var tree = new BinaryTree(); foreach (int value in values) tree.Insert(value); return tree; },
                tree => { foreach (int value in values) if (!tree.Contains(value)) throw new InvalidOperationException("Missing value."); },
                _ => { });
            measurement.Run($"search-{order}", "avl", n, n,
                () => { var tree = new AvlTree(); foreach (int value in values) tree.Insert(value); return tree; },
                tree => { foreach (int value in values) if (!tree.Contains(value)) throw new InvalidOperationException("Missing value."); },
                tree => { if (!tree.ValidateInvariants()) throw new InvalidOperationException("Invalid AVL."); });
        }
        measurement.Save();
    }
}
