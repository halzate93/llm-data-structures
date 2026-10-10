namespace DataStructures.Baseline;

public sealed class Node(int value)
{
    public int Value = value;
    public Node? Left;
    public Node? Right;
}

public sealed class BinaryTree
{
    public Node? Root;

    public void Insert(int value)
    {
        if (Root is null)
            Root = new Node(value);
        else
            InsertRecursive(Root, value);
    }

    private static void InsertRecursive(Node current, int value)
    {
        if (value < current.Value)
        {
            if (current.Left is null)
                current.Left = new Node(value);
            else
                InsertRecursive(current.Left, value);
        }
        else
        {
            if (current.Right is null)
                current.Right = new Node(value);
            else
                InsertRecursive(current.Right, value);
        }
    }

    // Copilot added iterative search to measure retrieval fairly; insertion is unchanged.
    public bool Contains(int value)
    {
        var node = Root;
        while (node is not null)
        {
            if (value == node.Value) return true;
            node = value < node.Value ? node.Left : node.Right;
        }
        return false;
    }

    public void PrintInOrder(Node? node)
    {
        if (node is null) return;
        PrintInOrder(node.Left);
        Console.Write(node.Value + " ");
        PrintInOrder(node.Right);
    }
}
