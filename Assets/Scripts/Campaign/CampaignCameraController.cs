using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignCameraController : MonoBehaviour
{

    public static CampaignCameraController Instance;

    [Header("Object Assignments")]
    [SerializeField] private Transform _cameraTransformToMove;
    [SerializeField] private Image _vignetteImage;

    private float _permanentZIndex;
    private Action _restoreCameraAfterZoom;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        _permanentZIndex = _cameraTransformToMove.position.z;
        Debug.Assert(_cameraTransformToMove != null, "No assigned camera transform to move!", this);
        Debug.Assert(_vignetteImage != null, "Vignette was not assigned", this);
    }

    public void MoveCameraToPosition(Vector2 position)
    {
        SaveCameraState();
        _cameraTransformToMove.position = new Vector3(position.x, position.y, _permanentZIndex);
    }

    ///<summary>
    /// Changes the alpha of the vignette around the borders of the screen.
    /// Applies smoothing.
    ///</summary>
    public void LerpVignetteAlpha(float targetAlpha, float timeToWait)
    {
        SaveCameraState();
        StartCoroutine(LerpVignetteAlphaCoroutine(targetAlpha, timeToWait));
    }

    ///<summary>
    /// Changes the alpha of the vignette around the borders of the screen.
    /// Applies smoothing. Coroutine version of above function.
    ///</summary>
    public IEnumerator LerpVignetteAlphaCoroutine(float targetAlpha, float timeToWait)
    {
        float currTime = 0;
        Color initialColor = _vignetteImage.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _vignetteImage.color = Color.Lerp(initialColor, targetColor, Mathf.SmoothStep(0, 1, currTime / timeToWait));
            yield return null;
        }
    }

    ///<summary>
    /// Make the camera controller lerp to a specific position.
    /// Applies smoothing.
    ///</summary>
    public void LerpCameraToPosition(Vector2 position, float timeToWait)
    {
        SaveCameraState();
        StartCoroutine(LerpCameraToPositionCoroutine(position, timeToWait));
    }

    ///<summary>
    /// Make the camera controller lerp to a specific position.
    /// Applies smoothing. Coroutine version of above function.
    ///</summary>
    public IEnumerator LerpCameraToPositionCoroutine(Vector2 position, float timeToWait)
    {
        float currTime = 0;
        Vector3 initialPosition = _cameraTransformToMove.position;
        Vector3 targetPosition = new Vector3(position.x, position.y, _permanentZIndex);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _cameraTransformToMove.position = Vector3.Lerp(initialPosition, targetPosition, Mathf.SmoothStep(0, 1, currTime / timeToWait));
            yield return null;
        }
    }

    ///<summary>
    /// Saves the camera's current state, typically before an animation.
    ///</summary>
    public void SaveCameraState()
    {
        _restoreCameraAfterZoom = null;
        Vector3 currCameraPosition = Camera.main.transform.position;
        float currCameraSize = Camera.main.orthographicSize;
        float currVignetteColor = _vignetteImage.color.a;
        _restoreCameraAfterZoom += () => ZoomCameraOnObject(currCameraPosition, 0.5f, currCameraSize, currVignetteColor, null, false);
    }

    ///<summary>
    /// Restores the camera back to a saved state.
    ///</summary>
    public void RestoreCamera()
    {
        _restoreCameraAfterZoom.Invoke();
    }

    ///<summary>
    /// Zooms the camera in on an object.
    /// Applies smoothing. Optional function to invoke after finished.
    ///</summary>
    public void ZoomCameraOnObject(GameObject target, float animationTime, float targetZoom, float vignetteTargetAlpha, Action codeToRunAfter = null, bool shouldSaveBeforeRunning = true)
    {
        if (shouldSaveBeforeRunning) SaveCameraState();
        StartCoroutine(ZoomCameraOnObjectCoroutine(target.transform.position, animationTime, targetZoom, vignetteTargetAlpha, codeToRunAfter));
    }

    ///<summary>
    /// Zooms the camera in on a specified transform.position.
    /// Applies smoothing. Optional function to invoke after finished.
    ///</summary>
    public void ZoomCameraOnObject(Vector3 targetPosition, float animationTime, float targetZoom, float vignetteTargetAlpha, Action codeToRunAfter = null, bool shouldSaveBeforeRunning = true)
    {
        if (shouldSaveBeforeRunning) SaveCameraState();
        StartCoroutine(ZoomCameraOnObjectCoroutine(targetPosition, animationTime, targetZoom, vignetteTargetAlpha, codeToRunAfter));
    }

    ///<summary>
    /// Zooms the camera in on an object.
    /// Applies smoothing. Optional function to invoke after finished.
    ///</summary>
    public IEnumerator ZoomCameraOnObjectCoroutine(Vector3 targetPosition, float animationTime, float targetZoom, float vignetteTargetAlpha, Action codeToRunAfter)
    {
        float currTime = 0;
        float timeToWait = animationTime;
        float initialZoom = Camera.main.orthographicSize;
        LerpCameraToPosition(targetPosition, timeToWait);
        LerpVignetteAlpha(vignetteTargetAlpha, timeToWait);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            Camera.main.orthographicSize = Mathf.Lerp(initialZoom, targetZoom, Mathf.SmoothStep(0, 1, currTime / timeToWait));
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        if (codeToRunAfter != null) codeToRunAfter.Invoke();
    }

}
