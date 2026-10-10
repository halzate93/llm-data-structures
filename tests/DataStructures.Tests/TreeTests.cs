using DataStructures.Baseline;
using DataStructures.Optimized;

namespace DataStructures.Tests;

public sealed class TreeTests
{
    [Theory]
    [InlineData(30, 20, 10)]
    [InlineData(10, 20, 30)]
    [InlineData(30, 10, 20)]
    [InlineData(10, 30, 20)]
    public void AllRotationCasesPreserveOrder(int a, int b, int c)
    {
        var tree = new AvlTree();
        foreach (int value in new[] { a, b, c })
        {
            tree.Insert(value);
            Assert.True(tree.ValidateInvariants());
        }
        Assert.Equal(new[] { 10, 20, 30 }, tree.InOrder());
        Assert.Equal(2, tree.Height);
    }

    [Theory]
    [InlineData("ordered")]
    [InlineData("reverse")]
    [InlineData("random")]
    [InlineData("duplicates")]
    public void LargeInputsStayBalanced(string order)
    {
        var random = new Random(42);
        int[] values = order switch
        {
            "reverse" => Enumerable.Range(0, 4096).Reverse().ToArray(),
            "random" => Enumerable.Range(0, 4096).Select(_ => random.Next(-1000, 1000)).ToArray(),
            "duplicates" => Enumerable.Repeat(7, 4096).ToArray(),
            _ => Enumerable.Range(0, 4096).ToArray()
        };
        var tree = new AvlTree();
        foreach (int value in values)
        {
            tree.Insert(value);
            Assert.True(tree.ValidateInvariants());
        }
        Assert.Equal(values.Order(), tree.InOrder());
        Assert.Equal(values.Length, tree.Count);
        Assert.True(tree.Height <= Math.Ceiling(1.45 * Math.Log2(values.Length + 2)));
        Assert.All(values, value => Assert.True(tree.Contains(value)));
        Assert.False(tree.Contains(int.MinValue));
    }

    [Fact]
    public void EmptyAndExtremeValuesAreSupported()
    {
        var tree = new AvlTree();
        Assert.Empty(tree.InOrder());
        Assert.False(tree.Contains(0));
        Assert.Equal(0, tree.Height);
        Assert.True(tree.ValidateInvariants());
        foreach (int value in new[] { int.MinValue, 0, int.MaxValue, int.MinValue })
            tree.Insert(value);
        Assert.True(tree.ValidateInvariants());
        Assert.Equal(new[] { int.MinValue, int.MinValue, 0, int.MaxValue }, tree.InOrder());
    }

    [Fact]
    public void BaselineExposesDegenerationButPreservesDuplicates()
    {
        var tree = new BinaryTree();
        foreach (int value in Enumerable.Range(0, 100)) tree.Insert(value);
        tree.Insert(99);
        int depth = 0;
        var node = tree.Root;
        while (node is not null)
        {
            Assert.Null(node.Left);
            depth++;
            node = node.Right;
        }
        Assert.Equal(101, depth);
        Assert.True(tree.Contains(99));
        Assert.False(tree.Contains(100));
        var empty = new BinaryTree();
        Assert.False(empty.Contains(1));
    }
}
