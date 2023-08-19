using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignCameraController : MonoBehaviour
{

    public static CampaignCameraController Instance;
    [SerializeField] private Transform _transformToFollow;

    private float _permanentZIndex;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        _permanentZIndex = transform.position.z;
    }

    public void MoveCameraToPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, _permanentZIndex);
    }

    ///<summary>
    /// Make the camera controller lerp to a specific position.
    /// Applies smoothing.
    ///</summary>
    public void LerpCameraToPosition(Vector2 position, float timeToWait)
    {
        StartCoroutine(LerpCameraToPositionCoroutine(position, timeToWait));
    }

    ///<summary>
    /// Make the camera controller lerp to a specific position.
    /// Applies smoothing. Coroutine version of above function.
    ///</summary>
    public IEnumerator LerpCameraToPositionCoroutine(Vector2 position, float timeToWait)
    {
        float currTime = 0;
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = new Vector3(position.x, position.y, _permanentZIndex);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPosition, targetPosition, Mathf.SmoothStep(0, 1, currTime / timeToWait));
            yield return null;
        }
    }

}
