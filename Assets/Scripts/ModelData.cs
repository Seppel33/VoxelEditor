using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ModelData
{
    public int[] dimensions;
    public float[,] colors;
    public bool[] blockThere;
    public int actions;
    public int timeTaken;
    public float[,] colorWheelColors;
    public bool lastColorPicked;
    public ModelData(Vector3Int dimensions, int actions, int seconds, bool selectedLastColor, Color mainColor, GameObject colorPicker)
    {
        this.actions = actions;
        timeTaken = seconds;
        this.dimensions = new int[3];
        this.dimensions[0] = dimensions.x;
        this.dimensions[1] = dimensions.y;
        this.dimensions[2] = dimensions.z;

        colors = new float[3, dimensions.x*dimensions.y*dimensions.z];
        blockThere = new bool[dimensions.x * dimensions.y * dimensions.z];
        int count = 0;
        for (int i = 0; i< dimensions[0]; i++)
        {
            for(int k = 0; k < dimensions[1]; k++)
            {
                for (int h = 0; h < dimensions[2]; h++)
                {
                    if(SceneController.gridOfObjects[i, k, h] != null)
                    {
                        blockThere[count] = true;
                        Color color = SceneController.gridOfObjects[i, k, h].transform.GetComponent<Renderer>().material.GetColor("Color_E5F6C120");
                        colors[0,count] = color.r;
                        colors[1, count] = color.g;
                        colors[2, count] = color.b;
                    }
                    count++;
                }
            }
        }
        lastColorPicked = selectedLastColor;
        colorWheelColors = new float[6, 3];
        colorWheelColors[0, 0] = mainColor.r;
        colorWheelColors[0, 1] = mainColor.g;
        colorWheelColors[0, 2] = mainColor.b;
        for (int i = 1; i< 6; i++)
        {
            colorWheelColors[i,0] = colorPicker.GetComponentsInChildren<Image>()[i].color.r;
            colorWheelColors[i, 1] = colorPicker.GetComponentsInChildren<Image>()[i].color.g;
            colorWheelColors[i, 2] = colorPicker.GetComponentsInChildren<Image>()[i].color.b;
        }
    }
    

}
