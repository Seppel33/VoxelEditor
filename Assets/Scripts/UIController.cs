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
    public GameObject doneButton;
    private bool debugMode = false;
    public Text fps;
    public Text operatingSystem;
    public Text monitor;

    private Vector3 originalEulers;
    private Vector3 transformEulers;
    private int remainingFrames;
    public int timeInFrames = 100;
    public GameObject debugInfos;
    private bool refocusAnimation = false;

    public int selectedState = 0;


    // Start is called before the first frame update
    void Start()
    {
        operatingSystem.text = SystemInfo.operatingSystem;
        


        Debug.Log("dc: " + Display.displays.Length + " ss: " + Input.stylusTouchSupported + " ts: " + Input.touchSupported);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        monitor.text = " Displays: " + Display.displays.Length + " " + Input.stylusTouchSupported + " " + Input.touchSupported + " " + Input.touchCount;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode)
        {
            if (debugInfos.activeSelf)
            {
                fps.text = (int)(1.0 / Time.smoothDeltaTime) + " FPS";
                monitor.text = Display.displays[Display.displays.Length - 1].ToString() + " Number: " + Display.displays.Length + " " + Input.stylusTouchSupported + " " + Input.touchSupported + " " + Input.touchCount;
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
        if (refocusAnimation)
        {
            if (remainingFrames > 0)
            {
                voxelModel.transform.eulerAngles += transformEulers;
                remainingFrames--;
            }
            else
            {
                refocusAnimation = false;
                remainingFrames = timeInFrames;
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
            doneButton.SetActive(false);



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



    public void ResetRotation()
    {
        if (!refocusAnimation)
        {
            transformEulers = -1 * voxelModel.transform.eulerAngles / timeInFrames;
            refocusAnimation = true;
        }

    }

    public void setSelectedState(int state)
    {
        selectedState = state;
    }

}
