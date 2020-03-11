using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmVisualise : MonoBehaviour
{
    bool[,,] gridOfObjectChecks;
    bool[] neighbor;
    bool[,,] gridOfObjectOverwrittenChecks;
    Vector3Int objPos;
    int neighbors;
    public GameObject vis;
    public static int globalIteration = 0;
    public static int sideCheck = 0;
    int iteration;
    bool dataSet = false;
    bool first;
    public static int lastSide = -1;

    public void SetData(bool[,,] gridOfObjectChecks, bool[] neighbor, bool[,,] gridOfObjectOverwrittenChecks, Vector3Int objPos, int neighbors, int iteration)
    {
        Debug.Log("AV Instance created" + iteration);
        this.gridOfObjectChecks = gridOfObjectChecks;
        this.neighbor = neighbor;
        this.gridOfObjectOverwrittenChecks = gridOfObjectOverwrittenChecks;
        this.objPos = objPos;
        this.neighbors = neighbors;
        this.iteration = iteration;
        dataSet = true;
        first = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (dataSet && first)
        {
            if (globalIteration == iteration)
            {
                bool[,,] tempGridOfObjectChecks = gridOfObjectChecks;
                List<Vector3Int> checkedBlocks = new List<Vector3Int>();
                if (lastSide != sideCheck)
                {
                    Debug.Log(globalIteration + " " + iteration);
                    lastSide++;
                    if (!neighbor[sideCheck])
                    {
                        bool newNeighbor = false;
                        switch (sideCheck)
                        {
                            case 0:
                                if (objPos.y < SceneController.dimensions.y)
                                    gridOfObjectChecks = CalculateNewList(tempGridOfObjectChecks, gridOfObjectOverwrittenChecks, new Vector3Int(objPos.x + SceneController.dimensions.x / 2, objPos.y + 1, objPos.z + SceneController.dimensions.y / 2), checkedBlocks, out newNeighbor);
                                break;
                            case 1:
                                if (objPos.y > 0)
                                    gridOfObjectChecks = CalculateNewList(tempGridOfObjectChecks, gridOfObjectOverwrittenChecks, new Vector3Int(objPos.x + SceneController.dimensions.x / 2, objPos.y - 1, objPos.z + SceneController.dimensions.y / 2), checkedBlocks, out newNeighbor);
                                break;
                            case 2:
                                if (objPos.x < SceneController.dimensions.x / 2)
                                    gridOfObjectChecks = CalculateNewList(tempGridOfObjectChecks, gridOfObjectOverwrittenChecks, new Vector3Int(objPos.x + 1 + SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2), checkedBlocks, out newNeighbor);
                                break;
                            case 3:
                                if (objPos.x > -SceneController.dimensions.x / 2)
                                    gridOfObjectChecks = CalculateNewList(tempGridOfObjectChecks, gridOfObjectOverwrittenChecks, new Vector3Int(objPos.x - 1 + SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2), checkedBlocks, out newNeighbor);
                                break;
                            case 4:
                                if (objPos.z < SceneController.dimensions.z / 2)
                                    gridOfObjectChecks = CalculateNewList(tempGridOfObjectChecks, gridOfObjectOverwrittenChecks, new Vector3Int(objPos.x + SceneController.dimensions.x / 2, objPos.y, objPos.z + 1 + SceneController.dimensions.y / 2), checkedBlocks, out newNeighbor);
                                break;
                            case 5:
                                if (objPos.z > -SceneController.dimensions.z / 2)
                                    gridOfObjectChecks = CalculateNewList(tempGridOfObjectChecks, gridOfObjectOverwrittenChecks, new Vector3Int(objPos.x + SceneController.dimensions.x / 2, objPos.y, objPos.z - 1 + SceneController.dimensions.y / 2), checkedBlocks, out newNeighbor);
                                break;
                        }
                        if (newNeighbor)
                        {
                            neighbor[sideCheck] = true;
                            neighbors++;
                        }
                    }
                }
                if(sideCheck >= 5)
                {
                    first = false;
                }
            }
        }
    }
    private bool[,,] CalculateNewList(bool[,,] tempGridOfObjectChecks, bool[,,] gridOfOverwrittenChecks, Vector3Int position, List<Vector3Int> checkedBlocks, out bool newNeighbor)
    {
        bool allInside = true;
        RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, position, checkedBlocks, allInside, new Vector3(1.1f,1.1f,1.1f));

        foreach (Vector3Int v in checkedBlocks)
        {
            tempGridOfObjectChecks[v.x, v.y, v.z] = allInside;
        }
        newNeighbor = allInside;
        return tempGridOfObjectChecks;
    }
    private bool RecursiveCheck(bool[,,] tempGridOfObjectChecks, bool[,,] gridOfOverwrittenChecks, Vector3Int position, List<Vector3Int> checkedBlocks, bool allInside, Vector3 scale)
    {
        if (allInside)
        {
            Debug.Log("Checked Position: " + position);
            scale = new Vector3(scale.x * 0.98f, scale.y * 0.98f, scale.z * 0.98f);
            Vector3 pos = new Vector3(position.x - (SceneController.dimensions.x / 2), position.y, position.z - (SceneController.dimensions.z / 2));
            GameObject v = Instantiate(vis, pos, Quaternion.identity);
            v.transform.localScale.Set(scale.x, scale.y, scale.z);
            if (tempGridOfObjectChecks[position.x, position.y, position.z] == false)
            {
                if (gridOfOverwrittenChecks[position.x, position.y, position.z] == false)
                {
                    checkedBlocks.Add(new Vector3Int(position.x, position.y, position.z));
                    gridOfOverwrittenChecks[position.x, position.y, position.z] = true;
                    if (position.x == 0 || position.x == SceneController.dimensions.x - 1 || position.y == 0 || position.y == SceneController.dimensions.y - 1 || position.z == 0 || position.z == SceneController.dimensions.z - 1)
                    {
                        allInside = false;
                    }
                    else
                    {
                        for (int t = 0; t < 6; t++)
                        {
                            switch (t)
                            {
                                case 0:
                                    RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y + 1, position.z), checkedBlocks, allInside, scale);
                                    break;
                                case 1:
                                    RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y - 1, position.z), checkedBlocks, allInside, scale);
                                    break;
                                case 2:
                                    RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x + 1, position.y, position.z), checkedBlocks, allInside, scale);
                                    break;
                                case 3:
                                    RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x - 1, position.y, position.z), checkedBlocks, allInside, scale);
                                    break;
                                case 4:
                                    RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y, position.z + 1), checkedBlocks, allInside, scale);
                                    break;
                                case 5:
                                    RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y, position.z - 1), checkedBlocks, allInside, scale);
                                    break;
                            }

                        }
                    }
                }
            }
        }
        return allInside;
    }
}
