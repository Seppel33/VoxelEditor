using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject combinedMesh;
    public GameObject voxelModel;
    private bool m_editDone = false;
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
            if(debugInfos.activeSelf)
            {
                fps.text = (int)(1.0 / Time.smoothDeltaTime) + " FPS";
            }
            else
            {
                debugInfos.SetActive(true);
            }
        }else if (debugInfos.activeSelf)
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
            combinedMesh.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            combinedMesh.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
            combinedMesh.transform.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            combinedMesh.transform.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            combinedMesh.transform.GetComponent<MeshFilter>().mesh.Optimize();

            combinedMesh.transform.gameObject.SetActive(true);
            m_editDone = true;
            doneButton.SetActive(false);
            SceneManager.LoadScene("MainScene");
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
