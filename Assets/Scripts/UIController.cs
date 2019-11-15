using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class UIController : MonoBehaviour
{
    public GameObject combinedMesh;
    public GameObject voxelModel;
    public GameObject doneButton;
    private bool debugMode = false;
    public Text fps;
    public Text operatingSystem;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode)
        {
            if (debugInfos.activeSelf)
            {
                fps.text = (int)(1.0 / Time.smoothDeltaTime) + " FPS";
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
            combinedMesh = PrefabUtility.SaveAsPrefabAssetAndConnect(combinedMesh, "Assets/CustomModels/CombinedMesh.prefab", InteractionMode.AutomatedAction);
            MeshFilter mFilter = combinedMesh.AddComponent<MeshFilter>(); ;
            mFilter.sharedMesh = new Mesh();
            mFilter.sharedMesh.CombineMeshes(combine, true);
            mFilter.sharedMesh.RecalculateBounds();
            mFilter.sharedMesh.RecalculateNormals();
            mFilter.sharedMesh.Optimize();



            //combinedMesh.transform.gameObject.SetActive(true);
            doneButton.SetActive(false);



            MeshFilter m = (MeshFilter)Instantiate(combinedMesh.transform.GetComponent<MeshFilter>());
            AssetDatabase.CreateAsset(m.mesh, "Assets/CustomModels/MyMesh.asset");
            AssetDatabase.SaveAssets();
            combinedMesh.transform.GetComponent<MeshFilter>().mesh = m.mesh;

            //SceneManager.LoadScene("MainScene");
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
