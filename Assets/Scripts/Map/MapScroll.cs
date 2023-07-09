using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapScroll : MonoBehaviour
{
    public static MapScroll Instance;
    [Header("Object Assignments")]
    public EventSystem eventSystem;
    public float scrollSpeed;
    public float transitionSpeed;
    private GameObject _cameraObject;
    private Vector3 _targetPosition;
    private float _lastScroll;
    private float _minCameraY;
    private float _maxCameraY;
    private bool _isPlayingBeginningAnimation = false;
    private Animator _cameraAnimator;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _cameraAnimator = Camera.main.GetComponent<Animator>();
    }

    // Start the animation where the camera starts from the top and
    // pans to the bottom, where the player can start moving.
    // This also prohibits the player from scrolling as it's animating.
    public IEnumerator PanCameraAcrossMapCoroutine()
    {
        yield return SetScrollBounds();
        _isPlayingBeginningAnimation = true;
        _cameraAnimator.enabled = true;
        _cameraAnimator.Play("PanCamera");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying("PanCamera"));
        _cameraAnimator.enabled = false;
        _isPlayingBeginningAnimation = false;
        SetCameraPosition(Camera.main.transform.position);
    }

    public void SetCameraPosition(Vector3 pos)
    {
        Camera.main.transform.position = pos;
        _targetPosition = pos;
        _cameraObject = Camera.main.gameObject;
        StartCoroutine(SetScrollBounds());
    }

    // Sets the bounds at which the user can scroll.
    private IEnumerator SetScrollBounds()
    {
        yield return new WaitForEndOfFrame();
        Transform bossChoiceTransform = MapController.Instance.mapParentObject.transform.Find("BossChoice");
        _minCameraY = Camera.main.transform.position.y;
        _maxCameraY = bossChoiceTransform.position.y - 1;
    }

    private void Update()
    {
        if (_isPlayingBeginningAnimation || eventSystem.IsPointerOverGameObject())
        {
            return;
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll - _lastScroll) > Mathf.Epsilon)
        {
            _lastScroll = scroll;
            _targetPosition.y += scroll * scrollSpeed;
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, _minCameraY, _maxCameraY);
        }
        _cameraObject.transform.position = Vector3.Lerp(_cameraObject.transform.position, _targetPosition, Time.deltaTime * transitionSpeed);
    }

    public bool IsPlaying(string stateName)
    {
        return _cameraAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && _cameraAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }
}