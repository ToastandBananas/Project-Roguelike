using Pathfinding;
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

    public void MoveToNextPointOnPath()
    {
        Path path = seeker.GetCurrentPath();
        Vector3 nextDest = path.vectorPath[1];
        StartCoroutine(SmoothMovement(nextDest, true));
    }
}
