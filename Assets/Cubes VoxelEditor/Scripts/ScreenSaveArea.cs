using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSaveArea : MonoBehaviour
{
    public RectTransform Panel;
    private Rect LastSafeArea = new Rect(0, 0, 0, 0);

    // Start is called before the first frame update
    void Awake()
    {
        Panel = GetComponent<RectTransform>();
        Refresh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Refresh()
    {
        Rect safeArea = Screen.safeArea;

        //safeArea.width = safeArea.width - 90;
        if (safeArea != LastSafeArea)
        {
            ApplySafeArea(safeArea);
        }
    }
    private void ApplySafeArea(Rect r)
    {
        LastSafeArea = r;
        //r.position = new Vector2(r.position.x + 90, r.position.y);

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        //Debug.Log(anchorMin + " " + anchorMax);
        //anchorMin.x = 0.05f;
        Debug.Log(anchorMin + " " + anchorMax);

        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;

        Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
    }
}
