using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    public GameObject borderCollider;


    private static bool arMode = false;
    private static bool vrMode = false;
    private static bool onMobile = false;
    public static bool standalone = true;

    private Vector3 originPointerPosition;
    private Vector3 correctedOriginPointerPosition;
    private int[] startPosData = new int[3];
    private bool touched;
    private bool possible;
    private Vector3 oldMousePosition = new Vector3();
    public bool mouseMoved = false;

    private Renderer rend;
    [SerializeField]
    private Vector3Int dimension = new Vector3Int(15, 15, 15); //for inspector
    public static Vector3Int dimensions;
    public static GameObject[,,] gridOfObjects;
    public static int actionsQuantity;
    public static float timeTaken;
    public static string lastDebugMessage;

    private bool moveGesture = false;

    [Header("Debug/Temp Settings")]
    public bool debugMode = true;
    public bool buggyTouchCountInEditor = false; //for inspector
    public static bool buggyTouchCountInEdit;
    private int touchCountEditorFix = 0;
    public static Settings settings;
    // Start is called before the first frame update
    void Start()
    {
        Input.simulateMouseWithTouches = false;
        if (Application.isEditor && buggyTouchCountInEditor)
        {
            touchCountEditorFix++;
        }

        Debug.Log("Scenes in Project: " + SceneManager.sceneCountInBuildSettings);
        if (SceneManager.sceneCountInBuildSettings > 1)
        {
            standalone = false;
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
        dimensions = dimension;
        gridOfObjects = new GameObject[dimensions.x, dimensions.y, dimensions.z];
        settings = SaveSystem.LoadSettings();
        if (!settings.startedOnce)
        {
            SaveSystem.CreateExampleData();
            settings.startedOnce = true;
            SaveSystem.SaveSettings(settings);
        }
        oldMousePosition = Input.mousePosition;
        UpdateScene();

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
            if (!UIController.GetActiveColorSelector() && !UIController.GetActiveMenu() && !EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.touchCount > 1)
                {
                    mouseMoved = false;
                    oldMousePosition = Input.mousePosition;
                    moveGesture = true;
                }
                else if (Input.touchCount == 0)
                {
                    moveGesture = false;
                }
                else
                {
                    mouseMoved = false;
                    oldMousePosition = Input.mousePosition;
                }
                if (oldMousePosition != Input.mousePosition)
                {
                    mouseMoved = true;
                }

                switch (UIController.selectedState)
                {
                    case 0:
                        StandardControlOnBlock();
                        break;
                    case 1:
                        StandardControlInBlock(0);
                        break;
                    case 2:
                        StandardControlInBlock(1);
                        break;
                }
                timeTaken += Time.deltaTime;
            }
            else if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || (Input.touchCount == 1 && Input.GetTouch(0).phase != TouchPhase.Ended))
            {
                if (Input.touchCount == 1)//temporary
                {
                    if (!EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                    {
                        if (UIController.GetActiveMenu())
                        {
                            //UIController.toggleMenu();
                        }
                        else
                        {
                            UIController.CloseColorWheel(null);
                        }
                    }
                }
                else if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (UIController.GetActiveMenu())
                    {
                        //UIController.toggleMenu();
                    }
                    else
                    {
                        UIController.CloseColorWheel(null);
                    }
                }
            }

            else if (UIController.GetActiveColorSelector())
            {
                timeTaken += Time.deltaTime;
            }
        }
    }
    public void UpdateScene()
    {
        groundPlane.transform.localScale = new Vector3(dimensions.x / 10f, 1, dimensions.z / 10f);
        borderCollider.transform.localScale = new Vector3(dimensions.x / 10f, dimensions.y / 10f, dimensions.z / 10f);

        rend = groundPlane.transform.GetComponent<Renderer>();
        rend.material.shader = Shader.Find("Shader Graphs/Ground");
        rend.material.SetVector("Vector2_3CDFD44B", new Vector4(dimensions.x, dimensions.z, 0, 0));
    }
    public static bool GetArMode()
    {
        return arMode;
    }
    public static bool GetOnMobile()
    {
        return onMobile;
    }
    private void DeleteObj(ArrayList cubes)
    {
        ArrayList action = new ArrayList();
        action.Add(1); //1 -> deleted object
        action.Add(cubes); //v -> gameobject

        foreach (GameObject g in cubes)
        {
            gridOfObjects[(int)g.transform.position.x + (int)(dimensions.x / 2), (int)g.transform.position.y, (int)g.transform.position.z + (int)(dimensions.z / 2)] = null;
            g.SetActive(false);
        }
        undoRedoScript.AddAction(action);
    }
    public void ClearGrid()
    {
        foreach (Transform child in voxelModel.transform.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
    }
    private Vector2[] CalculateBounds(Vector3 endPosition)
    {
        //start- & end-position of selection
        Vector2[] bounds = new Vector2[3];
        if (endPosition.x < correctedOriginPointerPosition.x)
        {
            bounds[0] = new Vector2(endPosition.x, correctedOriginPointerPosition.x);
        }
        else
        {
            bounds[0] = new Vector2(correctedOriginPointerPosition.x, endPosition.x);
        }

        if (endPosition.y < correctedOriginPointerPosition.y)
        {
            bounds[1] = new Vector2(endPosition.y, correctedOriginPointerPosition.y);
        }
        else
        {
            bounds[1] = new Vector2(correctedOriginPointerPosition.y, endPosition.y);
        }

        if (endPosition.z < correctedOriginPointerPosition.z)
        {
            bounds[2] = new Vector2(endPosition.z, correctedOriginPointerPosition.z);
        }
        else
        {
            bounds[2] = new Vector2(correctedOriginPointerPosition.z, endPosition.z);
        }
        return bounds;
    }
    private Vector3 CorrectedPositions(Vector3 endPosition, bool touched, out int[] endPosData, int state)
    {
        endPosData = new int[3];
        Vector3 output = endPosition;
        switch (state)
        {
            case 0:
                tileSelector.SetActive(false);
                break;
            case 1:
                deleteSelector.SetActive(false);
                break;
            case 2:
                deleteSelector.SetActive(false);
                break;
        }
        int count = 0;
        if (output.x < -dimensions.x / 2)
        {
            output.x = -dimensions.x / 2;
            endPosData[0] = 0;
        }
        else if (output.x > dimensions.x / 2)
        {
            output.x = dimensions.x / 2;
            endPosData[0] = 1;
        }
        else
        {
            endPosData[0] = 2;
            count++;
        }
        if (output.y < 0)
        {
            output.y = 0;
            endPosData[1] = 0;
        }
        else if (output.y >= dimensions.y)
        {
            output.y = dimensions.y - 1;
            endPosData[1] = 1;
        }
        else
        {
            endPosData[1] = 2;
            count++;
        }
        if (output.z < -dimensions.z / 2)
        {
            output.z = -dimensions.z / 2;
            endPosData[2] = 0;
        }
        else if (output.z > dimensions.z / 2)
        {
            output.z = dimensions.z / 2;
            endPosData[2] = 1;
        }
        else
        {
            endPosData[2] = 2;
            count++;
        }
        if (touched && CheckIfPossible(endPosData, endPosition, output))
        {
            switch (state)
            {
                case 0:
                    tileSelector.SetActive(true);
                    break;
                case 1:
                    deleteSelector.SetActive(true);
                    break;
                case 2:
                    deleteSelector.SetActive(true);
                    break;
            }
        }
        else if (count == 3)
        {
            switch (state)
            {
                case 0:
                    tileSelector.SetActive(true);
                    break;
                case 1:
                    deleteSelector.SetActive(true);
                    break;
                case 2:
                    deleteSelector.SetActive(true);
                    break;
            }
        }
        return output;
    }
    private bool CheckIfPossible(int[] endPosData, Vector3 endposition, Vector3 output)
    {
        possible = false;

        if ((endposition == output || originPointerPosition == correctedOriginPointerPosition) || IsOpposite(endPosData))
        {
            possible = true;
        }

        return possible;
    }
    private bool IsOpposite(int[] endPosData)
    {
        bool isOpposite = false;

        if ((endPosData[0] != startPosData[0] && ((endPosData[1] != startPosData[1] || endPosData[2] != startPosData[2]) || (endPosData[1] == 2 && endPosData[2] == 2 && startPosData[1] == 2 && startPosData[2] == 2))) || (endPosData[1] != startPosData[1] && ((endPosData[0] != startPosData[0] || endPosData[2] != startPosData[2]) || (endPosData[0] == 2 && endPosData[2] == 2 && startPosData[0] == 2 && startPosData[2] == 2))) || (endPosData[2] != startPosData[2] && ((endPosData[1] != startPosData[1] || endPosData[0] != startPosData[0]) || (endPosData[1] == 2 && endPosData[0] == 2 && startPosData[1] == 2 && startPosData[0] == 2))))
        {
            isOpposite = true;
        }

        return isOpposite;
    }
    public void StandardControlOnBlock()
    {
        deleteSelector.SetActive(false);
        if (((Input.touchCount < touchCountEditorFix + 2 && Input.touchCount > touchCountEditorFix) && !moveGesture) || (mouseMoved))
        {
            RaycastHit hit;
            Ray ray;
            bool withMouse;
            if (mouseMoved)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                withMouse = true;
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(touchCountEditorFix).position);
                withMouse = false;
            }
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                Vector3 endPosition = hit.point + hit.normal / 2;

                endPosition.x = (float)System.Math.Round(endPosition.x, System.MidpointRounding.AwayFromZero);
                endPosition.y = (float)System.Math.Round(endPosition.y, System.MidpointRounding.AwayFromZero);
                endPosition.z = (float)System.Math.Round(endPosition.z, System.MidpointRounding.AwayFromZero);

                int[] endPosData;
                Vector3 newEndPosition = CorrectedPositions(endPosition, touched, out endPosData, 0);

                tileSelector.transform.position = newEndPosition;

                tileSelector.transform.GetChild(0).gameObject.transform.up = hit.normal;

                bool endedTouch = false;
                if (!withMouse)
                {
                    if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                    {
                        touched = true;
                        originPointerPosition = endPosition;
                        correctedOriginPointerPosition = newEndPosition;
                        startPosData = endPosData;
                    }
                    else if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Ended)
                    {
                        endedTouch = true;
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        touched = true;
                        originPointerPosition = endPosition;
                        correctedOriginPointerPosition = newEndPosition;
                        startPosData = endPosData;
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        endedTouch = true;
                    }
                }

                if (possible && touched)
                {
                    if (((Input.touchCount == touchCountEditorFix + 1 && !withMouse) || (Input.GetMouseButton(0) && withMouse)) && !endedTouch)
                    {
                        Vector3 difference = newEndPosition - correctedOriginPointerPosition;

                        tileSelector.transform.localScale = new Vector3(Mathf.Abs(difference.x) + 1, Mathf.Abs(difference.y) + 1, Mathf.Abs(difference.z) + 1);
                        tileSelector.transform.position = correctedOriginPointerPosition + difference / 2;

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
                        BuildSelected(newEndPosition);
                        touched = false;
                    }
                }
                else if (endedTouch)
                {
                    touched = false;
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
            touched = false;
        }
    }

    private void StandardControlInBlock(int state)
    {
        tileSelector.SetActive(false);
        if (((Input.touchCount < touchCountEditorFix + 2 && Input.touchCount > touchCountEditorFix) && !moveGesture) || (mouseMoved))
        {
            RaycastHit hit;
            Ray ray;
            bool withMouse;
            if (mouseMoved)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                withMouse = true;
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(touchCountEditorFix).position);
                withMouse = false;
            }
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

                    int[] endPosData;
                    Vector3 newEndPosition = CorrectedPositions(endPosition, touched, out endPosData, 1);

                    deleteSelector.transform.position = endPosition;
                    deleteSelector.transform.GetChild(0).gameObject.transform.up = hit.normal;

                    bool endedTouch = false;
                    if (!withMouse)
                    {
                        if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                        {
                            touched = true;
                            originPointerPosition = endPosition;
                            correctedOriginPointerPosition = newEndPosition;
                            startPosData = endPosData;
                        }
                        else if (Input.GetTouch(touchCountEditorFix).phase == TouchPhase.Ended)
                        {
                            endedTouch = true;
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                        {
                            touched = true;
                            originPointerPosition = endPosition;
                            correctedOriginPointerPosition = newEndPosition;
                            startPosData = endPosData;
                        }
                        else if (Input.GetMouseButtonUp(0))
                        {
                            endedTouch = true;
                        }
                    }
                    if (possible && touched)
                    {
                        if (((Input.touchCount == touchCountEditorFix + 1 && !withMouse) || (Input.GetMouseButton(0) && withMouse)) && !endedTouch)
                        {
                            Vector3 difference = newEndPosition - correctedOriginPointerPosition;
                            deleteSelector.transform.localScale = new Vector3(Mathf.Abs(difference.x) + 1, Mathf.Abs(difference.y) + 1, Mathf.Abs(difference.z) + 1);
                            deleteSelector.transform.position = correctedOriginPointerPosition + difference / 2;

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
                            switch (state)
                            {
                                case 0:
                                    DeleteSelected(newEndPosition);
                                    break;
                                case 1:
                                    PaintSelected(newEndPosition);
                                    break;
                            }

                            touched = false;
                        }
                    }
                    else if (endedTouch)
                    {
                        touched = false;
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
            touched = false;
        }
    }
    private void BuildSelected(Vector3 endPosition)
    {
        actionsQuantity++;
        Vector2[] bounds = CalculateBounds(endPosition);

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

        undoRedoScript.AddAction(action);
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
    private void DeleteSelected(Vector3 endPosition)
    {
        Vector2[] bounds = CalculateBounds(endPosition);

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
    private void PaintSelected(Vector3 endPosition)
    {
        Vector2[] bounds = CalculateBounds(endPosition);

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

        undoRedoScript.AddAction(action);

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
