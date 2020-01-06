using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHandler : MonoBehaviour
{
    [SerializeField]
    private Material standardMaterial;
    public GameObject OptimizeMesh(GameObject voxelModel)
    {
        GameObject model = voxelModel;
        for (int i = 0; i < model.transform.childCount; i++)
        {
            bool[] neighbor = new bool[6];
            int neighbors = 0;

            Vector3Int objPos = new Vector3Int((int)model.transform.GetChild(i).transform.position.x, (int)model.transform.GetChild(i).transform.position.y, (int)model.transform.GetChild(i).transform.position.z);
            if (objPos.y < SceneController.dimensions.y)
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
                UnityEngine.Object.Destroy(model.transform.GetChild(i));
            }
            /*
            else if (neighbors > 0)
            {
                Mesh mesh = voxelModel.transform.GetChild(i).transform.GetComponent<MeshFilter>().mesh;
                int[] oldTriangles = mesh.triangles;
                int[] newTriangles = new int[mesh.triangles.Length - neighbors * 2 * 3];

                HashSet<int> indices = new HashSet<int>(mesh.triangles.AsEnumerable().Distinct().Where(index =>
                mesh.normals[index] == Vector3.up
                ).ToList());
                int counter = 0;
                foreach (int h in indices)
                {
                    newTriangles[counter++] = h;
                }
            }
            */
        }
        return model;
    }
    public void AsignStandardMaterials(GameObject model)
    {
        foreach(MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>())
        {
            Color c= renderer.material.GetColor("Color_E5F6C120");
            renderer.sharedMaterial = standardMaterial;
            renderer.sharedMaterial.SetColor("_BaseColor", c);
        }
    }
    public void CombineMeshes(GameObject model)
    {
        MeshFilter[] filters = model.GetComponentsInChildren<MeshFilter>(false);

        List<Material> materials = new List<Material>();
        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(false);
        foreach(MeshRenderer renderer in renderers)
        {
            if (renderer.transform == model.transform)
                continue;
            Material[] localMats = renderer.sharedMaterials;
            foreach (Material localMat in localMats)
                if (!materials.Contains(localMat))
                    materials.Add(localMat);
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

                Material[] localMaterials = renderer.sharedMaterials;
                for(int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
                {
                    if (localMaterials[materialIndex] != material)
                        continue;
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = filter.sharedMesh;
                    ci.subMeshIndex = materialIndex;
                    ci.transform = filter.transform.localToWorldMatrix;
                    combiners.Add(ci);
                }
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
        model.AddComponent<MeshFilter>();
        model.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        model.AddComponent<MeshRenderer>();
        model.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        Debug.Log("Final mesh has " + submeshes.Count + " materials");
    }
}
