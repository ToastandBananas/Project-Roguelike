using UnityEngine;

public class Zoom : MonoBehaviour
{
    [SerializeField] Camera ppwzCamera;

    PerfectPixelWithZoom ppwz;

    void Start()
    {
        ppwz = ppwzCamera.GetComponent<PerfectPixelWithZoom>();
    }

    void Update()
    {
        if (GameControls.gamePlayActions.cameraZoomAxis > 0)
            ppwz.ZoomIn();
        else if (GameControls.gamePlayActions.cameraZoomAxis < 0)
            ppwz.ZoomOut();

        /*if (Input.mouseScrollDelta.y != 0)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                ppwz.ZoomIn();
            }
            else
            {
                ppwz.ZoomOut();
            }
        }*/
    }
}
