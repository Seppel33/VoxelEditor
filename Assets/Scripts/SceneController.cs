﻿using System.Collections;
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
    public UIController UIController;
    public UndoRedo undoRedoScript;

    private static bool arMode = false;
    private static bool vrMode = false;
    private static bool onMobile = false;

    private Vector3 originPointerPosition;

    private Renderer rend;
    [SerializeField]
    private Vector3Int dimension = new Vector3Int(21, 21, 21); //for inspector
    public static Vector3Int dimensions;
    public static GameObject[,,] gridOfObjects;

    private bool moveGesture = false;

    [Header("Debug/Temp Settings")]
    public bool debugMode = true;
    public bool buggyTouchCountInEditor = false; //for inspector
    public static bool buggyTouchCountInEdit;
    public bool activeTouchControls = false;
    public static bool activeTouchControl;
    private int touchCountEditorFix = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isEditor && buggyTouchCountInEditor)
        {
            touchCountEditorFix++;
        }
        string operatingSystem = SystemInfo.operatingSystem;
        if (operatingSystem[0] == 'i' || operatingSystem[0] == 'A')
        {
            onMobile = true;
        }
        Debug.Log(operatingSystem);
        Debug.Log(SystemInfo.deviceModel);
        UIController.SetDebugMode(debugMode);
        buggyTouchCountInEdit = buggyTouchCountInEditor;
        activeTouchControl = activeTouchControls;
        dimensions = dimension;
        gridOfObjects = new GameObject[dimensions.x, dimensions.y, dimensions.z];
    }

    // Update is called once per frame
    void Update()
    {
        if (vrMode)
        {

        }
        else if (arMode)
        {

        }
        else
        {
            if (activeTouchControl)
            {
                if (Input.touchCount > 1)
                {
                    moveGesture = true;

                }
                else if (Input.touchCount == 0)
                {
                    moveGesture = false;
                }
                switch (UIController.selectedState)
                {
                    case 0:
                        touchBuildControl();
                        break;
                    case 1:
                        touchDeleteControl();
                        break;
                    case 2:
                        touchPaintControl();
                        break;
                }
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
                    case 2:
                        standardPaintControl();
                        break;
                }
            }

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

        foreach (GameObject g in cubes)
        {
            gridOfObjects[(int)g.transform.position.x + (int)(dimensions.x / 2), (int)g.transform.position.y, (int)g.transform.position.z + (int)(dimensions.z / 2)] = null;
            g.SetActive(false);
        }
        undoRedoScript.addAction(action);
    }
    public void touchBuildControl()
    {
        deleteSelector.SetActive(false);
        if ((Input.touchCount < touchCountEditorFix + 2 && Input.touchCount > touchCountEditorFix) && !moveGesture)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(touchCountEditorFix).position);
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

                if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Began)
                {
                    touched = true;
                }
                else if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Ended)
                {
                    endedTouch = true;
                }
                if (touched)
                {
                    originPointerPosition = endPosition;
                }

                if (Input.touchCount == touchCountEditorFix + 1 && !endedTouch)
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
                else if (endedTouch)
                {
                    buildSelected(endPosition);
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
    private void standardBuildControl()
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

            if (Input.GetMouseButtonDown(0))
            {
                originPointerPosition = endPosition;
            }

            if (Input.GetMouseButton(0))
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
            else if (Input.GetMouseButtonUp(0))
            {
                buildSelected(endPosition);
            }
        }
        else
        {
            tileSelector.SetActive(false);
        }
    }
    private void buildSelected(Vector3 endPosition)
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

                        rend = cube.transform.GetComponent<Renderer>();
                        rend.material.shader = Shader.Find("Shader Graphs/Blocks");
                        Color c = rend.material.GetColor("Color_E5F6C120");
                        rend.material.SetColor("Color_E5F6C120", UIController.selectedColor);

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
    private void standardDeleteControl()
    {
        tileSelector.SetActive(false);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
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

                if (Input.GetMouseButtonDown(0))
                {
                    originPointerPosition = endPosition;
                }

                if (Input.GetMouseButton(0))
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
                else if (Input.GetMouseButtonUp(0))
                {
                    deleteSelected(endPosition);
                }
            }
            else
            {
                deleteSelector.SetActive(false);
            }
        }

    }
    private void touchDeleteControl()
    {
        tileSelector.SetActive(false);
        if ((Input.touchCount < touchCountEditorFix + 2 && Input.touchCount > touchCountEditorFix) && !moveGesture)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(touchCountEditorFix).position);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
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
                    if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Began)
                    {
                        touched = true;
                    }
                    else if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Ended)
                    {
                        endedTouch = true;
                    }
                    if (touched)
                    {
                        originPointerPosition = endPosition;
                    }

                    if (Input.touchCount == touchCountEditorFix + 1 && !endedTouch)
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
                    else if (endedTouch)
                    {
                        deleteSelected(endPosition);
                    }
                }
                else
                {
                    deleteSelector.SetActive(false);
                }
            }
            else
            {
                deleteSelector.SetActive(false);
            }
        }
        else
        {
            deleteSelector.SetActive(false);
        }
    }

    private void deleteSelected(Vector3 endPosition)
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
    public void standardPaintControl()
    {
        tileSelector.SetActive(false);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
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

                if (Input.GetMouseButtonDown(0))
                {
                    originPointerPosition = endPosition;
                }

                if (Input.GetMouseButton(0))
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
                else if (Input.GetMouseButtonUp(0))
                {
                    paintSelected(endPosition);
                }
            }
            else
            {
                deleteSelector.SetActive(false);
            }
        }
    }
    public void touchPaintControl()
    {
        tileSelector.SetActive(false);
        if ((Input.touchCount < touchCountEditorFix + 2 && Input.touchCount > touchCountEditorFix) && !moveGesture)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(touchCountEditorFix).position);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (string.CompareOrdinal(hit.transform.gameObject.name, groundPlane.name) != 0)
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
                    if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Began)
                    {
                        touched = true;
                    }
                    else if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Ended)
                    {
                        endedTouch = true;
                    }
                    if (touched)
                    {
                        originPointerPosition = endPosition;
                    }

                    if (Input.touchCount == touchCountEditorFix + 1 && !endedTouch)
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
                    else if (endedTouch)
                    {
                        paintSelected(endPosition);
                    }
                }
                else
                {
                    deleteSelector.SetActive(false);
                }
            }
            else
            {
                deleteSelector.SetActive(false);
            }
        }
        else
        {
            deleteSelector.SetActive(false);
        }
    }
    private void paintSelected(Vector3 endPosition)
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

        ArrayList paintedCubes = new ArrayList();
        ArrayList oldColors = new ArrayList();

        for (int i = (int)bounds[0].x; i <= bounds[0].y; i++)
        {
            for (int w = (int)bounds[1].x; w <= bounds[1].y; w++)
            {
                for (int t = (int)bounds[2].x; t <= bounds[2].y; t++)
                {

                    if (gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)] != null)
                    {
                        paintedCubes.Add(gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)]);
                        rend = gridOfObjects[i + (int)(dimensions.x / 2), w, t + (int)(dimensions.z / 2)].transform.GetComponent<Renderer>();
                        rend.material.shader = Shader.Find("Shader Graphs/Blocks");
                        oldColors.Add(rend.material.GetColor("Color_E5F6C120"));
                        rend.material.SetColor("Color_E5F6C120", UIController.selectedColor);
                    }
                }
            }
        }

        ArrayList action = new ArrayList();
        action.Add(3); //3 -> painted Object
        action.Add(paintedCubes); //selected Cubes
        action.Add(UIController.selectedColor); //new Color
        action.Add(oldColors);

        undoRedoScript.addAction(action);

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
    //arBuildControl
    //arDeleteControl
    //arPaintControl
    //vrBuildControl
    //vrDeleteControl
    //vrPaintControl
}
