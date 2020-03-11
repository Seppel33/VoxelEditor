using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveEditableModel(string path, Vector3Int dimensions, int actionsQuantity, int timeTaken, bool selectedLastColor, Color mainColor, GameObject colorPicker)
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

        ModelData data = new ModelData(dimensions, actionsQuantity, timeTaken, selectedLastColor, mainColor, colorPicker);

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
    public static void DeleteFile(string name)
    {
        System.IO.File.Delete(Application.persistentDataPath + "/models/" + name + ".vx");
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
    public static void ExportModelToObj(string path, GameObject voxelModel, float scale, GameObject checker, out bool executed)
    {
        MeshHandler meshHandler = new MeshHandler();
        if (voxelModel.GetComponentsInChildren<MeshFilter>() != null)
        {
            GameObject fullMeshObject = meshHandler.CreateTempModel(voxelModel);
            if (meshHandler.PrepareMesh(ref fullMeshObject, checker))
            {
                ObjExporter.MeshToFile(fullMeshObject, path, scale);
                executed = true;
            }
            else
            {
                executed = false;
            }
            GameObject.Destroy(fullMeshObject);
        }
        else
        {
            executed = false;
        }
    }
    public static Settings LoadSettings()
    {
        string path = Application.persistentDataPath + "/data/settings.bin";

        if (File.Exists(path))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                Settings data = formatter.Deserialize(stream) as Settings;

                stream.Close();
                return data;
            }
            catch
            {
                Debug.LogError("Settings could not be loaded");
                Settings settings = new Settings();
                return settings;
            }
        }
        else
        {
            Settings settings = new Settings();
            return settings;
        }
    }
    public static void SaveSettings(Settings settings)
    {
        try
        {
            if (!Directory.Exists(Application.persistentDataPath + "/data"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/data");
            }

        }
        catch (IOException ex)
        {
            Debug.LogError(ex.Message);
        }

        string path = Application.persistentDataPath + "/data/settings.bin";

        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, settings);

        Debug.Log("Settings saved under: " + path);
        stream.Close();
    }
    public static void CreateExampleData()
    {
        TextAsset asset = Resources.Load<TextAsset>("ExampleHarvester");
        Stream s = new MemoryStream(asset.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        ModelData modelData = formatter.Deserialize(s) as ModelData;

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
        
        FileStream stream = new FileStream(Application.persistentDataPath + "/models/ExampleHarvester.vx", FileMode.Create);

        formatter.Serialize(stream, modelData);

        stream.Close();
    }
}