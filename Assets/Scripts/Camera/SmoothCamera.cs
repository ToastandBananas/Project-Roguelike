using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    float interpVelocity;

    public GameObject target;
    public Vector3 offset;

    Vector3 targetPos;
    
    void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player");

        if (target != null)
            targetPos = target.transform.position;
        else
            Debug.LogError("Player gameobject not found!");
    }
    
    void LateUpdate()
    {
        if (target)
        {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.transform.position.z;

            Vector3 targetDirection = (target.transform.position - posNoZ);

            interpVelocity = targetDirection.magnitude * 30f;

            targetPos = transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, targetPos + offset, 0.25f);
        }
    }
}
