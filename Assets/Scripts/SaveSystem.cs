using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveEditableModel(string path, Vector3Int dimensions, int actionsQuantity, int timeTaken)
    {
        try
        {
            if (!Directory.Exists(Application.persistentDataPath + "/models"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/models");
            }

        }
        catch (IOException ex)
        {
            Debug.LogError(ex.Message);
        }

        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(path, FileMode.Create);

        ModelData data = new ModelData(dimensions, actionsQuantity, timeTaken);

        formatter.Serialize(stream, data);

        Debug.Log("Saved under: " + path);
        stream.Close();
    }

    public static ModelData LoadEditableModel(string dataName)
    {
        string path = Application.persistentDataPath + "/models/" + dataName + ".vx";
        
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ModelData data = formatter.Deserialize(stream) as ModelData;

            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
    public static void DeleteFile()
    {
        
    }
    public static string ImportModel(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/models"))
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/models");
        }
        System.IO.File.Copy(path, Application.persistentDataPath + "/models/" + name +".vx", true);
        return name;
    }

}
