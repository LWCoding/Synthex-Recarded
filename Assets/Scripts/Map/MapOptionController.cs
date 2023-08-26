using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MouseHoverScaler))]
public class MapOptionController : MonoBehaviour
{

    [Header("Prefab Assignments")]
    [SerializeField] private GameObject _linePrefab;
    [Header("Object Assignments")]
    [SerializeField] private Transform _lineParentTransform;
    [SerializeField] private SpriteRenderer _iconRenderer;
    [SerializeField] private SpriteRenderer _outlineRenderer;
    [SerializeField] private GameObject _pSystemObject;

    [HideInInspector] public SerializableMapLocation SerializableMapLocation;
    [HideInInspector] public bool IsConnected = false;

    private bool _isInteractable = false;
    private bool _wasVisited = false;
    private Animator _optionAnimator;
    private EventSystem _eventSystem;
    private MouseHoverScaler _mouseHoverScaler;

    private void Awake()
    {
        _optionAnimator = GetComponent<Animator>();
        _eventSystem = EventSystem.current;
        _mouseHoverScaler = GetComponent<MouseHoverScaler>();
        _mouseHoverScaler.Initialize(_iconRenderer.transform);
    }

    private void OnMouseDown()
    {
        if (_eventSystem.IsPointerOverGameObject())
        {
            return;
        }
        if (!_isInteractable) { return; }
        // Make sure this is marked as visited.
        _wasVisited = true;
        // Choose the option in the MapController.
        MapController.Instance.ChooseOption(SerializableMapLocation);
    }

    public void SetType(MapLocationType mapLoc, int floor)
    {
        SerializableMapLocation = new SerializableMapLocation();
        SerializableMapLocation.floorNumber = floor;
        SerializableMapLocation.mapLocationType = mapLoc;
        SerializableMapLocation.position = transform.position;
        _iconRenderer.sprite = mapLoc.sprite;
        _iconRenderer.transform.localScale = new Vector3(mapLoc.iconScale, mapLoc.iconScale, 1);
        // Change how obstacles are displayed.
        if (mapLoc.isObstacle)
        {
            _iconRenderer.GetComponent<SpriteRenderer>().sortingOrder = 1;
            _outlineRenderer.enabled = false;
            _iconRenderer.color -= new Color(0, 0, 0, 0.3f);
        }
        // Play particle system if it's a miniboss encounter.
        if (mapLoc.type == MapChoice.MINIBOSS_ENCOUNTER)
        {
            _pSystemObject.SetActive(true);
        }
    }

    public void SetBossFloor(int floor)
    {
        SerializableMapLocation.position = transform.position;
        SerializableMapLocation.floorNumber = floor;
    }

    // Sets whether or not the current sprite is interactable.
    // Second parameter controls whether or not all of the outlines
    // should change transparency or not.
    public void SetInteractable(bool isInteractable, bool shouldChangeTransparency)
    {
        _isInteractable = isInteractable;
        _mouseHoverScaler.SetIsInteractable(isInteractable);
        // If it's interactable, make it a solid color.
        // Or else, make it transparent.
        if (shouldChangeTransparency)
        {
            if (isInteractable)
            {
                _iconRenderer.color = new Color(1, 1, 1, 1);
                // Don't make the boss icon pulse.
                if (SerializableMapLocation.mapLocationType.type != MapChoice.BOSS)
                {
                    _optionAnimator.Play("Pulse");
                }
            }
            else
            {
                // If it's not selectable, alpha = 0.2f;
                _iconRenderer.color = new Color(1, 1, 1, 0.2f);
                _mouseHoverScaler.ResetScale();
            }
        }
        // If it was visited, make it even more transparent.
        if (_wasVisited)
        {
            // If it's already visited, alpha = 0.05f;
            _iconRenderer.color = new Color(1, 1, 1, 0.05f);
        }
    }

    // Instantiates a new line, give another MapOptionController.
    // Adjusts the rotation of the line, as well as the position of the
    // mask, to point towards the targetPosition.
    public void CreateLineTo(MapOptionController other, int sortingOrder)
    {
        Vector3 targetPosition = other.transform.position;
        GameObject lineObject = Instantiate(_linePrefab);
        lineObject.transform.SetParent(_lineParentTransform);
        lineObject.transform.position = _lineParentTransform.transform.position;
        Transform lineTransform = lineObject.transform.Find("Line").transform;
        Transform lineMaskTransform = lineObject.transform.Find("LineMask").transform;
        float distance = Vector3.Distance(lineTransform.position, targetPosition);
        float scale = distance / 0.63f;
        lineTransform.right = targetPosition - lineTransform.position;
        lineMaskTransform.right = targetPosition - lineMaskTransform.position;
        lineTransform.localScale = new Vector3(0.3f, lineTransform.localScale.y, 1);
        lineMaskTransform.localScale = new Vector3(scale, lineTransform.localScale.y, 1);
        lineTransform.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        lineMaskTransform.GetComponent<SpriteMask>().isCustomRangeActive = true;
        lineMaskTransform.GetComponent<SpriteMask>().frontSortingOrder = sortingOrder;
        lineMaskTransform.GetComponent<SpriteMask>().backSortingOrder = sortingOrder - 1;
        other.IsConnected = true;
    }

    // Instantiates a new line, given a Vector3 position.
    // Adjusts the rotation of the line, as well as the position of the
    // mask, to point towards the targetPosition.
    public void CreateLineTo(Vector3 targetPosition, int sortingOrder)
    {
        GameObject lineObject = Instantiate(_linePrefab);
        lineObject.transform.SetParent(_lineParentTransform);
        lineObject.transform.position = _lineParentTransform.transform.position;
        Transform lineTransform = lineObject.transform.Find("Line").transform;
        Transform lineMaskTransform = lineObject.transform.Find("LineMask").transform;
        float distance = Vector3.Distance(lineTransform.position, targetPosition);
        float scale = distance / 0.63f;
        lineTransform.right = targetPosition - lineTransform.position;
        lineMaskTransform.right = targetPosition - lineMaskTransform.position;
        lineTransform.localScale = new Vector3(0.3f, lineTransform.localScale.y, 1);
        lineMaskTransform.localScale = new Vector3(scale, lineTransform.localScale.y, 1);
        lineTransform.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        lineMaskTransform.GetComponent<SpriteMask>().isCustomRangeActive = true;
        lineMaskTransform.GetComponent<SpriteMask>().frontSortingOrder = sortingOrder;
        lineMaskTransform.GetComponent<SpriteMask>().backSortingOrder = sortingOrder - 1;
    }

}
