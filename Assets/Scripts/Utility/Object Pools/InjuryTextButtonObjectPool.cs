using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjuryTextButtonObjectPool : ObjectPool
{
    [HideInInspector] public List<InjuryTextButton> pooledInjuryTextButtons = new List<InjuryTextButton>();

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
                InjuryTextButton existingInjuryTextButton = transform.GetChild(i).GetComponent<InjuryTextButton>();
                pooledInjuryTextButtons.Add(existingInjuryTextButton);
                pooledObjects.Add(existingInjuryTextButton.gameObject);
            }

            // Create and add new objects to the list
            InjuryTextButton injuryTextButton;
            for (int i = 0; i < amountToPool; i++)
            {
                injuryTextButton = Instantiate(objectToPool.GetComponent<InjuryTextButton>());
                injuryTextButton.transform.SetParent(transform);
                injuryTextButton.gameObject.SetActive(false);
                pooledInjuryTextButtons.Add(injuryTextButton);
                pooledObjects.Add(injuryTextButton.gameObject);
            }

            hasBeenInitialized = true;
        }
    }

    public InjuryTextButton GetPooledInjuryTextButton()
    {
        for (int i = 0; i < pooledInjuryTextButtons.Count; i++)
        {
            if (pooledInjuryTextButtons[i].gameObject.activeSelf == false)
                return pooledInjuryTextButtons[i];
        }

        InjuryTextButton injuryTextButton = Instantiate(objectToPool).GetComponent<InjuryTextButton>();
        injuryTextButton.transform.SetParent(transform);
        pooledInjuryTextButtons.Add(injuryTextButton);
        pooledObjects.Add(injuryTextButton.gameObject);
        return injuryTextButton;
    }
}
