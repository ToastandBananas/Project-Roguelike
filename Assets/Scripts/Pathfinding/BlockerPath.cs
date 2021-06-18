using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public class BlockerPath : MonoBehaviour
{
    public BlockManager blockManager;
    public List<SingleNodeBlocker> obstacles;
    public Transform target;
    public Vector3 targetPos;

    BlockManager.TraversalProvider traversalProvider;

    public void Start()
    {
        if (blockManager == null)
            blockManager = FindObjectOfType<BlockManager>();

        // Create a traversal provider which says that a path should be blocked by all the SingleNodeBlockers in the obstacles array
        traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.OnlySelector, obstacles);
    }

    public void Update()
    {
        CreatePath();
    }

    public void CreatePath()
    {
        // Create a new Path object
        ABPath path;
        if (target != null)
            path = ABPath.Construct(transform.position, target.position, null);
        else
            path = ABPath.Construct(transform.position, targetPos);

        // Make the path use a specific traversal provider
        path.traversalProvider = traversalProvider;
        //path.traversalProvider = new CustomTraversalProvider();

        // Calculate the path synchronously
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        if (path.error)
        {
            //Debug.Log("No path was found");
        }
        else
        {
            //Debug.Log("A path was found with " + path.vectorPath.Count + " nodes");

            // Draw the path in the scene view
            for (int i = 0; i < path.vectorPath.Count - 1; i++)
            {
                Debug.DrawLine(path.vectorPath[i], path.vectorPath[i + 1], Color.red);
            }
        }
    }
}