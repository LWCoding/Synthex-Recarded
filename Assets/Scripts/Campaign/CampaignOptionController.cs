using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MouseHoverScaler))]
public class CampaignOptionController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private SpriteRenderer _iconSpriteRenderer;
    [SerializeField] private SpriteRenderer _bgSpriteRenderer;
    [SerializeField] private BoxCollider2D _iconCollider;
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

    public bool ShouldActivateWhenVisited() => LocationChoice == LocationChoice.NONE || !WasVisited || CanRenderMultipleTimes;
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
        _mouseHoverScaler.Initialize(_iconSpriteRenderer);
        _originalIconColliderSize = _iconCollider.size;
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
        // If the location was already visited and shouldn't be
        // rendered multiple times, make it very transparent and stop.
        if (WasVisited && !ShouldActivateWhenVisited())
        {
            LerpIconSpriteColorTo(new Color(1, 1, 1, 0.15f));
            return;
        }
        // If it's interactable and should be active, make it a solid color.
        // Or else, make it transparent.
        if (isInteractable && ShouldActivateWhenVisited())
        {
            LerpIconSpriteColorTo(new Color(1, 1, 1, 1));
            // Make any icon except the boss icon pulse.
            if (LocationChoice != LocationChoice.BOSS_ENCOUNTER)
            {
                _optionAnimator.Play("Pulse");
            }
        }
        else
        {
            LerpIconSpriteColorTo(new Color(1, 1, 1, 0.2f));
            _mouseHoverScaler.ResetScale();
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
    public void SetAsCurrentLevel()
    {
        // If the player has finished this location for the first time, invoke
        // after any animations are finished.
        if (!WasVisited)
        {
            StartCoroutine(WaitAndInvokeVisitedFirstTimeCoroutine());
        }
        CampaignController.Instance.RegisterVisitedLevel(transform.position);
        WasVisited = true;
        Initialize();
    }

    private IEnumerator WaitAndInvokeVisitedFirstTimeCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning());
        OnVisitedFirstTime.Invoke();
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

}
