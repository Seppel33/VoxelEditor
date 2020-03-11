using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshHandler
{
    private GameObject model;
    private int iteration = 0;
    public GameObject checker;
    public bool PrepareMesh(ref GameObject fullMesh, GameObject checker)
    {
        this.checker = checker;
        OptimizeMesh();
        AssignStandardMaterials();
        bool b = CombineMeshes();
        fullMesh = model;
        return b;
    }
    private void OptimizeMesh()
    {
        bool[,,] gridOfObjectChecks = new bool[SceneController.dimensions.x, SceneController.dimensions.y, SceneController.dimensions.z];
        bool[,,] gridOfObjectOverwrittenChecks = new bool[SceneController.dimensions.x, SceneController.dimensions.y, SceneController.dimensions.z];
        for (int i = 0; i< SceneController.dimensions.x; i++)
        {
            for (int h = 0; h < SceneController.dimensions.y; h++)
            {
                for (int k = 0; k < SceneController.dimensions.z; k++)
                {
                    if(SceneController.gridOfObjects[i,h,k] != null)
                    {
                        gridOfObjectChecks[i, h, k] = true;
                    }
                }
            }
        }
        for (int i = 0; i < model.transform.childCount; i++)
        {
            //check for neighbors
            bool[] neighbor = new bool[6];
            int neighbors = 0;

            Vector3Int objPos = new Vector3Int((int)model.transform.GetChild(i).transform.position.x, (int)model.transform.GetChild(i).transform.position.y, (int)model.transform.GetChild(i).transform.position.z);
            if (objPos.y < SceneController.dimensions.y-1)
            {
                if (gridOfObjectChecks[objPos.x + SceneController.dimensions.x / 2, objPos.y + 1, objPos.z + SceneController.dimensions.y / 2])
                {
                    neighbor[0] = true;
                    neighbors++;
                }
            }
            if (objPos.y > 0)
            {
                if (gridOfObjectChecks[objPos.x + SceneController.dimensions.x / 2, objPos.y - 1, objPos.z + SceneController.dimensions.y / 2])
                {
                    neighbor[1] = true;
                    neighbors++;
                }
            }
            if (objPos.x < SceneController.dimensions.x / 2)
            {
                if (gridOfObjectChecks[objPos.x + 1 + SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2])
                {
                    neighbor[2] = true;
                    neighbors++;
                }
            }
            if (objPos.x > -SceneController.dimensions.x / 2)
            {
                if (gridOfObjectChecks[objPos.x - 1 + SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2])
                {
                    neighbor[3] = true;
                    neighbors++;
                }
            }
            if (objPos.z < SceneController.dimensions.z / 2)
            {
                if (gridOfObjectChecks[objPos.x + SceneController.dimensions.x / 2, objPos.y, objPos.z + 1 + SceneController.dimensions.y / 2])
                {
                    neighbor[4] = true;
                    neighbors++;
                }
            }
            if (objPos.z > -SceneController.dimensions.z / 2)
            {
                if (gridOfObjectChecks[objPos.x + SceneController.dimensions.x / 2, objPos.y, objPos.z - 1 + SceneController.dimensions.y / 2])
                {
                    neighbor[5] = true;
                    neighbors++;
                }
            }
            //check if empty neighbor is enclosed and could be counted as filled
            if (neighbors < 6)
            {
                //GameObject c = UnityEngine.GameObject.Instantiate(checker);
                //c.GetComponent<AlgorithmVisualise>().SetData(gridOfObjectChecks, neighbor, gridOfObjectOverwrittenChecks, objPos, neighbors, iteration);

                
                //iteration++;
                /*
                bool[,,] tempGridOfObjectChecks = gridOfObjectChecks;
                List<Vector3Int> checkedBlocks = new List<Vector3Int>();
                for (int t = 0; t < neighbor.Length; t++)
                {
                    if (!neighbor[t])
                    {
                        bool newNeighbor = false;
                        switch (t)
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
                            neighbor[t] = true;
                            neighbors++;
                        }
                    }
                }*/
            }
            //if all neighbors it can't be seen so delete it
            if (neighbors == 6)
            {
                model.transform.GetChild(i).gameObject.SetActive(false);
            }
            //Remove faces facing neighbors
            else if (neighbors > 0)
            {
                //delete triangles
                Mesh mesh = model.transform.GetChild(i).transform.GetComponent<MeshFilter>().mesh;
                int[] oldTriangles = mesh.triangles;
                int[] newTriangles = new int[oldTriangles.Length - neighbors*2*3];
                int j = 0;
                int k = 0;
                bool[] flaggedForDelete = new bool[mesh.vertexCount];
                while (j < oldTriangles.Length - 2)
                {
                    Vector3 normal = mesh.normals[oldTriangles[j]];
                    if ((normal == Vector3.up && neighbor[0]) || (normal == Vector3.down && neighbor[1]) || (normal == Vector3.right && neighbor[2]) || (normal == Vector3.left && neighbor[3]) || (normal == Vector3.forward && neighbor[4]) || (normal == Vector3.back && neighbor[5]))
                    {
                        flaggedForDelete[oldTriangles[j]] = true;
                        flaggedForDelete[oldTriangles[j+1]] = true;
                        flaggedForDelete[oldTriangles[j+2]] = true;

                        j += 3;
                    }
                    else
                    {
                        newTriangles[k++] = oldTriangles[j];
                        newTriangles[k++] = oldTriangles[j + 1];
                        newTriangles[k++] = oldTriangles[j + 2];
                        j += 3;
                    }
                }
                //delete verts and rearrange triangles
                /*
                List<Vector3> newVerts = new List<Vector3>();
                List<Vector3> newNormals = new List<Vector3>();
                List<Vector2> newUV = new List<Vector2>();
                int correction = 0;
                for (int m =0; m< flaggedForDelete.Length; m++)
                {
                    
                    if (!flaggedForDelete[m])
                    {
                        newVerts.Add(mesh.vertices[m]);
                        newNormals.Add(mesh.normals[m]);
                        newUV.Add(mesh.uv[m]);
                    }
                    else
                    {
                        for(int n = 0; n<newTriangles.Length; n++)
                        {
                            if (newTriangles[n] > m-correction)
                            {
                                newTriangles[n] -= 1;
                            }
                        }
                        correction++;
                    }
                }
                mesh.vertices = newVerts.ToArray();
                mesh.normals = newNormals.ToArray();
                mesh.uv = newUV.ToArray();
                */

                mesh.triangles = newTriangles;
                //mesh.RecalculateNormals();
            }
        }
    }
    private bool[,,] CalculateNewList(bool[,,] tempGridOfObjectChecks, bool[,,] gridOfOverwrittenChecks, Vector3Int position, List<Vector3Int> checkedBlocks, out bool newNeighbor)
    {
        bool allInside = true;
        RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, position, checkedBlocks, allInside);

        foreach (Vector3Int v in checkedBlocks)
        {
            tempGridOfObjectChecks[v.x, v.y, v.z] = allInside;
        }
        newNeighbor = allInside;
        return tempGridOfObjectChecks;
    }
    private bool RecursiveCheck(bool[,,] tempGridOfObjectChecks, bool[,,] gridOfOverwrittenChecks, Vector3Int position, List<Vector3Int> checkedBlocks, bool allInside)
    {
        Debug.Log("Checked Position: " + position);
        if (tempGridOfObjectChecks[position.x, position.y, position.z] == false)
        {
            if (gridOfOverwrittenChecks[position.x, position.y, position.z] == false)
            {
                checkedBlocks.Add(new Vector3Int(position.x, position.y, position.z));
                gridOfOverwrittenChecks[position.x, position.y, position.z] = true;
                if(position.x == SceneController.dimensions.x / 2 || position.x == -SceneController.dimensions.x / 2 || position.y == 0 || position.y == SceneController.dimensions.y || position.z == SceneController.dimensions.z / 2 || position.z == -SceneController.dimensions.z / 2)
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
                                RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y + 1, position.z), checkedBlocks, allInside);
                                break;
                            case 1:
                                RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y - 1, position.z), checkedBlocks, allInside);
                                break;
                            case 2:
                                RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x + 1, position.y, position.z), checkedBlocks, allInside);
                                break;
                            case 3:
                                RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x - 1, position.y, position.z), checkedBlocks, allInside);
                                break;
                            case 4:
                                RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y, position.z + 1), checkedBlocks, allInside);
                                break;
                            case 5:
                                RecursiveCheck(tempGridOfObjectChecks, gridOfOverwrittenChecks, new Vector3Int(position.x, position.y, position.z - 1), checkedBlocks, allInside);
                                break;
                        }

                    }
                }
            }
        }
        return allInside;
    }
    private void AssignStandardMaterials()
    {
        Material standardMat = Resources.Load("Standard", typeof(Material)) as Material;
        foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>())
        {
            Color c= renderer.material.GetColor("_Color");
            renderer.sharedMaterial = standardMat;
            renderer.material.SetColor("_Color", c);
        }
    }
    public bool CombineMeshes()
    {
        MeshFilter[] filters = model.GetComponentsInChildren<MeshFilter>(false);

        List<Material> materials = new List<Material>();
        List<Color> colors = new List<Color>();
        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(false);
        foreach(MeshRenderer renderer in renderers)
        {
            if (renderer.transform == model.transform)
                continue;
            Material[] localMats = renderer.materials;
            foreach (Material localMat in localMats)
            {
                bool containsInList = false;
                foreach (Color color in colors)
                {
                    if (localMat.GetColor("_Color").Equals(color)){
                        containsInList = true;
                    }
                }
                if (!containsInList)
                {
                    materials.Add(localMat);
                    colors.Add(localMat.GetColor("_Color"));
                }
                    
            }
        }

        List<Mesh> submeshes = new List<Mesh>();
        foreach (Material material in materials)
        {
            List<CombineInstance> combiners = new List<CombineInstance>();
            foreach(MeshFilter filter in filters)
            {
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if(renderer == null)
                {
                    Debug.LogError(filter.name + "has no MeshRenderer");
                    continue;
                }
                if (!renderer.material.GetColor("_Color").Equals(material.GetColor("_Color")))
                    continue;
                CombineInstance ci = new CombineInstance();
                ci.mesh = filter.sharedMesh;
                ci.subMeshIndex = 0;
                ci.transform = filter.transform.localToWorldMatrix;
                combiners.Add(ci);
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combiners.ToArray(),true);
            submeshes.Add(mesh);
        }
        List<CombineInstance> finalCombiners = new List<CombineInstance>();
        foreach(Mesh mesh in submeshes)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            ci.subMeshIndex = 0;
            ci.transform = Matrix4x4.identity;
            finalCombiners.Add(ci);
        }
        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
        if (finalMesh.vertexCount > 64000)
        {
            Debug.LogError("Mesh hast over 64k vertecies");
            return false;
        }
        model.AddComponent<MeshFilter>();
        model.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        model.AddComponent<MeshRenderer>();
        model.GetComponent<MeshRenderer>().materials = materials.ToArray();
        Debug.Log("Final mesh has " + submeshes.Count + " materials and " + finalMesh.vertexCount + " vertecies");
        return true;
    }
    public GameObject CreateTempModel(GameObject voxelModel)
    {
        model = UnityEngine.Object.Instantiate(voxelModel);
        return model;
    }
    public void ShowVerts()
    {
        Vector3[] vertices = model.GetComponent<MeshFilter>().mesh.vertices;
        GameObject[] spheres = vertices.Select(vert =>
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = vert;
            return sphere;
        }).ToArray();
    }
}
