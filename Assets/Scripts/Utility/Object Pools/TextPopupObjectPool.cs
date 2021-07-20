using System.Collections.Generic;
using UnityEngine;

public class TextPopupObjectPool : ObjectPool
{
    [HideInInspector] public List<TextPopup> pooledTextPopups = new List<TextPopup>();

    public override void Awake()
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
                TextPopup existingTextPopups = transform.GetChild(i).GetComponent<TextPopup>();
                pooledTextPopups.Add(existingTextPopups);
                pooledObjects.Add(existingTextPopups.gameObject);
                existingTextPopups.gameObject.SetActive(false);
            }

            // Create and add new objects to the list
            TextPopup textPopup;
            for (int i = 0; i < amountToPool; i++)
            {
                textPopup = Instantiate(objectToPool.GetComponent<TextPopup>());
                textPopup.transform.SetParent(transform);
                textPopup.gameObject.SetActive(false);
                pooledTextPopups.Add(textPopup);
                pooledObjects.Add(textPopup.gameObject);
            }

            hasBeenInitialized = true;
        }
    }

    public TextPopup GetPooledTextPopup()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledObjects[i].activeInHierarchy == false)
                return pooledTextPopups[i];
        }

        TextPopup textPopup = Instantiate(objectToPool).GetComponent<TextPopup>();
        textPopup.transform.SetParent(transform);
        pooledTextPopups.Add(textPopup);
        pooledObjects.Add(textPopup.gameObject);

        return textPopup;
    }
}
