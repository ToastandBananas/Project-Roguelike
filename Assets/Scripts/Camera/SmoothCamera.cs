using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    public GameObject target;

    Camera cam;
    
    void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player");

        cam = Camera.main;
    }
    
    void LateUpdate()
    {
        if (target != null)
            transform.position = Vector3.Lerp(transform.position, target.transform.position + new Vector3(cam.orthographicSize / 2, 0, 0), 0.1f) + new Vector3(0, 0, -10);
    }
}
