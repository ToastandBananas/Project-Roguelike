using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.EventSystems;

public class PixelPerfectZoom : MonoBehaviour
{
    [SerializeField] float pixelsPerUnit = 16;

    //[SerializeField] // Uncomment to watch scaling in the editor
    float pixelsPerUnitScale = 1;

    [SerializeField] float zoomScaleMax = 6f;
    [SerializeField] float zoomScaleStart = 1f;
    [SerializeField] bool smoothZoom = true;
    [SerializeField] float smoothZoomDuration = 0.5f; // In seconds

    int screenHeight;

    float cameraSize;
    Camera cameraComponent;
    PixelPerfectCamera pixelPerfectCamera;
    GameManager gm;

    float zoomStartTime = 0f;
    float zoomScaleMin = 2f;
    float zoomCurrentValue = 1f;
    float zoomNextValue = 1f;
    float zoomInterpolation = 1f;

    public float currentZoomScale { get { return pixelsPerUnitScale; } }

    void Start()
    {
        screenHeight = Screen.height;
        cameraComponent = GetComponent<Camera>();
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        gm = GameManager.instance;
        cameraComponent.orthographic = true;
        SetZoomImmediate(zoomScaleStart);
    }

    void Update()
    {
        if (screenHeight != Screen.height)
        {
            screenHeight = Screen.height;
            UpdateCameraScale();
        }

        if (midZoom)
        {
            if (smoothZoom)
                zoomInterpolation = (Time.time - zoomStartTime) / smoothZoomDuration;
            else
                zoomInterpolation = 1f; // express to the end

            pixelsPerUnitScale = Mathf.Lerp(zoomCurrentValue, zoomNextValue, zoomInterpolation);
            UpdateCameraScale();
        }

        if (EventSystem.current.IsPointerOverGameObject() == false)
        {
            if (GameControls.gamePlayActions.cameraZoomAxis > 0)
                ZoomIn();
            else if (GameControls.gamePlayActions.cameraZoomAxis < 0)
                ZoomOut();
        }
    }

    private void UpdateCameraScale()
    {
        // The magic formular from teh Unity Docs
        // cameraSize = (screenHeight / (pixelsPerUnitScale * pixelsPerUnit)) * 0.5f;
        // cameraComponent.orthographicSize = cameraSize;

        if (zoomNextValue == 2)
            pixelPerfectCamera.refResolutionX = 960;
        else if (zoomNextValue == 3)
            pixelPerfectCamera.refResolutionX = 640;
        else if (zoomNextValue == 4)
            pixelPerfectCamera.refResolutionX = 480;
        else if (zoomNextValue == 5)
            pixelPerfectCamera.refResolutionX = 384;
        else if (zoomNextValue == 6)
            pixelPerfectCamera.refResolutionX = 320;
    }

    private bool midZoom { get { return zoomInterpolation < 1; } }

    private void SetUpSmoothZoom()
    {
        zoomStartTime = Time.time;
        zoomCurrentValue = pixelsPerUnitScale;
        zoomInterpolation = 0f;
    }

    public void SetPixelsPerUnit(int pixelsPerUnitValue)
    {
        pixelsPerUnit = pixelsPerUnitValue;
        UpdateCameraScale();
    }

    // Has to be >= zoomScaleMin
    public void SetZoomScaleMax(int zoomScaleMaxValue)
    {
        zoomScaleMax = Mathf.Max(zoomScaleMaxValue, zoomScaleMin);
    }

    public void SetSmoothZoomDuration(float smoovZoomDurationValue)
    {
        smoothZoomDuration = Mathf.Max(smoovZoomDurationValue, 0.0333f); // 1/30th of a second sounds small enough
    }

    // Clamped to the range [1, zoomScaleMax], Integer values will be pixel-perfect
    public void SetZoom(float scale)
    {
        SetUpSmoothZoom();
        zoomNextValue = Mathf.Max(Mathf.Min(scale, zoomScaleMax), zoomScaleMin);
    }

    // Clamped to the range [1, zoomScaleMax], Integer values will be pixel-perfect
    public void SetZoomImmediate(float scale)
    {
        pixelsPerUnitScale = Mathf.Max(Mathf.Min(scale, zoomScaleMax), zoomScaleMin);
        UpdateCameraScale();
    }

    public void ZoomIn()
    {
        if (!midZoom)
        {
            SetUpSmoothZoom();
            zoomNextValue = Mathf.Min(pixelsPerUnitScale + 1, zoomScaleMax);
        }
    }

    public void ZoomOut()
    {
        SetUpSmoothZoom();
        zoomNextValue = Mathf.Max(pixelsPerUnitScale - 1, zoomScaleMin);
    }
}
