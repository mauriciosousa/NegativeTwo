using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHandCursor : MonoBehaviour {

    public Texture cursorTexture;
    public int textureSize = 10;

    public Vector2 leftHandPixel;
    public Vector2 rightHandPixel;

    void Start () {
        leftHandPixel = Vector2.positiveInfinity;
        rightHandPixel = Vector2.positiveInfinity;
	}

    void OnGUI()
    {
        if(!float.IsPositiveInfinity(leftHandPixel.x))
        {
            GUI.DrawTexture(new Rect(leftHandPixel.x - textureSize * 0.5f, Screen.height - leftHandPixel.y - textureSize * 0.5f, textureSize, textureSize), cursorTexture);
        }

        if (!float.IsPositiveInfinity(rightHandPixel.x))
        {
            GUI.DrawTexture(new Rect(rightHandPixel.x - textureSize * 0.5f, Screen.height - rightHandPixel.y - textureSize * 0.5f, textureSize, textureSize), cursorTexture);
        }
    }
}
