using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public float cameraSensitivity = 40f;
    public GameObject voxelObject;
    public Light directionalLight;

    public float moveSpeed;
    public float mouseSensitivity;
    public bool invertMouse;
    public bool autoLockCursor = false;
    public float touchSenitivity = 10;

    private float pitch;
    private float yaw;
    private bool inSwipeAnimation = false;

    private Vector3 oldMousePos;

    // Update is called once per frame
    private void Start()
    {
        //transform.LookAt(transform.position);
    }
    void LateUpdate()
    {
        if (SceneController.getOnMobile())
        {
            MobileControls(false);
        }
        else
        {

            MobileControls(true);
            //WindowsControls();
            
        }

    }
    private void WindowsControls()
    {
        //if (Cursor.lockState == CursorLockMode.Locked)
        //{
            float speed = moveSpeed;
            Vector3 moveForward = transform.forward;
            moveForward.y = 0;
            moveForward.Normalize();
            Vector3 moveRight = transform.right;
            transform.Translate(moveForward * speed * Input.GetAxis("Vertical"), Space.World);
            transform.Translate(moveRight * speed * Input.GetAxis("Horizontal"), Space.World);
            transform.Translate(Vector3.up * speed * Input.GetAxis("Jump"), Space.World);
            transform.Translate(Vector3.up * speed * Input.GetAxis("Crouch") * -1, Space.World);

            pitch += Input.GetAxis("Mouse Y") * mouseSensitivity * ((invertMouse) ? 1 : -1);
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity * ((invertMouse) ? -1 : 1);
            pitch = Mathf.Clamp(pitch, -89, 89);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        //}

        /*
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }*/
    }
    private void MobileControls(bool windows)
    {
        if (windows)
        {
            if (Input.GetMouseButton(1))
            {
                Vector3 deltaPos = Input.mousePosition - oldMousePos;
                if (Mathf.Abs(deltaPos.y) > Mathf.Abs(deltaPos.x))
                {
                    if (Mathf.Abs(deltaPos.x) <= 2 && Mathf.Abs(deltaPos.y) > 2)
                    {
                        deltaPos.x = 0;
                    }
                }
                else
                {
                    if (Mathf.Abs(deltaPos.y) <= 2 && Mathf.Abs(deltaPos.x) > 2)
                    {
                        deltaPos.y = 0;
                    }
                }
                pitch = -deltaPos.y * touchSenitivity;
                yaw = deltaPos.x * touchSenitivity;

                transform.parent.transform.Rotate(0, yaw, 0, Space.World);
                transform.parent.transform.Rotate(pitch, 0, 0, Space.Self);
                directionalLight.transform.Rotate(0, yaw, 0, Space.World);
                directionalLight.transform.Rotate(pitch, 0, 0, Space.Self);

                oldMousePos = Input.mousePosition;
            }
            else
            {
                oldMousePos = Input.mousePosition;
            }
        }
        else
        {
            if (SceneController.getArMode())
            {
                //rotate object y-Axis |rotate gesture
                //rotate object x-Axis |swipe up/down
            }
            else
            {
                if(Input.touchCount > 0)
                {
                    Debug.Log(Input.GetTouch(0).tapCount);
                    if(Input.touchCount == 2)
                    {
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        Vector2 deltaPos = (touch1.deltaPosition + touch2.deltaPosition) / 2;
                        pitch = deltaPos.y * touchSenitivity;
                        yaw = deltaPos.x * touchSenitivity;
                        voxelObject.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
                    }
                    else
                    {

                    }
                }
            }

            /* Options
             * 1.
             * rotate y-Axis |rotate gesture
             * rotate x-Axis |two finger swipe up/down
             * zoom,move closer/away  |pinch gesture
             * 2.
             * switch to move mode
             * rotate x- and y-Axis |One Finger swipe
             * zoom,move closer/away  |pinch gesture
             * 3.
             * rotate x- and y-Axis |Joystick
             * zoom,move closer/away  |pinch gesture
             * 4.
             * rotate x- and y-Axis |two finger swipe
             * zoom,move closer/away  |pinch gesture
            */
        }
    }
}
