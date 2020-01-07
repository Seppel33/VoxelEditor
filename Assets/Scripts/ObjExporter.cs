using UnityEngine;
using System.IO;
using System.Text;

public class ObjExporter
{

    private static string MeshToString(MeshFilter mf, Renderer rend, string filename)
    {
        Mesh m = mf.mesh;
        Material[] mats = rend.sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("mtllib ").Append(Path.ChangeExtension(filename, ".mtl")).Append("\n");
        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z).Replace(",", "."));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y).Replace(",", "."));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z).Replace(",","."));
        }
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(ColorUtility.ToHtmlStringRGB(mats[material].color)).Append("\n");
            sb.Append("usemap ").Append(ColorUtility.ToHtmlStringRGB(mats[material].color)).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }
        return sb.ToString();
    }
    private static string MaterialsToString(Renderer rend)
    {
        Material[] mats = rend.materials;

        StringBuilder sb = new StringBuilder();

        foreach(Material mat in mats)
        {
            sb.Append("newmtl ").Append(ColorUtility.ToHtmlStringRGB(mat.color)).Append("\n");
            string color = mat.color.r + " " + mat.color.g + " " + mat.color.b;
            color.Replace(",", ".");
            sb.Append("Ka").Append(color).Append("\n");
            sb.Append("Kd 0.0000 1.0000 0.0000").Append("\n");
            sb.Append("illum 1").Append("\n").Append("\n");
        }
        

        return sb.ToString();
    }

    public static void MeshToFile(GameObject objectToExport, string filename)
    {
        
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(objectToExport.GetComponent<MeshFilter>(), objectToExport.GetComponent<Renderer>(), filename));
        }
        filename = Path.ChangeExtension(filename, ".mtl");
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MaterialsToString(objectToExport.GetComponent<Renderer>()));
        }
    }
}

