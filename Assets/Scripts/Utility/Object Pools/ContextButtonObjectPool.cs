using System.Collections.Generic;
using UnityEngine;

public class ContextButtonObjectPool : ObjectPool
{
    [HideInInspector] public List<ContextMenuButton> pooledContextButtons = new List<ContextMenuButton>();

    public override void Start()
    {
        Init();
    }

    public override void Init()
    {
        if (hasBeenInitialized == false)
        {
            // Add existing objects to the list
            for (int i = 0; i < transform.childCount; i++)
            {
                ContextMenuButton existingContextButtons = transform.GetChild(i).GetComponent<ContextMenuButton>();
                pooledContextButtons.Add(existingContextButtons);
                pooledObjects.Add(existingContextButtons.gameObject);
            }

            // Create and add new objects to the list
            ContextMenuButton contextButton;
            for (int i = 0; i < amountToPool; i++)
            {
                contextButton = Instantiate(objectToPool.GetComponent<ContextMenuButton>());
                contextButton.transform.SetParent(transform);
                contextButton.gameObject.SetActive(false);
                pooledContextButtons.Add(contextButton);
                pooledObjects.Add(contextButton.gameObject);
            }

            hasBeenInitialized = true;
        }
    }

    public ContextMenuButton GetPooledContextButton()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledObjects[i].activeInHierarchy == false)
                return pooledContextButtons[i];
        }

        ContextMenuButton contextButton = Instantiate(objectToPool).GetComponent<ContextMenuButton>();
        contextButton.transform.SetParent(transform);
        pooledContextButtons.Add(contextButton);
        pooledObjects.Add(contextButton.gameObject);

        return contextButton;
    }
}
