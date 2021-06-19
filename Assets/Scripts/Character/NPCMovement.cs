using Pathfinding;
using System.Collections;
using UnityEngine;

public class NPCMovement : Movement
{
    AIPath aiPath;
    AIDestinationSetter aiDestSetter;
    Seeker seeker;

    public override void Start()
    {
        base.Start();

        aiPath = GetComponent<AIPath>();
        aiDestSetter = GetComponent<AIDestinationSetter>();
        seeker = GetComponent<Seeker>();

        turnManager.npcs.Add(this);
    }

    public void TakeTurn()
    {
        StartCoroutine(MoveToNextPointOnPath());
    }

    IEnumerator MoveToNextPointOnPath()
    {
        aiPath.SearchPath();

        while (aiPath.pathPending)
        {
            yield return null;
        }

        StartCoroutine(SmoothMovement(GetNextPosition(), true));
    }

    Vector3 GetNextPosition()
    {
        Path path = seeker.GetCurrentPath();
        if (path.vectorPath.Count <= 1)
            return transform.position;
        else
        {
            Vector3 dir = (path.vectorPath[1] - transform.position).normalized;
            Vector3 nextPos;
            if (dir == new Vector3(0, 1) || dir == new Vector3(0, -1) || dir == new Vector3(-1, 0) || dir == new Vector3(1, 0)) // Up, down, left, or right
                nextPos = transform.position + dir;
            else if (dir.x < 0 && dir.y > 0) // Up-left
                nextPos = transform.position + new Vector3(-1, 1);
            else if (dir.x > 0 && dir.y > 0) // Up-right
                nextPos = transform.position + new Vector3(1, 1);
            else if (dir.x < 0 && dir.y < 0) // Down-left
                nextPos = transform.position + new Vector3(-1, -1);
            else if (dir.x > 0 && dir.y < 0) // Down-right
                nextPos = transform.position + new Vector3(1, -1);
            else
                return transform.position;

            if (gameTiles.gridGraph.GetNearest(nextPos).node.Tag == 31)
                return transform.position;
            else
                return nextPos;
        }
    }
}
