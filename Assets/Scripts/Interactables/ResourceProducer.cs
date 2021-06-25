using UnityEngine;

public class ResourceProducer : MonoBehaviour
{
    [Header("Resources Drops")]
    public Vector2 resourceDropOffset = new Vector2(2.5f, -0.8f);
    public Item[] resources;
    public int[] resourceCounts;

    [HideInInspector] public DropItemController dropItemController;

    public virtual void Start()
    {
        dropItemController = DropItemController.instance;
    }

    public virtual void ProduceResources()
    {

    }
}
