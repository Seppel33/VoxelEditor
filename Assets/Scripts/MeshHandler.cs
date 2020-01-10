using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHandler
{
    private GameObject model;
    public bool PrepareMesh(ref GameObject fullMesh)
    {
        OptimizeMesh();
        AssignStandardMaterials();
        bool b = CombineMeshes();
        fullMesh = model;
        return b;
    }
    private void OptimizeMesh()
    {
        for (int i = 0; i < model.transform.childCount; i++)
        {
            bool[] neighbor = new bool[6];
            int neighbors = 0;

            Vector3Int objPos = new Vector3Int((int)model.transform.GetChild(i).transform.position.x, (int)model.transform.GetChild(i).transform.position.y, (int)model.transform.GetChild(i).transform.position.z);
            if (objPos.y < SceneController.dimensions.y-1)
            {
                if (SceneController.gridOfObjects[objPos.x + SceneController.dimensions.x / 2, objPos.y + 1, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[0] = true;
                    neighbors++;
                }
            }
            if (objPos.y > 0)
            {
                if (SceneController.gridOfObjects[objPos.x + SceneController.dimensions.x / 2, objPos.y - 1, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[1] = true;
                    neighbors++;
                }
            }
            if (objPos.x < SceneController.dimensions.x / 2)
            {
                if (SceneController.gridOfObjects[objPos.x + 1 + SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[2] = true;
                    neighbors++;
                }
            }
            if (objPos.x > -SceneController.dimensions.x / 2)
            {
                if (SceneController.gridOfObjects[objPos.x - 1 + SceneController.dimensions.x / 2, objPos.y, objPos.z + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[3] = true;
                    neighbors++;
                }
            }
            if (objPos.z < SceneController.dimensions.z / 2)
            {
                if (SceneController.gridOfObjects[objPos.x + SceneController.dimensions.x / 2, objPos.y, objPos.z + 1 + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[4] = true;
                    neighbors++;
                }
            }
            if (objPos.z > -SceneController.dimensions.z / 2)
            {
                if (SceneController.gridOfObjects[objPos.x + SceneController.dimensions.x / 2, objPos.y, objPos.z - 1 + SceneController.dimensions.y / 2] != null)
                {
                    neighbor[5] = true;
                    neighbors++;
                }
            }
            if (neighbors == 6)
            {
                model.transform.GetChild(i).gameObject.SetActive(false);
            }
            
            else if (neighbors > 0)
            {
                /*
                Mesh mesh = model.transform.GetChild(i).transform.GetComponent<MeshFilter>().mesh;
                int[] oldTriangles = mesh.triangles;
                int[] newTriangles = new int[(mesh.triangles.Length-neighbors*2*3)];
                int j = 0;
                int k = 0;
                while (j < oldTriangles.Length-2)
                {
                    Vector3 normal = mesh.normals[oldTriangles[j]];
                    if ((normal == Vector3.up && neighbor[0])|| (normal == Vector3.down && neighbor[1])|| (normal == Vector3.right && neighbor[2])|| (normal == Vector3.left && neighbor[3])|| (normal == Vector3.forward && neighbor[4])|| (normal == Vector3.back && neighbor[5]))
                    {
                        newTriangles[k++] = oldTriangles[j];
                        newTriangles[k++] = oldTriangles[j+1];
                        newTriangles[k++] = oldTriangles[j+2];
                    }
                    j += 3;
                }
                mesh.triangles = newTriangles;
                */
            }
            
        }
    }
    private void AssignStandardMaterials()
    {
        Material standardMat = Resources.Load("Standard", typeof(Material)) as Material;
        foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>())
        {
            Color c= renderer.material.GetColor("Color_E5F6C120");
            renderer.sharedMaterial = standardMat;
            renderer.material.SetColor("_BaseColor", c);
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
                    if (localMat.GetColor("_BaseColor").Equals(color)){
                        containsInList = true;
                    }
                }
                if (!containsInList)
                {
                    materials.Add(localMat);
                    colors.Add(localMat.GetColor("_BaseColor"));
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
                if (!renderer.material.GetColor("_BaseColor").Equals(material.GetColor("_BaseColor")))
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
    public void DestroyTempModel()
    {
        UnityEngine.Object.Destroy(model);
    }
    public GameObject CreateTempModel(GameObject voxelModel)
    {
        model = UnityEngine.Object.Instantiate(voxelModel);
        return model;
    }
}
