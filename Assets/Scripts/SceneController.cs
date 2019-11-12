using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SceneController : MonoBehaviour
{
    public GameObject groundPlane;
    public GameObject tileSelector;
    public GameObject voxelModel;
    public GameObject voxel;
    
    public bool debugMode = false;
    public UIController UIController;

    private static bool arMode = false;
    private static bool onIOS = false;

    private Vector3 originPointerPosition;

    private bool m_mouseHold = false;
    private bool m_clickBegin = false;
    private int m_clickCountdown = 20;
    private bool m_clicked = false;
    private bool m_holdRelease = false;

    private bool m_rMouseHold = false;
    private bool m_rClickBegin = false;
    private int m_rClickCountdown = 20;
    private bool m_rClicked = false;
    private bool m_rHoldRelease = false;

    private bool m_firstClickFrame = false;
    private Renderer rend;

    public int maxUndo = 30;
    public static List<ArrayList> m_lastActions = new List<ArrayList>();
    public static List<ArrayList> m_lastUndos = new List<ArrayList>();

    
    public List<ArrayList> m_lastActionsTest = new List<ArrayList>();

    public List<ArrayList> m_lastlastUndosTest = new List<ArrayList>();

    // Start is called before the first frame update
    void Start()
    {
        string operatingSystem = SystemInfo.operatingSystem;
        if (operatingSystem[0] == 'i')
        {
            onIOS = true;
            UIController.SetupIOSUI();
            Application.targetFrameRate = 30;
        }
        else
        {
            onIOS = false;
            Application.targetFrameRate = 60;
        }
        Debug.Log(operatingSystem);
        UIController.SetDebugMode(debugMode);
        if (debugMode)
        {
            m_lastActionsTest = m_lastActions;
            m_lastlastUndosTest = m_lastUndos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        if (!EventSystem.current.IsPointerOverGameObject() && Cursor.lockState == CursorLockMode.Locked)
        {
            mouseManager();
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {

                Vector3 endPosition = hit.point + hit.normal / 2;

                endPosition.x = (float)System.Math.Round(endPosition.x, System.MidpointRounding.AwayFromZero);
                endPosition.y = (float)System.Math.Round(endPosition.y, System.MidpointRounding.AwayFromZero);
                endPosition.z = (float)System.Math.Round(endPosition.z, System.MidpointRounding.AwayFromZero);

                if (!m_holdRelease)
                {
                    tileSelector.transform.position = endPosition;
                }

                tileSelector.transform.GetChild(0).gameObject.transform.up = hit.normal;
                tileSelector.SetActive(true);

                if (m_clickBegin && m_firstClickFrame)
                {
                    m_firstClickFrame = false;
                    originPointerPosition = endPosition;
                }
                if (endPosition != originPointerPosition && m_clickBegin)
                {
                    m_mouseHold = true;
                }

                if (m_clicked)
                {
                    GameObject cube = Instantiate(voxel, endPosition, voxelModel.transform.rotation);
                    cube.transform.parent = voxelModel.transform;

                    ArrayList action = new ArrayList();
                    action.Add(0); //0 -> placed object
                    action.Add(cube); //v -> gameobject
                    m_lastActions.Add(action);
                    if (m_lastActions.Count > 10)
                    {
                        m_lastActions.RemoveAt(0);
                    }
                }
                else if (m_mouseHold)
                {
                    Vector3 difference = endPosition - originPointerPosition;
                    tileSelector.transform.localScale = new Vector3(Mathf.Abs(difference.x) + 1, Mathf.Abs(difference.y) + 1, Mathf.Abs(difference.z) + 1);
                    tileSelector.transform.position = originPointerPosition + difference / 2;

                    int children = tileSelector.transform.GetChild(0).transform.childCount;
                    for (int i = 0; i < children; ++i)
                    {
                        rend = tileSelector.transform.GetChild(0).transform.GetChild(i).GetComponent<Renderer>();
                        rend.material.shader = Shader.Find("Shader Graphs/FieldSelect");
                        rend.material.SetFloat("Vector1_D6E874BF", (tileSelector.transform.GetChild(0).transform.localScale.y * 0.8f));
                    }

                }
                else if (m_holdRelease)
                {

                    Vector2[] bounds = new Vector2[3];
                    if(endPosition.x < originPointerPosition.x)
                    {
                        bounds[0] = new Vector2(endPosition.x, originPointerPosition.x);
                    }
                    else
                    {
                        bounds[0] = new Vector2(originPointerPosition.x, endPosition.x);
                    }

                    if (endPosition.y < originPointerPosition.y)
                    {
                        bounds[1] = new Vector2(endPosition.y, originPointerPosition.y);
                    }
                    else
                    {
                        bounds[1] = new Vector2(originPointerPosition.y, endPosition.y);
                    }

                    if (endPosition.z < originPointerPosition.z)
                    {
                        bounds[2] = new Vector2(endPosition.z, originPointerPosition.z);
                    }
                    else
                    {
                        bounds[2] = new Vector2(originPointerPosition.z, endPosition.z);
                    }

                    GameObject group = new GameObject();
                    group.transform.position = new Vector3(0, 0, 0);
                    group.transform.parent = voxelModel.transform;
                    group.name = "goupedCubes";

                    for(int i = (int)bounds[0].x; i <= bounds[0].y; i++)
                    {
                        for (int w = (int)bounds[1].x; w <= bounds[1].y; w++)
                        {
                            for (int t = (int)bounds[2].x; t <= bounds[2].y; t++)
                            {

                                Debug.Log("endPosition " + endPosition + " originPointerPosition" + originPointerPosition + " Durchlaufposition: (" + i + "," + w + "," + t +")");
                                GameObject cube = Instantiate(voxel, new Vector3(i,w,t), voxelModel.transform.rotation);

                                cube.transform.parent = group.transform;

                            }
                        }
                    }
                    group.transform.parent = voxelModel.transform;

                    tileSelector.transform.localScale = new Vector3(1, 1, 1);
                    tileSelector.transform.position = endPosition;

                    ArrayList action = new ArrayList();
                    action.Add(0); //0 -> placed object
                    action.Add(group); //group -> group of gameobjects

                    m_lastActions.Add(action);
                    if (m_lastActions.Count > 10)
                    {
                        m_lastActions.RemoveAt(0);
                    }

                    int children = tileSelector.transform.GetChild(0).transform.childCount;
                    for (int i = 0; i < children; ++i)
                    {
                        rend = tileSelector.transform.GetChild(0).transform.GetChild(i).GetComponent<Renderer>();
                        rend.material.shader = Shader.Find("Shader Graphs/FieldSelect");
                        rend.material.SetFloat("Vector1_D6E874BF", 0.8f);
                    }
                }else if (m_rClicked)
                {
                    Debug.Log("HitObj: " + hit.transform.gameObject.name + " Plane: " + groundPlane.name);
                    if (string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
                    {
                        DeleteObj(hit.transform.gameObject);
                    }
                }

            }
            else
            {
                tileSelector.SetActive(false);
            }
        }
        else
        {
            tileSelector.SetActive(false);
        }
        


    }
    public static bool getArMode()
    {
        return arMode;
    }
    public static bool getOnIOS()
    {
        return onIOS;
    }
    private void mouseManager()
    {
        if (Input.GetMouseButton(0) && !m_clickBegin)
        {
            //Debug.Log("ClickBegin");
            m_clickBegin = true;
            m_firstClickFrame = true;
        }
        else if (Input.GetMouseButton(0) && m_clickBegin)
        {
            if (!m_mouseHold)
            {
                //Debug.Log("HoldTest");
                m_clickCountdown--;
                if (m_clickCountdown == 0)
                {
                    m_clickCountdown = 20;
                    m_mouseHold = true;
                    //Debug.Log("HoldTest Done");
                }
            }
        }
        else if (!Input.GetMouseButton(0) && !m_mouseHold && m_clickBegin)
        {
            //Debug.Log("Clicked");
            m_clicked = true;
            m_clickBegin = false;
            m_clickCountdown = 20;
        }
        else if (!Input.GetMouseButton(0) && m_mouseHold && m_clickBegin)
        {
            //Debug.Log("HoldRelease");
            m_holdRelease = true;
            m_clickBegin = false;
            m_mouseHold = false;
            m_clickCountdown = 20;
        }
        else
        {
            m_clicked = false;
            m_holdRelease = false;
        }


        if (Input.GetMouseButton(1) && !m_rClickBegin)
        {
            //Debug.Log("ClickBeginR");
            m_rClickBegin = true;
        }
        else if (Input.GetMouseButton(1) && m_rClickBegin)
        {
            if (!m_rMouseHold)
            {
                //Debug.Log("HoldTestR");
                m_rClickCountdown--;
                if (m_rClickCountdown == 0)
                {
                    m_rClickCountdown = 20;
                    m_rMouseHold = true;
                    //Debug.Log("HoldTest Done R");
                }
            }
        }
        else if (!Input.GetMouseButton(1) && !m_rMouseHold && m_rClickBegin)
        {
            //Debug.Log("ClickedR");
            m_rClicked = true;
            m_rClickBegin = false;
            m_rClickCountdown = 20;
        }
        else if (!Input.GetMouseButton(1) && m_rMouseHold && m_rClickBegin)
        {
            //Debug.Log("HoldReleaseR");
            m_rHoldRelease = true;
            m_rClickBegin = false;
            m_rMouseHold = false;
            m_rClickCountdown = 20;
        }
        else
        {
            m_rClicked = false;
            m_rHoldRelease = false;
        }
    }
    public void DeleteObj(GameObject g)
    {
        ArrayList action = new ArrayList();
        action.Add(1); //1 -> deleted object
        action.Add(g); //v -> gameobject

        g.SetActive(false);
        m_lastActions.Add(action);
        if (m_lastActions.Count > 10)
        {
            m_lastActions.RemoveAt(0);
        }
    }

}
