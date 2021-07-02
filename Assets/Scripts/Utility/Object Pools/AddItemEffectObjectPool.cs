using System.Collections.Generic;
using UnityEngine;

public class AddItemEffectObjectPool : ObjectPool
{
    [HideInInspector] public List<AddItemEffect> pooledAddItemEffects = new List<AddItemEffect>();

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
                AddItemEffect existingAddItemEffect = transform.GetChild(i).GetComponent<AddItemEffect>();
                pooledAddItemEffects.Add(existingAddItemEffect);
                pooledObjects.Add(existingAddItemEffect.gameObject);
            }

            // Create and add new objects to the list
            AddItemEffect addItemEffect;
            for (int i = 0; i < amountToPool; i++)
            {
                addItemEffect = Instantiate(objectToPool.GetComponent<AddItemEffect>());
                addItemEffect.transform.SetParent(transform);
                addItemEffect.gameObject.SetActive(false);
                pooledAddItemEffects.Add(addItemEffect);
                pooledObjects.Add(addItemEffect.gameObject);
            }

            hasBeenInitialized = true;
        }
    }

    public AddItemEffect GetPooledAddItemEffect()
    {
        for (int i = 0; i < pooledAddItemEffects.Count; i++)
        {
            if (pooledAddItemEffects[i].gameObject.activeSelf == false)
                return pooledAddItemEffects[i];
        }

        AddItemEffect anim = Instantiate(objectToPool).GetComponent<AddItemEffect>();
        anim.transform.SetParent(transform);
        pooledAddItemEffects.Add(anim);
        pooledObjects.Add(anim.gameObject);
        return anim;
    }
}
