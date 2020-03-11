using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.SceneManagement;
using EmailSender;
using System.Text.RegularExpressions;

public class UIController : MonoBehaviour
{
    public GameObject voxelModel;
    public Button doneButton;
    private bool debugMode = false;
    public Text fps;
    public Text operatingSystem;
    public Text monitor;
    public Sprite colorImage;
    public GameObject voxel;

    public int colorWheelAnimationDuration = 100;
    public GameObject debugInfos;
    private bool colorWheelAnimation = false;
    private bool colorWheelOut = false;
    public GameObject colorWheel;
    public SceneController sceneController;
    public float longPressBias = 0.5f;

    public int selectedState = 0;
    public GameObject checker;

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
    public GameObject menu;
    public GameObject warningOverlayPanel;
    public GameObject overrideWarning;
    public GameObject unsavedChangesWarning;
    public Button savestate;
    public Button menuButton;
    public Animator menuAnimator;
    public GameObject NewSaveButtonGroup;
    public GameObject sizeSelectPopUp;
    public GameObject exportErrorPanel;
    public GameObject scaleSelectorPanel;
    public GameObject unsavedBlockade;
    public GameObject sendMailPanel;

    public Image pickedColor;
    public Color selectedColor;

    private float dpiScaler;
    private float clickTime;
    private bool lastColorSelected = false;
    private bool activeColorSelector;
    private bool activeMenu;
    private string currentDataName;
    private int comingFromSaveLoad;
    private VirtualKeyboard vk;
    private GameObject m_objectOnClick;
    private bool standalone;
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
    private string modelName;
    private Vector3 modelSize;
    private bool[] mailChecks = new bool[3];
    // Start is called before the first frame update
    void Start()
    {
        mailChecks[1] = true;
        vk = new VirtualKeyboard();
        selectedColor = pickedColor.GetComponent<Image>().color;

        doneButton.interactable = false;
        redoButton.interactable = false;

        operatingSystem.text = SystemInfo.operatingSystem;

        buildButton.interactable = false;
        deleteButton.interactable = true;
        paintButton.interactable = true;

        ScaleUIWithDpi();

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
                monitor.text = "DPs: " + Display.displays.Length + " Res: " + Screen.currentResolution + " TS: " + Input.touchSupported + " TC: " + Input.touchCount + " DPI: " + Screen.dpi + " SaveArea: " + Screen.safeArea + " MM: " + sceneController.mouseMoved + " DebugMessage: " + SceneController.lastDebugMessage;
            }
            else
            {
                debugInfos.SetActive(true);
            }
        }
        else if (debugInfos.activeSelf)
        {
            debugInfos.SetActive(false);
        }

        if (colorWheelAnimation)
        {
            ColorWheelAnimate();
        }

        if (Input.touchCount == 1)
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
    private void OnApplicationQuit()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            CloseKeyboard();
        }
    }
    public void SetupUIForVersions(bool standalone)
    {
        this.standalone = standalone;
        GameObject container = menu.transform.GetChild(1).transform.GetChild(0).gameObject;
        if (!standalone)
        {
            container.transform.Find("UseModelButton").gameObject.SetActive(true);
            container.transform.Find("UseModelStroke").gameObject.SetActive(true);
            container.transform.Find("ExitButton").transform.Find("Text").GetComponent<Text>().text = "Back";
        }
    }
    public void SetDebugMode(bool debugMode)
    {
        this.debugMode = debugMode;
    }
    public void Undo()
    {
        CloseColorWheel(null);
        undoRedo.Undo();
    }
    public void Redo()
    {
        CloseColorWheel(null);
        undoRedo.Redo();
    }
    public void SetSelectedState(int state)
    {
        CloseColorWheel(null);
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
    public void ChangeColor(Button button)
    {
        if (button.transform.GetSiblingIndex() == 5)
        {
            if (!lastColorSelected)
            {
                clickTime -= 0.5f;
                button.GetComponent<Image>().sprite = colorImage;
                lastColorSelected = true;
            }
        }
        if(Time.time- clickTime < longPressBias)
        {
            CloseColorWheel(button);
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
        undoRedo.unsavedChanges = true;
    }
    private void SetSelectedColor(Button button)
    {
        selectedColor = button.GetComponent<Image>().color;
        button.GetComponent<Image>().color = pickedColor.color;
        pickedColor.color = selectedColor;
    }
    private void ColorWheelAnimate()
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
    public void ToggleColorWheel()
    {
        if (Time.time - clickTime < longPressBias)
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
                CloseColorWheel(null);
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
            SetSelectedColor(colorSelect);
            undoRedo.unsavedChanges = true;
        }
    }
    public void CloseColorWheel(Button b)
    {
        if (activeColorSelector)
        {
            SetSelectedColor(fcp.getComesFromButton());
            fcp.gameObject.SetActive(false);
            activeColorSelector = false;
        }
        else
        {
            if (b != null)
            {
                SetSelectedColor(b);
            }
        }
        if (colorWheel.activeInHierarchy)
        {
            colorWheelAnimation = true;
        }
    }
    private void ScaleUIWithDpi()
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

        color.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        leftUI.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        bottomUI.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        menuButton.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        overrideWarning.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        unsavedChangesWarning.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
        sizeSelectPopUp.transform.localScale = new Vector3(1 + dpiScaler, 1 + dpiScaler, 1);
    }
    public bool GetActiveColorSelector()
    {
        return activeColorSelector;
    }
    public void ToggleMenu()
    {
        CloseColorWheel(null);
        if (activeMenu)
        {
            menuAnimator.SetBool("activeSecondMenu", false);
            menuAnimator.SetBool("activeMenu", false);
            activeMenu = false;
        }
        else
        {
            menuAnimator.SetBool("activeMenu", true);
            activeMenu = true;
        }
    }
    public bool GetActiveMenu()
    {
        return activeMenu;
    }
    public void SaveLoad(Button saveState)
    {
        if(Time.time-clickTime < longPressBias)
        {
            if (comingFromSaveLoad == 0)
            {
                TryLoad(saveState);
            }
            else
            {
                TrySave(saveState);
            }
        }
        else
        {
            //StartDeleteProcess(saveState);
        }
    }
    private void TryLoad(Button saveState)
    {
        string dataName = saveState.GetComponentInChildren<Text>().text;
        CheckLoad(dataName);
    }
    private void CheckLoad(string dataName)
    {
        string path = Application.persistentDataPath + "/models/" + dataName + ".vx";
        if (undoRedo.unsavedChanges)
        {
            currentDataName = dataName;
            unsavedChangesWarning.SetActive(true);
            unsavedChangesWarning.transform.Find("YesButtonExit").gameObject.SetActive(false);
            unsavedChangesWarning.transform.Find("YesButtonNewScene").gameObject.SetActive(false);
            unsavedChangesWarning.transform.Find("YesButton").gameObject.SetActive(true);
            warningOverlayPanel.SetActive(true);
        }
        else
        {
            if (File.Exists(path))
            {
                LoadModel(dataName);
            }
        }
    }
    private void TrySave(Button saveState)
    {
        string dataName = saveState.GetComponentInChildren<Text>().text;
        string path = Application.persistentDataPath + "/models/" + dataName +".vx";

        if (File.Exists(path))
        {
            currentDataName = dataName;
            overrideWarning.SetActive(true);
            warningOverlayPanel.SetActive(true);
        }
        else
        {
            SaveModel(dataName);
        }
    }
    public void SaveUnderNew(InputField input)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            CloseKeyboard();
        }
        string dataName = input.text;
        string path = Application.persistentDataPath + "/models/" + dataName + ".vx";

        if (File.Exists(path))
        {
            currentDataName = dataName;
            overrideWarning.SetActive(true);
            warningOverlayPanel.SetActive(true);
        }
        else
        {
            SaveModel(dataName);
        }
        input.text = "";
        NewSaveButtonGroup.transform.GetChild(0).gameObject.SetActive(true);
        NewSaveButtonGroup.transform.GetChild(1).gameObject.SetActive(false);
    }
    private void SaveModel(string dataName)
    {
        modelName = dataName;
        menuAnimator.SetBool("activeSecondMenu", false);
        menu.transform.GetChild(1).transform.GetChild(0).transform.Find("SaveButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        undoRedo.unsavedChanges = false;
        SaveSystem.SaveEditableModel(Application.persistentDataPath + "/models/" + dataName + ".vx", SceneController.dimensions, SceneController.actionsQuantity, (int)SceneController.timeTaken, lastColorSelected, colorSelect.GetComponent<Image>().color, colorWheel);
    }
    private void LoadModel(string dataName)
    {
        ToggleMenu();
        menu.transform.GetChild(1).transform.GetChild(0).transform.Find("LoadButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        ModelData modelData = SaveSystem.LoadEditableModel(dataName);
        if(modelData != null)
        {
            modelName = dataName;
            Vector3Int dimensions = new Vector3Int();
            dimensions.x = modelData.dimensions[0];
            dimensions.y = modelData.dimensions[1];
            dimensions.z = modelData.dimensions[2];

            sceneController.ClearGrid();
            SceneController.dimensions = dimensions;
            SceneController.gridOfObjects = new GameObject[dimensions.x, dimensions.y, dimensions.z];
            SceneController.timeTaken = modelData.timeTaken;
            SceneController.actionsQuantity = modelData.actions;
            sceneController.UpdateScene();

            Renderer rend;
            int count = 0;
            for (int i = 0; i < modelData.dimensions[0]; i++)
            {
                for (int k = 0; k < modelData.dimensions[1]; k++)
                {
                    for (int h = 0; h < modelData.dimensions[2]; h++)
                    {
                        if (modelData.blockThere[count])
                        {
                            GameObject cube = Instantiate(voxel, new Vector3(i-dimensions.x/2, k, h - dimensions.z / 2), voxelModel.transform.rotation);
                            cube.transform.parent = voxelModel.transform;
                            rend = cube.transform.GetComponent<Renderer>();
                            rend.material.shader = Shader.Find("Standard");
                            Color c = new Color(modelData.colors[0,count], modelData.colors[1, count], modelData.colors[2, count]);
                            rend.material.SetColor("_Color", c);
                            SceneController.gridOfObjects[i, k, h] = cube;
                        }
                        count++;
                    }
                }
            }
            undoRedo.resetList();

            
            lastColorSelected = modelData.lastColorPicked;
            if (modelData.colorWheelColors != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    Color c = new Color(modelData.colorWheelColors[i, 0], modelData.colorWheelColors[i, 1], modelData.colorWheelColors[i, 2]);
                    if (i == 0)
                    {
                        colorSelect.GetComponent<Image>().color = c;
                    }
                    else if (i == 5)
                    {
                        if (lastColorSelected)
                        {
                            colorWheel.transform.GetChild(i).GetComponent<Image>().color = c;
                        }
                    }
                    else
                    {
                        colorWheel.transform.GetChild(i).GetComponent<Image>().color = c;
                    }
                }
            }
        }
    }
    public void AcceptDiscard(bool fromLoad)
    {
        if (fromLoad)
        {
            LoadModel(currentDataName);
        }
        else
        {
            sizeSelectPopUp.SetActive(true);
        }
        currentDataName = null;
        CloseChangeDiscardWarning();
    }
    public void DeclineDiscardWarning()
    {
        currentDataName = null;
        CloseChangeDiscardWarning();
    }
    private void CloseChangeDiscardWarning()
    {
        menuAnimator.SetBool("activeSecondMenu", false);
        menu.transform.GetChild(1).transform.GetChild(0).transform.Find("LoadButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        unsavedChangesWarning.SetActive(false);
        warningOverlayPanel.SetActive(false);
    }
    public void AcceptSaveOverride(bool response)
    {
        if (response)
        {
            SaveModel(currentDataName);
        }
        currentDataName = null;
        CloseSaveOverrideWarning();
    }
    private void CloseSaveOverrideWarning()
    {
        menuAnimator.SetBool("activeSecondMenu", false);
        menu.transform.GetChild(1).transform.GetChild(0).transform.Find("SaveButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        overrideWarning.SetActive(false);
        warningOverlayPanel.SetActive(false);
    }
    public void DisplaySaveFiles(int comingFrom)
    {
        comingFromSaveLoad = comingFrom;
        GameObject saveLoadMenu;
        saveLoadMenu = menu.transform.GetChild(0).transform.GetChild(2).gameObject;
        saveLoadMenu.SetActive(true);

        int count = 0;
        int height = 105;
        string path = Application.persistentDataPath + "/models";

        if (!Directory.Exists(path))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(path);
        }

        if (comingFrom == 1)
        {
            NewSaveButtonGroup.transform.GetChild(0).gameObject.SetActive(true);
            NewSaveButtonGroup.transform.GetChild(1).transform.GetChild(1).GetComponent<InputField>().text = "";
            NewSaveButtonGroup.transform.GetChild(1).gameObject.SetActive(false);
            NewSaveButtonGroup.SetActive(true);
            menu.transform.GetChild(1).transform.GetChild(0).transform.Find("SaveButton").GetComponent<Animator>().SetBool("SelectedByCode", true);
            menu.transform.GetChild(1).transform.GetChild(0).transform.Find("LoadButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        }
        else
        {
            NewSaveButtonGroup.transform.GetChild(0).gameObject.SetActive(true);
            NewSaveButtonGroup.transform.GetChild(1).gameObject.SetActive(false);
            NewSaveButtonGroup.SetActive(false);
            menu.transform.GetChild(1).transform.GetChild(0).transform.Find("LoadButton").GetComponent<Animator>().SetBool("SelectedByCode", true);
            menu.transform.GetChild(1).transform.GetChild(0).transform.Find("SaveButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
            height = 0;
        }
        for(int i = 1; i< saveLoadMenu.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.childCount; i++)
        {
            Destroy(saveLoadMenu.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject);
        }

        foreach (string file in System.IO.Directory.GetFiles(path))
        {
            height += 105;
            
            Button save = Instantiate(savestate) as Button;
            save.transform.SetParent(saveLoadMenu.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform, false);
            save.GetComponent<Button>().onClick.RemoveAllListeners();
            save.GetComponent<Button>().onClick.AddListener(delegate { SaveLoad(save); });
            save.transform.Find("Text").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(file);
            ModelData m = SaveSystem.LoadEditableModel(Path.GetFileNameWithoutExtension(file));
            int points = (int)((m.actions - (m.timeTaken * 0.05f)) * 10);
            if (points < 0)
            {
                points = 0;
            }
            save.transform.Find("PointsText").GetComponent<Text>().text = points + " Points";
            saveLoadMenu.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            count++;
        }
        menuAnimator.SetBool("activeSecondMenu", true);
    }
    public void DecoyButton(Button b) //only during unfinished Menu
    {
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }
    public void DisplaySaveInputField()
    {
        
        NewSaveButtonGroup.transform.GetChild(0).gameObject.SetActive(false);
        NewSaveButtonGroup.transform.GetChild(1).gameObject.SetActive(true);
        InputField inputField = NewSaveButtonGroup.transform.GetChild(1).GetComponentInChildren<InputField>();
        inputField.Select();
        inputField.ActivateInputField();

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            OpenKeyboard();
        }

    }
    public void CheckForLengh(InputField input)
    {
        if(input.text.Length < 1)
        {
            doneButton.interactable = false;
        }
        else
        {
            doneButton.interactable = true;
        }
    }
    public void CloseSecondMenu()
    {
        NewSaveButtonGroup.transform.GetChild(0).gameObject.SetActive(true);
        NewSaveButtonGroup.transform.GetChild(1).gameObject.SetActive(false);
        NewSaveButtonGroup.SetActive(false);
        menu.transform.GetChild(1).transform.GetChild(0).transform.Find("LoadButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        menu.transform.GetChild(1).transform.GetChild(0).transform.Find("SaveButton").GetComponent<Animator>().SetBool("SelectedByCode", false);
        menuAnimator.SetBool("activeSecondMenu", false);
    }
    public void NewScene()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            CloseKeyboard();
        }

        Vector3Int dimensions = new Vector3Int();
        switch (sizeSelectPopUp.transform.Find("Dropdown").GetComponent<Dropdown>().value)
        {
            case 0:
                dimensions = new Vector3Int(11, 11, 11);
                break;
            case 1:
                dimensions = new Vector3Int(21, 21, 21);
                break;
            case 2:
                dimensions = new Vector3Int(41, 41, 41);
                break;
            case 3:
                try
                {
                    dimensions.x = int.Parse(sizeSelectPopUp.transform.Find("CustomInput").transform.GetChild(0).GetComponent<InputField>().text);
                    dimensions.y = int.Parse(sizeSelectPopUp.transform.Find("CustomInput").transform.GetChild(1).GetComponent<InputField>().text);
                    dimensions.z = int.Parse(sizeSelectPopUp.transform.Find("CustomInput").transform.GetChild(2).GetComponent<InputField>().text);
                }
                catch
                {
                    dimensions = new Vector3Int(21, 21, 21);
                }
                
                break;
        }
        menuAnimator.SetBool("activeMenu", false);
        sceneController.ClearGrid();
        SceneController.dimensions = dimensions;
        SceneController.gridOfObjects = new GameObject[dimensions.x, dimensions.y, dimensions.z];
        SceneController.timeTaken = 0;
        SceneController.actionsQuantity = 0;
        sceneController.UpdateScene();
        sizeSelectPopUp.SetActive(false);
        activeMenu = false;
    }
    public void FitDimension(int field)
    {
        string s = sizeSelectPopUp.transform.Find("CustomInput").transform.GetChild(field).GetComponent<InputField>().text.Replace("-", "");
        if (!s.Equals(""))
        {
            int number = int.Parse(s);
            if (number == 0)
            {
                number++;
            }
            else if (number % 2 == 0)
            {
                number++;
            }
            sizeSelectPopUp.transform.Find("CustomInput").transform.GetChild(field).GetComponent<InputField>().text = "" + number;
        }
    }
    public void TryNewScene()
    {
        CloseSecondMenu();
        if (undoRedo.unsavedChanges)
        {
            unsavedChangesWarning.SetActive(true);
            unsavedChangesWarning.transform.Find("YesButtonExit").gameObject.SetActive(false);
            unsavedChangesWarning.transform.Find("YesButtonNewScene").gameObject.SetActive(true);
            unsavedChangesWarning.transform.Find("YesButton").gameObject.SetActive(false);
            warningOverlayPanel.SetActive(true);
        }
        else
        {
            sizeSelectPopUp.SetActive(true);
        }
    }

    public void ShowCustomSizeInput()
    {
        if(sizeSelectPopUp.transform.Find("Dropdown").GetComponent<Dropdown>().value == 3)
        {
            sizeSelectPopUp.transform.Find("CustomInput").gameObject.SetActive(true);
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                OpenKeyboard();
            }
        }
        else
        {
            sizeSelectPopUp.transform.Find("CustomInput").gameObject.SetActive(false);
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                CloseKeyboard();
            }
        }
    }
    public void ExitProgram()
    {
        if (!standalone)
        {
            SceneManager.LoadScene("Windridge City Demo Scene", LoadSceneMode.Single);
        }
        else
        {
            Application.Quit();
        }
    }
    public void TryExitProgram()
    {
        CloseSecondMenu();
        if (undoRedo.unsavedChanges)
        {
            unsavedChangesWarning.SetActive(true);
            unsavedChangesWarning.transform.Find("YesButtonExit").gameObject.SetActive(true);
            unsavedChangesWarning.transform.Find("YesButtonNewScene").gameObject.SetActive(false);
            unsavedChangesWarning.transform.Find("YesButton").gameObject.SetActive(false);
            warningOverlayPanel.SetActive(true);
        }
        else
        {
            ExitProgram();
        }
    }
    public void OpenKeyboard()
    {
        {
            vk.ShowTouchKeyboard();
        }
    }
    public void CloseKeyboard()
    {
        {
            vk.HideTouchKeyboard();
        }
    }
    public void ShowImportWindow()
    {
        CloseSecondMenu();
        string initialPath = "";
        FileBrowser.SetFilters(false, new FileBrowser.Filter("", ".vx"));
        FileBrowser.SetDefaultFilter(".vx");
        FileBrowser.ShowLoadDialog((path) => ImportModel(path), null, false, initialPath, "Import", "Open");
    }
    private void ImportModel(string path)
    {
        CheckLoad(SaveSystem.ImportModel(path));
    }
    private void ShowExportWindow()
    {
        CloseSecondMenu();
        string initialPath = "";
        FileBrowser.SetFilters(false, new FileBrowser.Filter("", ".vx"), new FileBrowser.Filter("", ".obj"));
        FileBrowser.SetDefaultFilter(".vx");
        FileBrowser.ShowSaveDialog((path) => ExportModel(path), null, false, initialPath, "Export", "Create");
    }
    private void ExportModel(string path)
    {
        string input = scaleSelectorPanel.transform.Find("ScaleInput").GetComponent<InputField>().text.Replace(".", ",");
        if (input.Length != 0)
        {
            if (input.Substring(input.Length - 1).Equals(","))
            {
                input = input.Substring(0, input.Length - 1);
            }
        }
        else
        {
            input = "0,3";
        }

        float scale = float.Parse(input);
        if (scale == 0)
        {
            scale = 0.3f;
        }
        scaleSelectorPanel.SetActive(false);

        string extension = Path.GetExtension(path);
        if (extension.Equals(".vx"))
        {
            SaveSystem.SaveEditableModel(path, SceneController.dimensions, SceneController.actionsQuantity, (int)SceneController.timeTaken, lastColorSelected, colorSelect.GetComponent<Image>().color, colorWheel);
        }
        else
        {
            bool executed;
            SaveSystem.ExportModelToObj(path, voxelModel, scale, checker, out executed);
            if (!executed)
            {
                OpenExportError();
            }
        }
    }
    private void StartDeleteProcess(Button clickedButton)
    {
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);

        Button overlayButton = Instantiate(savestate) as Button;
        overlayButton.transform.SetParent(menu.transform.GetChild(0).transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).transform,false);
        overlayButton.transform.Find("Stroke").gameObject.SetActive(false);
        Destroy(overlayButton.GetComponent<Animator>());
        overlayButton.transform.position = clickedButton.transform.position;
        //overlayButton.GetComponent<Button>().onClick.AddListener(delegate { SaveLoad(save); });
        overlayButton.transform.Find("Text").GetComponent<Text>().text = clickedButton.GetComponentsInChildren<Text>()[0].text;
        overlayButton.transform.Find("PointsText").GetComponent<Text>().text = clickedButton.GetComponentsInChildren<Text>()[1].text;

        clickedButton.GetComponentsInChildren<Text>()[0].enabled = false;
        clickedButton.GetComponentsInChildren<Text>()[1].enabled = false;
        Color color = overlayButton.GetComponent<Image>().color;
        color.a = 0.5f;
        overlayButton.GetComponent<Image>().color = color;
    }
    public void CloseExportError()
    {
        exportErrorPanel.SetActive(false);
    }
    private void OpenExportError()
    {
        exportErrorPanel.SetActive(true);
    }
    public void SortSavesByNumbers(bool fromHighest)
    {

    }
    public void SortSavesByNames(bool fromA)
    {

    }
    private void BackToSceneWithModel()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/internalModels/models"))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(Application.persistentDataPath + "/internalModels/models");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/internalModels/pictures"))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(Application.persistentDataPath + "/internalModels/pictures");
        }

        ExportModel(Application.persistentDataPath + "/internalModels/models/" + modelName +".obj");
        
        //hide ui
        this.GetComponent<Canvas>().enabled = false;

        //take screenshot
        StartCoroutine(TakeScreenshot());

        //switch scene
        SceneManager.LoadScene("Windridge City Demo Scene", LoadSceneMode.Single);
        
    }
    private IEnumerator TakeScreenshot()
    {
        yield return frameEnd;
        Texture2D image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        image.Apply();
        byte[] bytes = image.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath+ "/internalModels/pictures/" + modelName + ".png", bytes);
    }
    public void ShowScaleDialog(string from, string to, int dataType)
    {
        modelSize = sceneController.CalculateSize();
        if(modelSize.x == 0 && modelSize.y == 0 && modelSize.z == 0)
        {
            ToggleMenu();
            return;
        }

        scaleSelectorPanel.transform.Find("OkButton").GetComponent<Button>().onClick.RemoveAllListeners();
        scaleSelectorPanel.transform.Find("OkButton").GetComponent<Button>().onClick.AddListener(delegate { ExportForMail(from, to, dataType); });
        scaleSelectorPanel.SetActive(true);

        scaleSelectorPanel.transform.Find("LenghtText").GetComponent<Text>().text = "L = " + modelSize.x * 0.3f;
        scaleSelectorPanel.transform.Find("HeightText").GetComponent<Text>().text = "H = " + modelSize.y * 0.3f;
        scaleSelectorPanel.transform.Find("WidthText").GetComponent<Text>().text = "W = " + modelSize.z * 0.3f;
    }
    public void ShowScaleDialog(bool stayInScene)
    {
        modelSize = sceneController.CalculateSize();
        if (modelSize.x == 0 && modelSize.y == 0 && modelSize.z == 0)
        {
            ToggleMenu();
            return;
        }

        if (stayInScene)
        {
            scaleSelectorPanel.transform.Find("OkButton").GetComponent<Button>().onClick.RemoveAllListeners();
            scaleSelectorPanel.transform.Find("OkButton").GetComponent<Button>().onClick.AddListener(delegate { ShowExportWindow(); });
        }
        else
        {
            scaleSelectorPanel.transform.Find("OkButton").GetComponent<Button>().onClick.RemoveAllListeners();
            scaleSelectorPanel.transform.Find("OkButton").GetComponent<Button>().onClick.AddListener(delegate { BackToSceneWithModel(); });
        }
        scaleSelectorPanel.SetActive(true);

        scaleSelectorPanel.transform.Find("LenghtText").GetComponent<Text>().text = "L = " + modelSize.x * 0.3f;
        scaleSelectorPanel.transform.Find("HeightText").GetComponent<Text>().text = "H = " + modelSize.y * 0.3f;
        scaleSelectorPanel.transform.Find("WidthText").GetComponent<Text>().text = "W = " + modelSize.z * 0.3f;
    }
    public void UpdateScaleTexts()
    {
        string input = scaleSelectorPanel.transform.Find("ScaleInput").GetComponent<InputField>().text.Replace("-", "");
        scaleSelectorPanel.transform.Find("ScaleInput").GetComponent<InputField>().text = input;
        input = input.Replace(".", ",");
        if(input.Length != 0)
        {
            if (input.Substring(input.Length - 1).Equals(","))
            {
                input = input.Substring(0, input.Length - 1);
            }
        }
        else
        {
            input = "0,3";
        }
        
        float scale = float.Parse(input);
        if (scale == 0)
        {
            scale = 0.3f;
        }
        scaleSelectorPanel.transform.Find("LenghtText").GetComponent<Text>().text = "L = " + modelSize.x * scale + "m";
        scaleSelectorPanel.transform.Find("HeightText").GetComponent<Text>().text = "H = " + modelSize.y * scale + "m";
        scaleSelectorPanel.transform.Find("WidthText").GetComponent<Text>().text = "W = " + modelSize.z * scale + "m";
    }
    public void SendMailMessage(string from, string to, int mediaType)
    {
        MessageAttachment messageAttachment;
        MessageAttachmentList messageAttachments = new MessageAttachmentList();
        string bodyText;
        if (mediaType == 0)
        {
            messageAttachment = new MessageAttachment("text/plain", Application.persistentDataPath + "/models/" + modelName + ".vx");
            messageAttachments.Add(messageAttachment);

            
            bodyText = "You have gotten a Save-File from a Cubes-User.<br>" +
                "To work with the attached file please follow these steps:<br>" +
                "<ol>" +
                "<li>Download the file onto your device." +
                "<li>&#8226; Open 'Cubes' on your device (<a href='https://play.google.com/store/apps?hl=de'>Android</a> / <a href='https://google.com'>Windows</a>).<br>" +
                "or<br>" +
                "&#8226; Open '3D Engineer' on your device (<a href='https://play.google.com/store/apps?hl=de'>Android</a> / <a href='https://google.com'>Windows</a>)." +
                "<li>Import the downloaded file into Cubes.<br>" +
                "<li>Done! Now you can work with the model.</ol><br>" +
                "<br>" +
                "With best regards<br>" +
                "Your Cubes-Team<br>" +
                "<br>" +
                "<small>If you do not want to see any e-mails from Cubes again, please contact us and/or mark this e-mail as spam.</small>";
                
        }
        else if (mediaType == 1)
        {
            messageAttachment = new MessageAttachment("text/plain", Application.persistentDataPath + "/temp/" + modelName + ".obj");
            messageAttachments.Add(messageAttachment);
            messageAttachment = new MessageAttachment("text/plain", Application.persistentDataPath + "/temp/" + modelName + ".mtl");
            messageAttachments.Add(messageAttachment);

            bodyText = "You have gotten a 3D-Model from a Cubes-User.<br>" +
              "You can use it in any program that is able to work with .obj files. To use the attached files in '3D-Engineer' please follow these steps:<br>" +
              "<ol>" +
              "<li>Download both files onto your device into the same folder." +
              "<li>Open '3D Engineer' on your device (<a href='https://play.google.com/store/apps?hl=de'>Android</a> / <a href='https://google.com'>Windows</a>)." +
              "<li>Import the downloaded 3D-Model into '3D Engineer'.<br>" +
              "<li>Done! Now you can work with the model.</ol><br>" +
              "<br>" +
              "With best regards<br>" +
              "Your Cubes-Team<br>" +
              "<br>" +
              "<p style='color:lightgrey;'><small>If you do not want to see any e-mails from Cubes again, please contact us and/or mark this e-mail as spam.</small></p>";
        }
        else
        {
            messageAttachment = new MessageAttachment("text/plain", Application.persistentDataPath + "/temp/" + modelName + ".obj");
            messageAttachments.Add(messageAttachment);
            messageAttachment = new MessageAttachment("text/plain", Application.persistentDataPath + "/temp/" + modelName + ".mtl");
            messageAttachments.Add(messageAttachment);
            messageAttachment = new MessageAttachment("text/plain", Application.persistentDataPath + "/models/" + modelName + ".vx");
            messageAttachments.Add(messageAttachment);

            bodyText = "You have gotten a 3D-Model and the corresponding Save-File from a Cubes-User.<br>" +
                "To work with the attached Save-File please follow these steps:<br>" +
                "<ol>" +
                "<li>Download the file (.vx) onto your device." +
                "<li>&#8226; Open 'Cubes' on your device (<a href='https://play.google.com/store/apps?hl=de'>Android</a> / <a href='https://google.com'>Windows</a>).<br>" +
                "or<br>" +
                "&#8226; Open '3D Engineer' on your device (<a href='https://play.google.com/store/apps?hl=de'>Android</a> / <a href='https://google.com'>Windows</a>)." +
                "<li>Import the downloaded file into Cubes.<br>" +
                "<li>Done! Now you can work with the model.</ol><br>" +
                "You can use the 3D-Model in any program that is able to work with .obj files. To use the attached Model in '3D-Engineer' please follow these steps:<br>" +
                "<ol>" +
                "<li>Download the files (.obj + .mtl) onto your device into the same folder." +
                "<li>Open '3D Engineer' on your device (<a href='https://play.google.com/store/apps?hl=de'>Android</a> / <a href='https://google.com'>Windows</a>)." +
                "<li>Import the downloaded 3D-Model into '3D Engineer'.<br>" +
                "<li>Done! Now you can work with the model.</ol><br>" +
                "<br>" +
                "With best regards<br>" +
                "Your Cubes-Team<br>" +
                "<br>" +
                "<p style='color:lightgrey;'><small>If you do not want to see any e-mails from Cubes again, please contact us and/or mark this e-mail as spam.</small></p>";
        }

        SmtpSender smtpSender = new SmtpSender("smtp.gmail.com");
        smtpSender.UserName = "fiiem.cubes@gmail.com";
        smtpSender.Password = "tradpgkemlnzxafh";
        smtpSender.Send("fiiem.cubes@gmail.com", to, "File from " + from + " out of 'Cubes'", bodyText, messageAttachments);

        /*
        if(mediaType != 0)
        {
            if(File.Exists(Application.persistentDataPath + "/temp/" + modelName + ".obj"))
            {
                File.Delete(Application.persistentDataPath + "/temp/" + modelName + ".obj");
            }
            if (File.Exists(Application.persistentDataPath + "/temp/" + modelName + ".mtl"))
            {
                File.Delete(Application.persistentDataPath + "/temp/" + modelName + ".mtl");
            }
        }*/
    }
    public void TrySendMail()
    {
        if (undoRedo.unsavedChanges)
        {
            unsavedBlockade.SetActive(true);
            DisplaySaveFiles(1);
        }
        else
        {
            sendMailPanel.SetActive(true);
        }
    }
    public void CloseUI(Button button)
    {
        button.transform.parent.gameObject.SetActive(false);
    }
    public void MailToggleCheck(GameObject parent)
    {
        Toggle[] toggles = parent.GetComponentsInChildren<Toggle>();
        Color color;
        if(!toggles[0].isOn && !toggles[1].isOn)
        {
            color = new Color(1, 0.5f, 0.5f, 1);
            mailChecks[1] = false;
        }
        else
        {
            color = Color.white;
            mailChecks[1] = true;
        }
        parent.transform.GetChild(0).GetComponentInChildren<Image>().color = color;
        parent.transform.GetChild(1).GetComponentInChildren<Image>().color = color;

        bool interactable;
        if (mailChecks[0] && mailChecks[1] && mailChecks[2])
        {
            interactable = true;
        }
        else
        {
            interactable = false;
        }
        parent.transform.parent.transform.Find("SendButton").gameObject.GetComponent<Button>().interactable = interactable;

    }
    public void MailContentCheck(GameObject input)
    {
        string inputText = input.GetComponent<InputField>().text;
        string pattern = @"([a-zA-Z0-9.]{1,20})[@]([a-zA-Z0-9]{2,10})[.]([a-zA-Z0-9]{2,10})([.]([a-zA-Z0-9]{2,10})){0,1}";

        bool verifiedMail = false;
        foreach (Match m in Regex.Matches(inputText, pattern))
        {
            verifiedMail = true;
        }
        Color color;
        if (!verifiedMail)
        {
            color = new Color(1,0.5f,0.5f,1);
            mailChecks[0] = false;
        }
        else
        {
            color = Color.white;
            mailChecks[0] = true;
        }
        input.GetComponent<Image>().color = color;

        bool interactable;
        if(mailChecks[0] && mailChecks[1] && mailChecks[2])
        {
            interactable = true;
        }
        else
        {
            interactable = false;
        }
        input.transform.parent.transform.parent.transform.Find("SendButton").gameObject.GetComponent<Button>().interactable = interactable;
    }
    public void MailNameCheck(GameObject input)
    {
        Color color;
        if(input.GetComponent<InputField>().text != null && !input.GetComponent<InputField>().text.Equals(""))
        {
            color = Color.white;
            mailChecks[2] = true;
        }
        else
        {
            color = new Color(1, 0.5f, 0.5f, 1);
            mailChecks[2] = false;
        }
        input.GetComponent<Image>().color = color;

        bool interactable;
        if (mailChecks[0] && mailChecks[1] && mailChecks[2])
        {
            interactable = true;
        }
        else
        {
            interactable = false;
        }
        input.transform.parent.transform.parent.transform.Find("SendButton").gameObject.GetComponent<Button>().interactable = interactable;
    }
    public void PrepareMailData(GameObject parent)
    {
        string from = parent.transform.Find("From").transform.Find("InputField").GetComponent<InputField>().text;
        string to = parent.transform.Find("To").transform.Find("InputField").GetComponent<InputField>().text;
        int dataType;
        if(parent.transform.Find("TypeToggle").transform.GetChild(0).GetComponent<Toggle>().isOn && parent.transform.Find("TypeToggle").transform.GetChild(1).GetComponent<Toggle>().isOn)
        {
            dataType = 2;
        }else if (parent.transform.Find("TypeToggle").transform.GetChild(0).GetComponent<Toggle>().isOn)
        {
            dataType = 1;
        }
        else
        {
            dataType = 0;
        }
        if (dataType != 0)
        {
            ShowScaleDialog(from, to, dataType);
        }
        else
        {
            SendMailMessage(from, to, dataType);
        }
        sendMailPanel.SetActive(false);
    }
    public void ExportForMail(string from, string to, int dataType)
    {
        scaleSelectorPanel.SetActive(false);
        if (!Directory.Exists(Application.persistentDataPath + "/temp/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/temp/");
        }
        ExportModel(Application.persistentDataPath + "/temp/" + modelName + ".obj");
        SendMailMessage(from, to, dataType);
    }
}
