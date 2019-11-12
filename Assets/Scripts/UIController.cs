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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode)
        {
            if(fps.enabled)
            {
                fps.text = (int)(1.0 / Time.deltaTime) + " FPS / " + Time.deltaTime + " Sec";
            }
            else
            {
                fps.enabled = true;
            }
        }else if (fps.enabled)
        {
            fps.enabled = false;
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
    public void SetupIOSUI()
    {

    }
    public void SetDebugMode(bool debugMode)
    {
        this.debugMode = debugMode;
    }
    public void Undo()
    {

        if (SceneController.m_lastActions.Count != 0)
        {
            SceneController.m_lastUndos.Add(SceneController.m_lastActions[SceneController.m_lastActions.Count - 1]);
            if (SceneController.m_lastUndos.Count > 10)
            {
                if ((int)SceneController.m_lastUndos[0][0] == 0)
                {
                    Destroy((GameObject)SceneController.m_lastUndos[0][1]);
                }
                SceneController.m_lastUndos.RemoveAt(0);
            }
            if((int)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][0] == 0)
            {
                ((GameObject)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][1]).SetActive(false);
            }else if((int)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][0] == 1)
            {
                ((GameObject)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][1]).SetActive(true);
            }
            
            SceneController.m_lastActions.RemoveAt(SceneController.m_lastActions.Count - 1);

        }
    }
    public void Redo()
    {
        if(SceneController.m_lastUndos.Count != 0)
        {
            SceneController.m_lastActions.Add(SceneController.m_lastUndos[SceneController.m_lastUndos.Count - 1]);
            if (SceneController.m_lastActions.Count > 10)
            {
                if ((int)SceneController.m_lastActions[0][0] == 1)
                {
                    Destroy((GameObject)SceneController.m_lastActions[0][1]);
                }
                SceneController.m_lastActions.RemoveAt(0);
            }
            if ((int)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][0] == 0)
            {
                ((GameObject)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][1]).SetActive(true);
            }
            else if ((int)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][0] == 1)
            {
                ((GameObject)SceneController.m_lastActions[SceneController.m_lastActions.Count - 1][1]).SetActive(false);
            }
            SceneController.m_lastUndos.RemoveAt(SceneController.m_lastUndos.Count - 1);
        }
    }
}
