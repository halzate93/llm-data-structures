using DataStructures.Baseline;
using DataStructures.Optimized;

namespace Demo;

internal static class Activity1
{
    public static void Run()
    {
        var baseline = new BinaryTree();
        var optimized = new AvlTree();
        foreach (int value in new[] { 10, 20, 30, 40, 50, 30 })
        {
            baseline.Insert(value);
            optimized.Insert(value);
        }
        Console.WriteLine("Activity 1: AVL traversal = " + string.Join(", ", optimized.InOrder()));
        Console.WriteLine($"Contains 40: {optimized.Contains(40)}; height: {optimized.Height}");
        if (!optimized.ValidateInvariants() || !baseline.Contains(40))
            throw new InvalidOperationException("Tree demonstration failed.");
    }
}
