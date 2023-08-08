using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeAspectRatioController : MonoBehaviour
{

    public static Action OnScreenResize = null;
    private int _lastWidth = 0;
    private int _lastHeight = 0;

    void Start()
    {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
    }

    void Update()
    {
        int width = Screen.width;
        int height = Screen.height;

        if (_lastWidth != width) // if the user is changing the width
        {
#if UNITY_STANDALONE
            // Update the application window to maintain proper resolution
            float heightAccordingToWidth = Screen.width / 16.0f * 9.0f;
            Screen.SetResolution(Screen.width, (int)Mathf.Round(heightAccordingToWidth), FullScreenMode.Windowed, new RefreshRate());
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
#endif
            StartCoroutine(LateInvoke());
        }
        else if (_lastHeight != height) // if the user is changing the height
        {
#if UNITY_STANDALONE
            // Update the application window to maintain proper resolution
            float widthAccordingToHeight = Screen.height / 9.0f * 16.0f;
            Screen.SetResolution((int)Mathf.Round(widthAccordingToHeight), Screen.height, FullScreenMode.Windowed, new RefreshRate());
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
#endif
            StartCoroutine(LateInvoke());
        }
    }

    private IEnumerator LateInvoke()
    {
        yield return new WaitForEndOfFrame();
        if (OnScreenResize != null) { OnScreenResize.Invoke(); }
    }

}