using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeAspectRatioController : MonoBehaviour
{

#if UNITY_STANDALONE
    private int _lastWidth = 0;
    private int _lastHeight = 0;

    void Update()
    {
        var width = Screen.width;
        var height = Screen.height;

        if (_lastWidth != width) // if the user is changing the width
        {
            // update the height
            float heightAccordingToWidth = width / 16.0f * 9.0f;
            Screen.SetResolution(width, (int)Mathf.Round(heightAccordingToWidth), false, 0);
        }
        else if (_lastHeight != height) // if the user is changing the height
        {
            // update the width
            float widthAccordingToHeight = height / 9.0f * 16.0f;
            Screen.SetResolution((int)Mathf.Round(widthAccordingToHeight), height, false, 0);
        }

        _lastWidth = width;
        _lastHeight = height;
    }
#endif
}