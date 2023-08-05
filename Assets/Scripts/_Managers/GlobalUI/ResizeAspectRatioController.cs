using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeAspectRatioController : MonoBehaviour
{

    public static Action OnScreenResize = null;
    private int _lastWidth = 0;
    private int _lastHeight = 0;

    void Update()
    {
        var width = Screen.width;
        var height = Screen.height;

        if (_lastWidth != width) // if the user is changing the width
        {
#if UNITY_STANDALONE
            // Update the application window to maintain proper resolution
            float heightAccordingToWidth = width / 16.0f * 9.0f;
            Screen.SetResolution(width, (int)Mathf.Round(heightAccordingToWidth), FullScreenMode.Windowed, new RefreshRate());
#endif
            StartCoroutine(InvokeAfterFrame());
        }
        else if (_lastHeight != height) // if the user is changing the height
        {
#if UNITY_STANDALONE
            // Update the application window to maintain proper resolution
            float widthAccordingToHeight = height / 9.0f * 16.0f;
            Screen.SetResolution((int)Mathf.Round(widthAccordingToHeight), height, FullScreenMode.Windowed, new RefreshRate());
#endif
            StartCoroutine(InvokeAfterFrame());
        }

        _lastWidth = width;
        _lastHeight = height;
    }

    private IEnumerator InvokeAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        if (OnScreenResize != null) { OnScreenResize.Invoke(); }
    }

}