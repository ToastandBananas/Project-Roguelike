using UnityEngine;
using Pathfinding;

public class NodeBlocker : MonoBehaviour
{
    SingleNodeBlocker blocker;

    public void Start()
    {
        blocker = GetComponent<SingleNodeBlocker>();

        BlockCurrentPosition();
    }

    public void BlockCurrentPosition()
    {
        blocker.BlockAtCurrentPosition();
    }
}
