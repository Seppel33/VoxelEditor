using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UndoRedo : MonoBehaviour
{
    private List<ArrayList> m_lastActions = new List<ArrayList>();
    public int maxUndo = 30;
    private int undoPosition = 0;
    public Button undoButton;
    public Button redoButton;
    public bool unsavedChanges;

    public void Undo()
    {
        if(undoPosition != 0)
        {
            decreaseUndoPosition();

            if((int)m_lastActions[undoPosition][0] == 0)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][1])[i];
                    cube.SetActive(false);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = null;
                }
            }
            else if ((int)m_lastActions[undoPosition][0] == 1)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][1])[i];
                    cube.SetActive(true);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = cube;
                }
            }
            else if ((int)m_lastActions[undoPosition][0] == 2)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][1])[i];
                    cube.SetActive(false);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = null;
                }
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][2]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][2])[i];
                    cube.SetActive(true);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = cube;
                }
            }else if((int)m_lastActions[undoPosition][0] == 3)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    Renderer rend = ((GameObject)((ArrayList)m_lastActions[undoPosition][1])[i]).transform.GetComponent<Renderer>();
                    rend.material.shader = Shader.Find("Shader Graphs/Blocks");
                    rend.material.SetColor("Color_E5F6C120", (Color)((ArrayList)m_lastActions[undoPosition][3])[i]);
                }
            }
            redoButton.interactable = true;
            if(undoPosition == 0)
            {
                undoButton.interactable = false;
            }
        }
    }

    public void Redo()
    {
        if(undoPosition < m_lastActions.Count)
        {
            

            if ((int)m_lastActions[undoPosition][0] == 0)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][1])[i];
                    cube.SetActive(true);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = cube;
                }
            }
            else if ((int)m_lastActions[undoPosition][0] == 1)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][1])[i];
                    cube.SetActive(false);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = null;
                }
            }
            else if ((int)m_lastActions[undoPosition][0] == 2)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][2]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][2])[i];
                    cube.SetActive(false);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = null;
                }
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    GameObject cube = (GameObject)((ArrayList)m_lastActions[undoPosition][1])[i];
                    cube.SetActive(true);
                    SceneController.gridOfObjects[(int)cube.transform.position.x + (int)(SceneController.dimensions.x / 2), (int)cube.transform.position.y, (int)cube.transform.position.z + (int)(SceneController.dimensions.z / 2)] = cube;
                }
            }else if((int)m_lastActions[undoPosition][0] == 3)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[undoPosition][1]).Count; i++)
                {
                    Renderer rend = ((GameObject)((ArrayList)m_lastActions[undoPosition][1])[i]).transform.GetComponent<Renderer>();
                    rend.material.shader = Shader.Find("Shader Graphs/Blocks");
                    rend.material.SetColor("Color_E5F6C120", (Color)m_lastActions[undoPosition][2]);
                }
            }
            increaseUndoPosition();
            undoButton.interactable = true;
            if (undoPosition == m_lastActions.Count)
            {
                redoButton.interactable = false;
            }
        }
    }

    public void addAction(ArrayList action)
    {
        resetRedo();
        m_lastActions.Add(action);
        if (m_lastActions.Count > maxUndo)
        {
            /*
            if((int)m_lastActions[0][0] == 1)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[0][1]).Count; i++)
                {
                    Destroy((GameObject)((ArrayList)m_lastActions[0][1])[i]);
                }
            }
            else if((int)m_lastActions[0][0] == 2)
            {
                for (int i = 0; i < ((ArrayList)m_lastActions[0][1]).Count; i++)
                {
                    if (!((GameObject)((ArrayList)m_lastActions[0][1])[i]).activeInHierarchy)
                    {
                        Destroy((GameObject)((ArrayList)m_lastActions[0][1])[i]);
                    } 
                }
                for (int i = 0; i < ((ArrayList)m_lastActions[0][2]).Count; i++)
                {
                    if (!((GameObject)((ArrayList)m_lastActions[0][2])[i]).activeInHierarchy)
                    {
                        Destroy((GameObject)((ArrayList)m_lastActions[0][2])[i]);
                    }
                }
            }*/
            m_lastActions.RemoveAt(0);
        }
        increaseUndoPosition();
        unsavedChanges = true;
        undoButton.interactable = true;
    }

    private void increaseUndoPosition()
    {
        if(undoPosition < maxUndo)
        {
            undoPosition++;
        }
    }
    private void decreaseUndoPosition()
    {
        if (undoPosition > 0)
        {
            undoPosition--;
        }
    }
    private void resetRedo()
    {
        for(int i = m_lastActions.Count-1; i>= undoPosition; i--)
        {
            if((int)m_lastActions[i][0] == 0 || (int)m_lastActions[i][0] == 2)
            {
                for (int k = 0; k < ((ArrayList)m_lastActions[i][1]).Count; k++)
                {
                    //Destroy((GameObject)((ArrayList)m_lastActions[i][1])[k]);
                }
            }
            m_lastActions.RemoveAt(i);
        }
        redoButton.interactable = false;
    }
    public void resetList()
    {
        unsavedChanges = false;
        undoPosition = 0;
        m_lastActions = new List<ArrayList>();
    }

}
