using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

public class UIController : MonoBehaviour
{
    public GameObject combinedMesh;
    public GameObject voxelModel;
    public Button doneButton;
    private bool debugMode = false;
    public Text fps;
    public Text operatingSystem;
    public Text monitor;
    public GameObject DebugCursor;
    public Sprite colorImage;

    private Vector3 originalEulers;
    private Vector3 transformEulers;

    public int colorWheelAnimationDuration = 100;
    public GameObject debugInfos;
    private bool colorWheelAnimation = false;
    private bool colorWheelOut = false;
    public GameObject colorWheel;

    public int selectedState = 0;

    public Button paintButton;
    public Button buildButton;
    public Button deleteButton;
    public Button redoButton;
    public GameObject color;
    public GameObject leftUI;
    public GameObject bottomUI;
    public FlexibleColorPicker fcp;
    public UndoRedo undoRedo;
    public Button colorSelect;

    public Image pickedColor;

    public Color selectedColor;

    private float dpiScaler;
    private float clickTime;
    private bool lastColorSelected = false;
    private bool activeColorSelector;
    // Start is called before the first frame update
    void Start()
    {
        selectedColor = pickedColor.GetComponent<Image>().color;

        doneButton.interactable = false;
        redoButton.interactable = false;

        operatingSystem.text = SystemInfo.operatingSystem;

        buildButton.interactable = false;
        deleteButton.interactable = true;
        paintButton.interactable = true;

        scaleUIWithDpi();

        monitor.text = "DPs: " + Display.displays.Length + " Res: " + Screen.currentResolution + " TS: " + Input.touchSupported + " TC: " + Input.touchCount + " DPI: " + Screen.dpi + " SaveArea: " + Screen.safeArea;
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode)
        {
            if (debugInfos.activeSelf)
            {
                fps.text = (int)(1.0 / Time.smoothDeltaTime) + " FPS";
                monitor.text = "DPs: " + Display.displays.Length + " Res: " + Screen.currentResolution + " TS: " + Input.touchSupported + " TC: " + Input.touchCount + " DPI: " + Screen.dpi + " SaveArea: " + Screen.safeArea;
            }
            else
            {
                debugInfos.SetActive(true);
                //DebugCursor.SetActive(true);
            }
        }
        else if (debugInfos.activeSelf)
        {
            debugInfos.SetActive(false);
            //DebugCursor.SetActive(false);
        }

        if (colorWheelAnimation)
        {
            colorWheelAnimate();
        }
        if (SceneController.activeTouchControl)
        {
            if(Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    clickTime = Time.time;
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    clickTime = 0;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickTime = Time.time;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                clickTime = 0;
            }
        }
    }
    public void editDone()
    {
        if (voxelModel.GetComponentsInChildren<MeshFilter>() != null)
        {
            //opimizeMesh();

            
            MeshFilter[] meshFilters = voxelModel.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }
            //combinedMesh = PrefabUtility.SaveAsPrefabAssetAndConnect(combinedMesh, "Assets/CustomModels/CombinedMesh.prefab", InteractionMode.AutomatedAction);
            MeshFilter mFilter = combinedMesh.AddComponent<MeshFilter>(); ;
            mFilter.sharedMesh = new Mesh();
            mFilter.sharedMesh.CombineMeshes(combine, true);
            mFilter.sharedMesh.RecalculateBounds();
            mFilter.sharedMesh.RecalculateNormals();
            mFilter.sharedMesh.Optimize();



            //combinedMesh.transform.gameObject.SetActive(true);
            doneButton.enabled = false;



            MeshFilter m = (MeshFilter)Instantiate(combinedMesh.transform.GetComponent<MeshFilter>());
            //AssetDatabase.CreateAsset(m.mesh, "Assets/CustomModels/MyMesh.asset");
            //AssetDatabase.SaveAssets();
            combinedMesh.transform.GetComponent<MeshFilter>().mesh = m.mesh;

            //SceneManager.LoadScene("MainScene");
            
        }
    }

    private void opimizeMesh()
    {
        for(int i = 0; i<voxelModel.transform.childCount; i++)
        {
            bool[] neighbor = new bool[6];
            int neighbors = 0;

            Vector3Int objPos = new Vector3Int((int)voxelModel.transform.GetChild(i).transform.position.x, (int)voxelModel.transform.GetChild(i).transform.position.y, (int)voxelModel.transform.GetChild(i).transform.position.z);
            if (objPos.y < SceneController.dimensions.y)
            {
                if (SceneController.gridOfObjects[objPos.x+ SceneController.dimensions.x / 2, objPos.y+1, objPos.z+ SceneController.dimensions.y / 2] != null)
                {
                    neighbor[0] = true;
                    neighbors++;
                }
            }
            if (objPos.y > 0)
            {
                if (SceneController.gridOfObjects[objPos.x+ SceneController.dimensions.x / 2, objPos.y-1, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[1] = true;
                    neighbors++;
                }
            }
            if (objPos.x < SceneController.dimensions.x/2)
            {
                if (SceneController.gridOfObjects[objPos.x+1+ SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[2] = true;
                    neighbors++;
                }
            }
            if (objPos.x > -SceneController.dimensions.x / 2)
            {
                if (SceneController.gridOfObjects[objPos.x-1+ SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[3] = true;
                    neighbors++;
                }
            }
            if (objPos.z < SceneController.dimensions.z / 2)
            {
                if (SceneController.gridOfObjects[objPos.x+ SceneController.dimensions.x / 2, objPos.y, objPos.z+1 + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[4] = true;
                    neighbors++;
                }
            }
            if (objPos.z > -SceneController.dimensions.z / 2)
            {
                if (SceneController.gridOfObjects[objPos.x+ SceneController.dimensions.x / 2, objPos.y, objPos.z-1 + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[5] = true;
                    neighbors++;
                }
            }
            if (neighbors == 6)
            {
                Destroy(voxelModel.transform.GetChild(i));
            }
            else if(neighbors > 0)
            {
                Mesh mesh = voxelModel.transform.GetChild(i).transform.GetComponent<MeshFilter>().mesh;
                int[] oldTriangles = mesh.triangles;
                int[] newTriangles = new int[mesh.triangles.Length - neighbors*2*3];

                HashSet<int> indices = new HashSet<int>(mesh.triangles.AsEnumerable().Distinct().Where(index =>
                mesh.normals[index] == Vector3.up
                ).ToList());
                int counter = 0;
                foreach (int h in indices)
                {
                    newTriangles[counter++] = h;
                }
            }
        }
    }

    public void SetDebugMode(bool debugMode)
    {
        this.debugMode = debugMode;
    }


    /*
    public void ResetRotation()
    {
        if (!refocusAnimation)
        {
            transformEulers = -1 * voxelModel.transform.eulerAngles / timeInFrames;
            refocusAnimation = true;
        }

    }*/
    public void undo()
    {
        closeColorWheel(null);
        undoRedo.Undo();
    }
    public void redo()
    {
        closeColorWheel(null);
        undoRedo.Redo();
    }
    public void setSelectedState(int state)
    {
        closeColorWheel(null);
        selectedState = state;
        switch (state)
        {
            case 0:
                buildButton.interactable = false;
                deleteButton.interactable = true;
                paintButton.interactable = true;
                break;
            case 1:
                buildButton.interactable = true;
                deleteButton.interactable = false;
                paintButton.interactable = true;
                break;
            case 2:
                buildButton.interactable = true;
                deleteButton.interactable = true;
                paintButton.interactable = false;
                break;
        }
    }
    public void changeColor(Button button)
    {
        Debug.Log("ButtonClick");
        if (button.transform.GetSiblingIndex() == 5)
        {
            if (!lastColorSelected)
            {
                clickTime -= 0.5f;
                button.GetComponent<Image>().sprite = colorImage;
            }
        }
        if(Time.time- clickTime < 0.5f)
        {
            closeColorWheel(button);
        }
        else
        {
            fcp.setComesFromButton(button);
            activeColorSelector = true;
            Color buttonColor = button.GetComponent<Image>().color;
            fcp.startingColor = buttonColor;
            fcp.gameObject.SetActive(true);
            fcp.color = buttonColor;
        }
    }
    private void setSelectedColor(Button button)
    {
        selectedColor = button.GetComponent<Image>().color;
        button.GetComponent<Image>().color = pickedColor.color;
        pickedColor.color = selectedColor;
    }
    private void colorWheelAnimate()
    {
        if (colorWheelOut)
        {
            float scaler = (1f + dpiScaler) / colorWheelAnimationDuration;
            if (!(colorWheel.transform.localScale.x <= 0))
            {
                colorWheel.transform.localScale = new Vector3(colorWheel.transform.localScale.x - scaler, colorWheel.transform.localScale.y - scaler, 1);
            }
            else
            {
                colorWheel.transform.localScale = new Vector3(0, 0, 1);
                colorWheel.SetActive(false);
                colorWheelAnimation = false;
                colorWheelOut = false;
            }
        }
        else
        {
            float scaler = (1f+dpiScaler) / colorWheelAnimationDuration;
            if (!(colorWheel.transform.localScale.x >= 1))
            {
                colorWheel.transform.localScale = new Vector3(colorWheel.transform.localScale.x + scaler, colorWheel.transform.localScale.y + scaler, 1);
            }
            else
            {
                colorWheel.transform.localScale = new Vector3(1+dpiScaler, 1+ dpiScaler, 1);
                colorWheelAnimation = false;
                colorWheelOut = true;
            }
        }
    }
    public void toggleColorWheel()
    {
        if (Time.time - clickTime < 0.5f)
        {
            if (!colorWheel.activeInHierarchy)
            {
                if (fcp.gameObject.activeInHierarchy)
                {
                    fcp.gameObject.SetActive(false);
                    activeColorSelector = false;
                }
                colorWheel.SetActive(true);
                colorWheelAnimation = true;
            }
            else
            {
                closeColorWheel(null);
            }
        }
        else
        {
            fcp.setComesFromButton(colorSelect);
            activeColorSelector = true;
            Color buttonColor = colorSelect.GetComponent<Image>().color;
            fcp.startingColor = buttonColor;
            fcp.gameObject.SetActive(true);
            fcp.color = buttonColor;
            setSelectedColor(colorSelect);
        }
    }
    public void closeColorWheel(Button b)
    {
        if (activeColorSelector)
        {
            setSelectedColor(fcp.getComesFromButton());
            fcp.gameObject.SetActive(false);
            activeColorSelector = false;
        }
        else
        {
            if (b != null)
            {
                setSelectedColor(b);
            }
        }
        if (colorWheel.activeInHierarchy)
        {
            colorWheelAnimation = true;
        }
    }
    private void scaleUIWithDpi()
    {
        int longSide;
        if(Screen.currentResolution.width > Screen.currentResolution.height)
        {
            longSide = Screen.currentResolution.width;
        }
        else
        {
            longSide = Screen.currentResolution.height;
        }
        if(longSide/ Screen.dpi < 6)//Mobilephone
        {
            dpiScaler = Screen.dpi / 1800f;
        }else if(longSide / Screen.dpi < 12)//Tablet
        {
            dpiScaler = Screen.dpi / 2200f;
        }else if(longSide / Screen.dpi > 39)//Big Screen
        {
            dpiScaler = -Screen.dpi / 2200f;
        }
        else
        {
            dpiScaler = 0;
        }
        doneButton.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        color.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        leftUI.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        bottomUI.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);

        
    }
    public bool getActiveColorSelector()
    {
        return activeColorSelector;
    }
}
