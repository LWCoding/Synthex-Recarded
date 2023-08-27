using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MouseHoverScaler))]
public class CampaignOptionController : MonoBehaviour
{

    [Header("Prefab Assignments")]
    [SerializeField] private GameObject mapArrowObject;
    [Header("Object Assignments")]
    [SerializeField] private SpriteRenderer _iconSpriteRenderer;
    [SerializeField] private BoxCollider2D _iconCollider;
    [SerializeField] private Transform _arrowParentTransform;
    [SerializeField] private ParticleSystem _pSystem;
    [Header("Level Properties")]
    public LocationChoice LocationChoice;
    public bool CanRenderMultipleTimes;
    public CampaignOptionController LevelIfLeftPressed;
    public CampaignOptionController LevelIfRightPressed;
    public CampaignOptionController LevelIfUpPressed;
    public CampaignOptionController LevelIfDownPressed;
    [Header("Unity Events")]
    public UnityEvent OnSelectFirstTime;
    public UnityEvent OnVisitedFirstTime;
    [Header("Battle Properties (optional)")]
    public List<Enemy> EnemiesToRender;

    public bool WasVisited = false;

    private IEnumerator _iconColorChangeCoroutine = null;
    private Vector2 _originalIconColliderSize;
    private bool _isInteractable = false;
    private Animator _optionAnimator;
    private EventSystem _eventSystem;
    private MouseHoverScaler _mouseHoverScaler;

    public bool ShouldActivateWhenVisited() => LocationChoice != LocationChoice.NONE && (!WasVisited || CanRenderMultipleTimes);
    // Get the connected levels by checking the levels in the four directions.
    public List<CampaignOptionController> GetConnectedLevels()
    {
        List<CampaignOptionController> connectedLevels = new List<CampaignOptionController>();
        if (LevelIfLeftPressed != null) connectedLevels.Add(LevelIfLeftPressed);
        if (LevelIfUpPressed != null) connectedLevels.Add(LevelIfUpPressed);
        if (LevelIfRightPressed != null) connectedLevels.Add(LevelIfRightPressed);
        if (LevelIfDownPressed != null) connectedLevels.Add(LevelIfDownPressed);
        return connectedLevels;
    }

    private void Awake()
    {
        _eventSystem = EventSystem.current;
        _optionAnimator = GetComponent<Animator>();
        _mouseHoverScaler = GetComponent<MouseHoverScaler>();
        _mouseHoverScaler.Initialize(_iconSpriteRenderer.transform);
        _originalIconColliderSize = _iconCollider.size;
        InitializeArrows();
    }

    // Initialize all arrow objects pointing to available levels.
    private void InitializeArrows()
    {
        // Delete all children of parent.
        for (int i = 0; i < _arrowParentTransform.childCount; i++)
        {
            Destroy(_arrowParentTransform.GetChild(i).gameObject);
        }
        // Create all the arrows based on the number of connected levels.
        List<CampaignOptionController> connectedLevels = GetConnectedLevels();
        foreach (CampaignOptionController coc in connectedLevels)
        {
            GameObject arrowObject = Instantiate(mapArrowObject, _arrowParentTransform);
            arrowObject.transform.position = transform.position;
            arrowObject.transform.right = coc.transform.position - arrowObject.transform.position;
            arrowObject.transform.Rotate(0, 0, -90);
            arrowObject.transform.Translate(new Vector3(0, 1.1f, 0));
            // Make the current arrow transparent.
            // When it is clicked, execute a similar action to selecting the level.
            arrowObject.GetComponent<CampaignArrowHandler>().InstantlyHideArrow();
            arrowObject.GetComponent<CampaignArrowHandler>().OnClick.AddListener(coc.OnMouseDown);
        }
    }

    public void Initialize()
    {
        // Initialize the sprite and scale based on the type of location it is.
        CampaignInfo campaignInfo = Globals.GetCampaignInfo(GameManager.GetGameScene());
        LocationType locationType = campaignInfo.campaignLocations.Find((loc) => loc.locationType == LocationChoice);
        _iconSpriteRenderer.sprite = locationType.sprite;
        _iconSpriteRenderer.transform.localScale = new Vector2(locationType.iconScale, locationType.iconScale);
        // Play particle system if it's a miniboss encounter.
        if (LocationChoice == LocationChoice.MINIBOSS_ENCOUNTER)
        {
            _pSystem.gameObject.SetActive(true);
        }
        // Make hitbox bigger if it's an empty path.
        if (LocationChoice == LocationChoice.NONE)
        {
            _iconCollider.size = _originalIconColliderSize * new Vector2(1.3f, 1.3f);
        }
        else
        {
            _iconCollider.size = _originalIconColliderSize;
        }
    }

    // Sets whether or not the current sprite is interactable.
    // Second parameter controls whether or not all of the outlines
    // should change transparency or not.
    public void SetInteractable(bool isInteractable, bool shouldChangeVisuals = true)
    {
        _isInteractable = isInteractable;
        _mouseHoverScaler.SetIsInteractable(isInteractable);
        _optionAnimator.Play("Idle");
        // If we shouldn't change the visuals, return early.
        if (!shouldChangeVisuals) { return; }
        // If it's interactable, make the option pulse.
        // Or else, reset the scale because it shouldn't be interactable.
        if (isInteractable)
        {
            // Make any icon except the boss icon pulse.
            if (LocationChoice != LocationChoice.BOSS_ENCOUNTER)
            {
                _optionAnimator.Play("Pulse");
            }
        }
        else
        {
            _mouseHoverScaler.ResetScale();
        }
        // If the option should activate something when visited, make it opaque.
        // Or else, make it a bit transparent.
        LerpIconSpriteColorTo(new Color(1, 1, 1, (ShouldActivateWhenVisited()) ? 1 : 0.2f));
    }

    // Lerps the sprite icon color to a target color.
    // Stops any color-changing coroutines if they are running.
    private void LerpIconSpriteColorTo(Color targetColor)
    {
        if (_iconColorChangeCoroutine != null)
        {
            StopCoroutine(_iconColorChangeCoroutine);
        }
        _iconColorChangeCoroutine = LerpIconSpriteColorCoroutine(targetColor);
        StartCoroutine(_iconColorChangeCoroutine);
    }

    ///<summary>
    // Set the current option as the current level. This will invoke the
    // OnLoadFirstTime method if it is the first time it was loaded.
    // Also sets the level to be officially visited, since this runs when
    // the player has finished visiting the location.
    ///<summary>
    public void SelectLevel()
    {
        // If the player has finished this location for the first time, invoke
        // after any animations are finished.
        if (!WasVisited)
        {
            // Adds all events to the queue.
            OnVisitedFirstTime.Invoke();
            // Renders the events one-by-one.
            CampaignEventController.Instance.RenderAllQueuedEvents();
        }
        else
        {
            CampaignEventController.Instance.AreAllEventsComplete = true;
            if (TransitionManager.Instance.IsScreenDarkened)
            {
                // Then make the game fade from black to clear.
                TransitionManager.Instance.ShowScreen(1.25f);
            }
        }
        CampaignController.Instance.RegisterVisitedLevel(transform.position);
        WasVisited = true;
        Initialize();
        StartCoroutine(InitializeArrowsWhenPlayerCanChoose());
    }

    ///<summary>
    /// This function should be called when the player moves from this level to another
    /// level.
    ///</summary>
    public void DeselectLevel()
    {
        StartCoroutine(HideArrowsAfterDelay());
    }

    private IEnumerator HideArrowsAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        foreach (Transform arrowTransform in _arrowParentTransform)
        {
            arrowTransform.GetComponent<CampaignArrowHandler>().HideArrow();
        }
    }

    private void OnMouseDown()
    {
        // If we shouldn't be allowed to choose a level, disable this.
        if (!CampaignController.Instance.CanPlayerChooseLevel() || !_isInteractable) { return; }
        // Choose the option in the CampaignController if it should render.
        CampaignController.Instance.ChooseOption(this);
    }

    private IEnumerator LerpIconSpriteColorCoroutine(Color targetColor)
    {
        Color currColor = _iconSpriteRenderer.color;
        float currTime = 0;
        float timeToWait = 0.3f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _iconSpriteRenderer.color = Color.Lerp(currColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        _iconSpriteRenderer.color = targetColor;
    }

    private IEnumerator InitializeArrowsWhenPlayerCanChoose()
    {
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning);
        yield return new WaitUntil(() => CampaignEventController.Instance.AreAllEventsComplete);
        if (!ShouldActivateWhenVisited())
        {
            foreach (Transform arrowTransform in _arrowParentTransform)
            {
                arrowTransform.GetComponent<CampaignArrowHandler>().ShowArrow();
            }
        }
    }

}
