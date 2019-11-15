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
    public GameObject deleteSelector;
    public GameObject voxelModel;
    public GameObject voxel;

    public bool debugMode = false;
    public UIController UIController;

    private static bool arMode = false;
    private static bool vrMode = false;
    private static bool onMobile = false;

    private Vector3 originPointerPosition;

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

    public void DeleteObj(ArrayList cubes)
    {
        ArrayList action = new ArrayList();
        action.Add(1); //1 -> deleted object
        action.Add(cubes); //v -> gameobject

        foreach(GameObject g in cubes)
        {
            gridOfObjects[(int)g.transform.position.x + (int)(dimensions.x / 2), (int)g.transform.position.y, (int)g.transform.position.z + (int)(dimensions.z / 2)] = null;
            g.SetActive(false);
        }
        undoRedoScript.addAction(action);
    }

    public void standardBuildControl()
    {
        deleteSelector.SetActive(false);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {

            Vector3 endPosition = hit.point + hit.normal / 2;

            endPosition.x = (float)System.Math.Round(endPosition.x, System.MidpointRounding.AwayFromZero);
            endPosition.y = (float)System.Math.Round(endPosition.y, System.MidpointRounding.AwayFromZero);
            endPosition.z = (float)System.Math.Round(endPosition.z, System.MidpointRounding.AwayFromZero);
            
            tileSelector.transform.position = endPosition;

            tileSelector.transform.GetChild(0).gameObject.transform.up = hit.normal;
            tileSelector.SetActive(true);

            bool touched = false;
            bool endedTouch = false;
            if(Input.touchCount == 1)
            {
                if(Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    touched = true;
                }else if(Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    endedTouch = true;
                }
            }
            if (Input.GetMouseButtonDown(0) || touched)
            {
                originPointerPosition = endPosition;
            }

            if (Input.GetMouseButton(0) || Input.touchCount == 1)
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
            else if (Input.GetMouseButtonUp(0) || endedTouch)
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
                                    replacedCubes.Add(gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)]);
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
        }
        else
        {
            tileSelector.SetActive(false);
        }
    }
    public void standardDeleteControl()
    {
        tileSelector.SetActive(false);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if(string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
            {
                Vector3 endPosition = hit.point - hit.normal / 2;

                endPosition.x = (float)System.Math.Round(endPosition.x, System.MidpointRounding.AwayFromZero);
                endPosition.y = (float)System.Math.Round(endPosition.y, System.MidpointRounding.AwayFromZero);
                endPosition.z = (float)System.Math.Round(endPosition.z, System.MidpointRounding.AwayFromZero);

                if (endPosition.y < 0)
                {
                    endPosition.y = 0;
                }

                deleteSelector.transform.position = endPosition;
                deleteSelector.transform.GetChild(0).gameObject.transform.up = hit.normal;
                deleteSelector.SetActive(true);
                bool touched = false;
                bool endedTouch = false;
                if (Input.touchCount == 1)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        touched = true;
                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        endedTouch = true;
                    }
                }
                if (Input.GetMouseButtonDown(0) || touched)
                {
                    originPointerPosition = endPosition;
                }

                if (Input.GetMouseButton(0) || Input.touchCount == 1)
                {
                    Vector3 difference = endPosition - originPointerPosition;
                    deleteSelector.transform.localScale = new Vector3(Mathf.Abs(difference.x) + 1, Mathf.Abs(difference.y) + 1, Mathf.Abs(difference.z) + 1);
                    deleteSelector.transform.position = originPointerPosition + difference / 2;

                    /*
                    int children = deleteSelector.transform.GetChild(0).transform.childCount;
                    for (int i = 0; i < children; ++i)
                    {
                        rend = deleteSelector.transform.GetChild(0).transform.GetChild(i).GetComponent<Renderer>();
                        rend.material.shader = Shader.Find("Shader Graphs/FieldSelect");
                        rend.material.SetFloat("Vector1_D6E874BF", (deleteSelector.transform.GetChild(0).transform.localScale.y * 0.8f));
                    }
                    */

                }
                else if (Input.GetMouseButtonUp(0) || endedTouch)
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

                    ArrayList deletedCubes = new ArrayList();

                    for (int i = (int)bounds[0].x; i <= bounds[0].y; i++)
                    {
                        for (int w = (int)bounds[1].x; w <= bounds[1].y; w++)
                        {
                            for (int t = (int)bounds[2].x; t <= bounds[2].y; t++)
                            {

                                if (gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)] != null)
                                {
                                    deletedCubes.Add(gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)]);
                                }
                            }
                        }
                    }
                    if (deletedCubes.Count != 0)
                    {
                        DeleteObj(deletedCubes);
                    }

                    deleteSelector.transform.localScale = new Vector3(1, 1, 1);
                    deleteSelector.transform.position = endPosition;
                    /*
                    int children = deleteSelector.transform.GetChild(0).transform.childCount;
                    for (int i = 0; i < children; ++i)
                    {
                        rend = deleteSelector.transform.GetChild(0).transform.GetChild(i).GetComponent<Renderer>();
                        rend.material.shader = Shader.Find("Shader Graphs/FieldSelect");
                        rend.material.SetFloat("Vector1_D6E874BF", 0.8f);
                    }*/
                }
            }
            else
            {
                deleteSelector.SetActive(false);
            }
        }
            
    }
    //standardPaintControl
    //arBuildControl
    //arDeleteControl
    //arPaintControl
    //vrBuildControl
    //vrDeleteControl
    //vrPaintControl
}
