namespace DataStructures.Optimized;

public sealed class AvlTree
{
    private sealed class Node(int value)
    {
        public readonly int Value = value;
        public int Multiplicity = 1;
        public int Height = 1;
        public Node? Left;
        public Node? Right;
    }

    private Node? root;
    public int Height => GetHeight(root);
    public int Count { get; private set; }

    public void Insert(int value)
    {
        root = Insert(root, value);
        Count++;
    }

    public bool Contains(int value)
    {
        var node = root;
        while (node is not null)
        {
            if (value == node.Value) return true;
            node = value < node.Value ? node.Left : node.Right;
        }
        return false;
    }

    public IEnumerable<int> InOrder()
    {
        var stack = new Stack<Node>();
        var node = root;
        while (node is not null || stack.Count > 0)
        {
            while (node is not null)
            {
                stack.Push(node);
                node = node.Left;
            }
            node = stack.Pop();
            for (int i = 0; i < node.Multiplicity; i++)
                yield return node.Value;
            node = node.Right;
        }
    }

    private static int GetHeight(Node? node) => node?.Height ?? 0;
    private static int Balance(Node node) => GetHeight(node.Left) - GetHeight(node.Right);
    private static void UpdateHeight(Node node) =>
        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

    private static Node Insert(Node? node, int value)
    {
        if (node is null) return new Node(value);
        if (value < node.Value)
            node.Left = Insert(node.Left, value);
        else if (value > node.Value)
            node.Right = Insert(node.Right, value);
        else
        {
            // Copilot refinement: retain duplicates without growing a duplicate-only chain.
            node.Multiplicity++;
            return node;
        }

        // Cached heights make each Copilot-assisted AVL rebalance constant-time.
        UpdateHeight(node);
        if (Balance(node) > 1 && node.Left is { } left)
        {
            if (Balance(left) < 0) node.Left = RotateLeft(left);
            return RotateRight(node);
        }
        if (Balance(node) < -1 && node.Right is { } right)
        {
            if (Balance(right) > 0) node.Right = RotateRight(right);
            return RotateLeft(node);
        }
        return node;
    }

    private static Node RotateRight(Node node)
    {
        var pivot = node.Left ?? throw new InvalidOperationException("Missing left rotation pivot.");
        node.Left = pivot.Right;
        pivot.Right = node;
        UpdateHeight(node);
        UpdateHeight(pivot);
        return pivot;
    }

    private static Node RotateLeft(Node node)
    {
        var pivot = node.Right ?? throw new InvalidOperationException("Missing right rotation pivot.");
        node.Right = pivot.Left;
        pivot.Left = node;
        UpdateHeight(node);
        UpdateHeight(pivot);
        return pivot;
    }

    public bool ValidateInvariants()
    {
        var result = Validate(root, long.MinValue, long.MaxValue);
        return result.Valid && result.Count == Count;
    }

    private static (bool Valid, int Height, int Count) Validate(Node? node, long min, long max)
    {
        if (node is null) return (true, 0, 0);
        var left = Validate(node.Left, min, node.Value);
        var right = Validate(node.Right, node.Value, max);
        int height = 1 + Math.Max(left.Height, right.Height);
        return (left.Valid && right.Valid && node.Value > min && node.Value < max
            && node.Multiplicity > 0 && node.Height == height
            && Math.Abs(left.Height - right.Height) <= 1,
            height, left.Count + right.Count + node.Multiplicity);
    }
}
