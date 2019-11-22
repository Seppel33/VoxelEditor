using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public GameObject voxelObject;
    public Light directionalLight;

    public float touchSenitivity = 10;

    private float pitch;
    private float yaw;

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
            if (SceneController.activeTouchControl)
            {
                MobileControls(false);
            }
            else
            {
                MobileControls(true);
            }
            //WindowsControls();

        }

    }
    
    private void MobileControls(bool windows)
    {
        if (windows)
        {
            if (Input.GetMouseButton(1))
            {
                Vector3 deltaPos = Input.mousePosition - oldMousePos;
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
            if ((Input.mouseScrollDelta.y < 0 && transform.localPosition.z > -100) || (Input.mouseScrollDelta.y > 0 && (transform.localPosition.z < -5)))
            {
                transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + Input.mouseScrollDelta.y);
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
                int buggyTouchCount = 0;
                if (Application.isEditor && SceneController.buggyTouchCountInEdit)
                {
                    buggyTouchCount = 1;
                }

                if (Input.touchCount > buggyTouchCount)
                {
                    if (Input.touchCount == buggyTouchCount + 2)
                    {
                        Touch touch1 = Input.GetTouch(buggyTouchCount);
                        Touch touch2 = Input.GetTouch(buggyTouchCount + 1);
                        //Debug.Log("Touch1 Pos: " + touch1.position + " | Touch2 Pos: " + touch2.position);
                        Vector2 deltaPos = (touch1.deltaPosition + touch2.deltaPosition) / 2;
                        pitch = -deltaPos.y * touchSenitivity;
                        yaw = deltaPos.x * touchSenitivity;

                        transform.parent.transform.Rotate(0, yaw, 0, Space.World);
                        transform.parent.transform.Rotate(pitch, 0, 0, Space.Self);
                        directionalLight.transform.Rotate(0, yaw, 0, Space.World);
                        directionalLight.transform.Rotate(pitch, 0, 0, Space.Self);
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
