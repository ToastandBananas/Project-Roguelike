using UnityEngine;

public class ItemHighlight : MonoBehaviour
{
    public Material highlightMaterial;

    Material originalMaterial;
    SpriteRenderer sr;
    
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalMaterial = sr.material;
    }

    public void Highlight()
    {
        sr.material = highlightMaterial;
    }

    public void RemoveHighlight()
    {
        sr.material = originalMaterial;
    }
}
