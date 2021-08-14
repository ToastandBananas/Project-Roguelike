using UnityEngine;

public class Zoom : MonoBehaviour
{
    [SerializeField] Camera ppwzCamera;

    PixelPerfectZoom ppwz;

    void Start()
    {
        ppwz = ppwzCamera.GetComponent<PixelPerfectZoom>();
    }

    void Update()
    {
        if (GameControls.gamePlayActions.cameraZoomAxis > 0)
            ppwz.ZoomIn();
        else if (GameControls.gamePlayActions.cameraZoomAxis < 0)
            ppwz.ZoomOut();
    }
}
