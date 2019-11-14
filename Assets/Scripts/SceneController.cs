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
    private static bool vrMode = false;
    private static bool onMobile = false;

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
    [SerializeField]
    private Vector3Int dimension = new Vector3Int(21, 21, 21); //for inspector
    public static Vector3Int dimensions;
    public static GameObject[,,] gridOfObjects;

    public UndoRedo undoRedoScript;

    // Start is called before the first frame update
    void Start()
    {
        string operatingSystem = SystemInfo.operatingSystem;
        if (operatingSystem[0] == 'i' || operatingSystem[0] == 'A')
        {
            onMobile = true;
            Application.targetFrameRate = 30;
        }
        else if (operatingSystem[0] == 'W')
        {
            Application.targetFrameRate = 60;
        }
        Debug.Log(operatingSystem);
        UIController.SetDebugMode(debugMode);
        dimensions = dimension;
        gridOfObjects = new GameObject[dimensions.x, dimensions.y, dimensions.z];
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (vrMode)
            {

            }
            else if (arMode)
            {

            }
            else
            {
                switch (UIController.selectedState)
                {
                    case 0:
                        standardBuildControl();
                        break;
                    case 1:
                        standardDeleteControl();
                        break;
                }
                
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
    public static bool getOnMobile()
    {
        return onMobile;
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
    public void DeleteObj(GameObject g, Vector3Int position)
    {
        ArrayList action = new ArrayList();
        ArrayList cubes = new ArrayList();
        cubes.Add(g);
        action.Add(1); //1 -> deleted object
        action.Add(cubes); //v -> gameobject

        gridOfObjects[position.x + (int)(dimensions.x / 2), position.y, position.z + (int)(dimensions.z / 2)] = null;
        g.SetActive(false);
        undoRedoScript.addAction(action);
    }

    public void standardBuildControl()
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
                if (((int)endPosition.x + (int)(dimensions.x / 2) >= 0 && (int)endPosition.x + (int)(dimensions.x / 2) < dimensions.x) && ((int)endPosition.y >= 0 && (int)endPosition.y < dimensions.y)
                    && ((int)endPosition.z + (int)(dimensions.z / 2) >= 0 && (int)endPosition.z + (int)(dimensions.z / 2) < dimensions.z))
                {
                    GameObject cube = Instantiate(voxel, endPosition, voxelModel.transform.rotation);
                    cube.transform.parent = voxelModel.transform;
                    ArrayList cubes = new ArrayList();
                    cubes.Add(cube);

                    ArrayList action = new ArrayList();
                    action.Add(0); //0 -> placed object
                    action.Add(cubes); //v -> arraylist of gameobject

                    gridOfObjects[(int)endPosition.x + (int)(dimensions.x / 2), (int)endPosition.y, (int)endPosition.z + (int)(dimensions.z / 2)] = cube;

                    undoRedoScript.addAction(action);
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
                if (((int)endPosition.x + (int)(dimensions.x / 2) >= 0 && (int)endPosition.x + (int)(dimensions.x / 2) < dimensions.x) && ((int)endPosition.y >= 0 && (int)endPosition.y < dimensions.y)
                    && ((int)endPosition.z + (int)(dimensions.z / 2) >= 0 && (int)endPosition.z + (int)(dimensions.z / 2) < dimensions.z) && ((int)originPointerPosition.x + (int)(dimensions.x / 2) >= 0 && (int)originPointerPosition.x + (int)(dimensions.x / 2) < dimensions.x) && ((int)originPointerPosition.y >= 0 && (int)originPointerPosition.y < dimensions.y)
                    && ((int)originPointerPosition.z + (int)(dimensions.z / 2) >= 0 && (int)endPosition.z + (int)(dimensions.z / 2) < dimensions.z))
                {
                    //start- & end-position of selection
                    Vector2[] bounds = new Vector2[3];
                    if (endPosition.x < originPointerPosition.x)
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

                    ArrayList replacedCubes = new ArrayList();
                    ArrayList placedCubes = new ArrayList();

                    for (int i = (int)bounds[0].x; i <= bounds[0].y; i++)
                    {
                        for (int w = (int)bounds[1].x; w <= bounds[1].y; w++)
                        {
                            for (int t = (int)bounds[2].x; t <= bounds[2].y; t++)
                            {
                                GameObject cube = Instantiate(voxel, new Vector3(i, w, t), voxelModel.transform.rotation);
                                cube.transform.parent = voxelModel.transform;

                                if (gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)] != null)
                                {
                                    replacedCubes.Add(gridOfObjects[i + (int)(dimensions.x / 2), w + (int)(dimensions.y / 2), t + (int)(dimensions.z / 2)]);
                                    gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)].gameObject.SetActive(false);
                                }
                                gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)] = cube;
                                placedCubes.Add(cube);

                            }
                        }
                    }
                    ArrayList action = new ArrayList();
                    if (replacedCubes.Count == 0)
                    {
                        action.Add(0); //0 -> placed object
                        action.Add(placedCubes); //group -> group of gameobjects
                    }
                    else
                    {
                        action.Add(2); //2 -> replaced object
                        action.Add(placedCubes); //placedCubes -> group of placed gameobjects
                        action.Add(replacedCubes); //replacedCubes -> group of replaced gameobjects
                    }

                    undoRedoScript.addAction(action);

                }
                tileSelector.transform.localScale = new Vector3(1, 1, 1);
                tileSelector.transform.position = endPosition;
                int children = tileSelector.transform.GetChild(0).transform.childCount;
                for (int i = 0; i < children; ++i)
                {
                    rend = tileSelector.transform.GetChild(0).transform.GetChild(i).GetComponent<Renderer>();
                    rend.material.shader = Shader.Find("Shader Graphs/FieldSelect");
                    rend.material.SetFloat("Vector1_D6E874BF", 0.8f);
                }
            }
            else if (m_rClicked)
            {
                if (string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
                {
                    Vector3Int v = new Vector3Int((int)(endPosition.x - hit.normal.x), (int)(endPosition.y - hit.normal.y), (int)(endPosition.z - hit.normal.z));
                    DeleteObj(hit.transform.gameObject, v);
                }
            }

        }
        else
        {
            tileSelector.SetActive(false);
        }
    }
    public void standardDeleteControl()
    {

    }
    //standardPaintControl
    //arBuildControl
    //arDeleteControl
    //arPaintControl
    //vrBuildControl
    //vrDeleteControl
    //vrPaintControl
}
