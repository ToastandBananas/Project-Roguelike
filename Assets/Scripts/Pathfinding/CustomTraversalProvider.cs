using Pathfinding;

public class CustomTraversalProvider : ITraversalProvider
{
    public bool CanTraverse(Path path, GraphNode node)
    {
        // Make sure that the node is walkable and that the 'enabledTags' bitmask includes the node's tag.
        return node.Walkable && (path.enabledTags >> (int)node.Tag & 0x1) != 0;
        // alternatively:
        // return DefaultITraversalProvider.CanTraverse(path, node);
    }

    public uint GetTraversalCost(Path path, GraphNode node)
    {
        // The traversal cost is the sum of the penalty of the node's tag and the node's penalty
        return path.GetTagPenalty((int)node.Tag) + node.Penalty;
        // alternatively:
        // return DefaultITraversalProvider.GetTraversalCost(path, node);
    }
}
