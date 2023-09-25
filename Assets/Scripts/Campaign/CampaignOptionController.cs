using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct TravelLocation
{
    [SerializeField] private CampaignOptionController _destination;
    public List<GameEvent> Requirements;
    public CampaignOptionController GetDestination()
    {
        return (IsVisitable()) ? _destination : null;
    }
    public bool IsVisitable() => _destination != null &&
                                (Requirements == null ||
                                Requirements.TrueForAll((ge) => ge.IsCompleted()));
}

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
    public TravelLocation LevelIfLeftPressed;
    public TravelLocation LevelIfRightPressed;
    public TravelLocation LevelIfUpPressed;
    public TravelLocation LevelIfDownPressed;
    public GameObject InteractableObject;
    [Header("Unity Events")]
    [Tooltip("Runs scripts before the event renders the actual location data")]
    public UnityEvent OnSelectFirstTime;
    [Tooltip("Runs scripts after the event renders the actual location data")]
    public UnityEvent OnVisitedFirstTime;
    [Header("Additional Properties (optional)")]
    public List<Enemy> EnemiesToRenderInBattle;
    public ShopLoadout LoadoutInShop;

    public bool WasVisited = false;

    private IEnumerator _iconColorChangeCoroutine = null;
    private bool _isInteractable = false;
    private Animator _optionAnimator;
    private MouseHoverScaler _mouseHoverScaler;

    public bool ShouldActivateWhenVisited() => (LocationChoice == LocationChoice.BASIC_ENCOUNTER || LocationChoice == LocationChoice.MINIBOSS_ENCOUNTER || LocationChoice == LocationChoice.BOSS_ENCOUNTER) && (!WasVisited || CanRenderMultipleTimes);
    // Get the connected levels by checking the levels in the four directions.
    public List<CampaignOptionController> GetConnectedLevels()
    {
        HashSet<CampaignOptionController> connectedLevels = new HashSet<CampaignOptionController>();
        if (LevelIfLeftPressed.IsVisitable()) connectedLevels.Add(LevelIfLeftPressed.GetDestination());
        if (LevelIfUpPressed.IsVisitable()) connectedLevels.Add(LevelIfUpPressed.GetDestination());
        if (LevelIfRightPressed.IsVisitable()) connectedLevels.Add(LevelIfRightPressed.GetDestination());
        if (LevelIfDownPressed.IsVisitable()) connectedLevels.Add(LevelIfDownPressed.GetDestination());
        return new List<CampaignOptionController>(connectedLevels);
    }

    private void Awake()
    {
        _optionAnimator = GetComponent<Animator>();
        _mouseHoverScaler = GetComponent<MouseHoverScaler>();
        _mouseHoverScaler.Initialize(_iconSpriteRenderer.transform);
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
            _iconCollider.size = new Vector2(1.3f, 1.3f);
        }
        else
        {
            _iconCollider.size = new Vector2(1.1f, 1.1f);
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
        // If we have a battle, it should be transparent if visited.
        // If we have anything else, just make it fully opaque.
        if (LocationChoice == LocationChoice.BASIC_ENCOUNTER || LocationChoice == LocationChoice.MINIBOSS_ENCOUNTER || LocationChoice == LocationChoice.BOSS_ENCOUNTER)
        {
            // If the option should activate something when visited, make it opaque.
            // Or else, make it a bit transparent.
            LerpIconSpriteColorTo(new Color(1, 1, 1, (ShouldActivateWhenVisited()) ? 1 : 0.2f));
        }
        else
        {
            LerpIconSpriteColorTo(new Color(1, 1, 1, 1));
        }
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
        StartCoroutine(VisitLevelWhenPlayerCanChoose());
    }

    ///<summary>
    /// This function should be called when the player moves from this level to another
    /// level.
    ///</summary>
    public void DeselectLevel() => StartCoroutine(HideArrowsAfterDelay());

    private IEnumerator HideArrowsAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        if (InteractableObject != null) { InteractableObject.GetComponent<IInteractable>().OnLocationExit(); }
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

    private IEnumerator VisitLevelWhenPlayerCanChoose()
    {
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning);
        yield return new WaitUntil(() => CampaignEventController.Instance.AreAllEventsComplete);
        if (LocationChoice == LocationChoice.SHOP) GameManager.nextShopLoadout = LoadoutInShop;
        if (InteractableObject != null) { InteractableObject.GetComponent<IInteractable>().OnLocationEnter(); }
        foreach (Transform arrowTransform in _arrowParentTransform)
        {
            arrowTransform.GetComponent<CampaignArrowHandler>().ShowArrow();
        }
    }

}
