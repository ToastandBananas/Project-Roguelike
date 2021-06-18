using Pathfinding;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
    public BlockerPath myBlockerPath;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC") || collision.CompareTag("Player") || collision.CompareTag("Object") || collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent(out SingleNodeBlocker singleNodeBlocker))
                myBlockerPath.obstacles.Add(singleNodeBlocker);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || collision.CompareTag("Object") || collision.CompareTag("Player")))
        {
            if (collision.TryGetComponent(out SingleNodeBlocker singleNodeBlocker))
                myBlockerPath.obstacles.Remove(singleNodeBlocker);
        }
    }
}
