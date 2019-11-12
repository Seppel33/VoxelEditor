using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public float cameraSensitivity = 40f;
    public Camera Camera;
    public Light directionalLight;

    public float moveSpeed;
    public float mouseSensitivity;
    public bool invertMouse;
    public bool autoLockCursor = false;

    private float mouseY;
    private float mouseX;

    // Update is called once per frame
    private void Start()
    {
        Camera.transform.LookAt(transform.position);
    }
    void LateUpdate()
    {
        if (SceneController.getOnIOS())
        {
            IOSControls();
        }
        else
        {
            WindowsControls();
            
        }

    }
    private void WindowsControls()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float speed = moveSpeed;
            Vector3 moveForward = transform.forward;
            moveForward.y = 0;
            moveForward.Normalize();
            Vector3 moveRight = transform.right;
            transform.Translate(moveForward * speed * Input.GetAxis("Vertical"), Space.World);
            transform.Translate(moveRight * speed * Input.GetAxis("Horizontal"), Space.World);
            transform.Translate(Vector3.up * speed * Input.GetAxis("Jump"), Space.World);
            transform.Translate(Vector3.up * speed * Input.GetAxis("Crouch") * -1, Space.World);

            mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity * ((invertMouse) ? 1 : -1);
            mouseX += Input.GetAxis("Mouse X") * mouseSensitivity * ((invertMouse) ? -1 : 1);
            mouseY = Mathf.Clamp(mouseY, -89, 89);

            transform.eulerAngles = new Vector3(mouseY, mouseX, 0f);

        }

        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    private void IOSControls()
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
                if (Input.touchCount == 1)
                {

                }
                else if(Input.touchCount == 2)
                {

                }
            }
            

            /*
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
            */
        }
    }
}
