using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class CameraController : MonoBehaviour
{

    public GameObject VoxelObject;
    public Light DirectionalLight;
    public ScreenTransformGesture RotateGesture;
    public ScreenTransformGesture ZoomGesture;
    public UIController UIController;

    public float touchSenitivity = 0.5f;

    private float pitch;
    private float yaw;

    private Vector3 oldMousePos;

    private void Start()
    {
        //transform.LookAt(transform.position);
    }

    private void OnEnable()
    {
        RotateGesture.Transformed += rotateTransformedHandler;
        ZoomGesture.Transformed += zoomTransformedHandler;
    }
    private void OnDisable()
    {
        RotateGesture.Transformed -= rotateTransformedHandler;
        ZoomGesture.Transformed -= zoomTransformedHandler;
    }

    void LateUpdate()
    {
        if (!SceneController.activeTouchControl)
        {
            MouseControls();
        }
    }

    private void rotateTransformedHandler(object sender, System.EventArgs e)
    {
        if (!UIController.getActiveMenu())
        {
            transform.parent.transform.Rotate(0, RotateGesture.DeltaPosition.x * touchSenitivity, 0, Space.World);
            transform.parent.transform.Rotate(-RotateGesture.DeltaPosition.y * touchSenitivity, 0, 0, Space.Self);
            DirectionalLight.transform.Rotate(0, RotateGesture.DeltaPosition.x * touchSenitivity, 0, Space.World);
            DirectionalLight.transform.Rotate(-RotateGesture.DeltaPosition.y * touchSenitivity, 0, 0, Space.Self);
        }
    }

    private void zoomTransformedHandler(object sender, System.EventArgs e)
    {
        if (!UIController.getActiveMenu())
        {
            if (((ZoomGesture.DeltaScale - 1f) < 0 && transform.localPosition.z > -100) || ((ZoomGesture.DeltaScale - 1f) > 0 && (transform.localPosition.z < -5)))
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + (ZoomGesture.DeltaScale - 1f) * touchSenitivity * 20);
            }
        }
    }

    private void MouseControls()
    {
        if (!UIController.getActiveMenu())
        {
            if (Input.GetMouseButton(1))
            {
                Vector3 deltaPos = Input.mousePosition - oldMousePos;
                pitch = -deltaPos.y * touchSenitivity;
                yaw = deltaPos.x * touchSenitivity;

                transform.parent.transform.Rotate(0, yaw, 0, Space.World);
                transform.parent.transform.Rotate(pitch, 0, 0, Space.Self);
                DirectionalLight.transform.Rotate(0, yaw, 0, Space.World);
                DirectionalLight.transform.Rotate(pitch, 0, 0, Space.Self);

                oldMousePos = Input.mousePosition;
            }
            else
            {
                oldMousePos = Input.mousePosition;
            }
            if ((Input.mouseScrollDelta.y < 0 && transform.localPosition.z > -100) || (Input.mouseScrollDelta.y > 0 && (transform.localPosition.z < -5)))
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + Input.mouseScrollDelta.y);
            }
        }
    }

}
