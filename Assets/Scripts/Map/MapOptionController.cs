using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapOptionController : MonoBehaviour
{

    [Header("Object Assignments")]
    public GameObject linePrefab;
    public Transform lineParentTransform;
    public SpriteRenderer iconRenderer;
    public SpriteRenderer outlineRenderer;
    public GameObject pSystemObject;
    public bool isConnected = false;

    [Header("Map Location Data")]
    public SerializableMapLocation serializableMapLocation;

    private float _initialScale;
    private float _desiredScale;
    private bool _isInteractable = false;
    private bool _wasVisited = false;
    private Animator _mapAnimator;
    private EventSystem _eventSystem;

    private void Awake()
    {
        _mapAnimator = GetComponent<Animator>();
        _eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    private void Start()
    {
        _initialScale = iconRenderer.transform.localScale.x;
        _desiredScale = _initialScale;
    }

    public void OnMouseEnter()
    {
        if (!_isInteractable) { return; }
        _desiredScale = _initialScale + 0.1f;
    }

    private void OnMouseExit()
    {
        if (!_isInteractable) { return; }
        _desiredScale = _initialScale;
    }

    private void OnMouseDown()
    {
        if (_eventSystem.IsPointerOverGameObject())
        {
            return;
        }
        if (!_isInteractable) { return; }
        // Prevent the player from selecting another option.
        MapController.Instance.DisableMapOptionColliders();
        // Make sure this is marked as visited.
        _wasVisited = true;
        // Choose the option in the MapController.
        MapController.Instance.ChooseOption(serializableMapLocation);
    }

    public void SetType(MapLocationType mapLoc, int floor)
    {
        serializableMapLocation = new SerializableMapLocation();
        serializableMapLocation.floorNumber = floor;
        serializableMapLocation.mapLocationType = mapLoc;
        serializableMapLocation.position = transform.position;
        iconRenderer.sprite = mapLoc.sprite;
        iconRenderer.transform.localScale = new Vector3(mapLoc.iconScale, mapLoc.iconScale, 1);
        // Change how obstacles are displayed.
        if (mapLoc.isObstacle)
        {
            iconRenderer.GetComponent<SpriteRenderer>().sortingOrder = 1;
            outlineRenderer.enabled = false;
            iconRenderer.color -= new Color(0, 0, 0, 0.3f);
        }
        // Play particle system if it's a miniboss encounter.
        if (mapLoc.type == MapChoice.MINIBOSS_ENCOUNTER)
        {
            pSystemObject.SetActive(true);
        }
    }

    public void SetBossFloor(int floor)
    {
        serializableMapLocation.position = transform.position;
        serializableMapLocation.floorNumber = floor;
    }

    // Sets whether or not the current sprite is interactable.
    // Second parameter controls whether or not all of the outlines
    // should change transparency or not.
    public void SetInteractable(bool isInteractable, bool shouldChangeTransparency)
    {
        _isInteractable = isInteractable;
        // If it's interactable, make it a solid color.
        // Or else, make it transparent.
        if (shouldChangeTransparency)
        {
            if (isInteractable)
            {
                iconRenderer.color = new Color(1, 1, 1, 1);
                // Don't make the boss icon pulse.
                if (serializableMapLocation.mapLocationType.type != MapChoice.BOSS)
                {
                    _mapAnimator.Play("Pulse");
                }
            }
            else
            {
                // If it's not selectable, alpha = 0.2f;
                iconRenderer.color = new Color(1, 1, 1, 0.2f);
                _desiredScale = _initialScale;
            }
        }
        // If it was visited, make it even more transparent.
        if (_wasVisited)
        {
            // If it's already visited, alpha = 0.05f;
            iconRenderer.color = new Color(1, 1, 1, 0.05f);
        }
    }

    // Instantiates a new line, give another MapOptionController.
    // Adjusts the rotation of the line, as well as the position of the
    // mask, to point towards the targetPosition.
    public void CreateLineTo(MapOptionController other, int sortingOrder)
    {
        Vector3 targetPosition = other.transform.position;
        GameObject lineObject = Instantiate(linePrefab);
        lineObject.transform.SetParent(lineParentTransform);
        lineObject.transform.position = lineParentTransform.transform.position;
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
        other.isConnected = true;
    }

    // Instantiates a new line, given a Vector3 position.
    // Adjusts the rotation of the line, as well as the position of the
    // mask, to point towards the targetPosition.
    public void CreateLineTo(Vector3 targetPosition, int sortingOrder)
    {
        GameObject lineObject = Instantiate(linePrefab);
        lineObject.transform.SetParent(lineParentTransform);
        lineObject.transform.position = lineParentTransform.transform.position;
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

    public void FixedUpdate()
    {
        float difference = Mathf.Abs(iconRenderer.transform.localScale.x - _desiredScale);
        if (difference > 0.011f)
        {
            if (iconRenderer.transform.localScale.x > _desiredScale)
            {
                if (difference < 0.05f)
                {
                    iconRenderer.transform.localScale -= new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    iconRenderer.transform.localScale -= new Vector3(0.03f, 0.03f, 0);
                }
            }
            else
            {
                if (difference < 0.05f)
                {
                    iconRenderer.transform.localScale += new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    iconRenderer.transform.localScale += new Vector3(0.03f, 0.03f, 0);
                }
            }
        }
    }

}
