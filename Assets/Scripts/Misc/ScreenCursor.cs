using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    private void Awake()
    {
        // Set hardware cursor off
        Cursor.visible = false;
    }

    private void Update()
    {
        // Set cursor position to mouse position
        transform.position = Input.mousePosition;
    }
}
