using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MouseHoverScaler))]
public class CampaignOptionController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private Transform _lineParentTransform;
    [SerializeField] private SpriteRenderer _iconSpriteRenderer;
    [SerializeField] private SpriteRenderer _bgSpriteRenderer;
    [SerializeField] private ParticleSystem _pSystem;
    [Header("Level Properties")]
    public LocationChoice LocationChoice;
    public List<Enemy> EnemiesToRender;
    public List<CampaignOptionController> ConnectedLevels = new List<CampaignOptionController>();

    private bool _isInteractable = false;
    private bool _wasVisited = false;
    private Animator _optionAnimator;
    private EventSystem _eventSystem;
    private MouseHoverScaler _mouseHoverScaler;

    private void Awake()
    {
        _eventSystem = EventSystem.current;
        _optionAnimator = GetComponent<Animator>();
        _mouseHoverScaler = GetComponent<MouseHoverScaler>();
        _mouseHoverScaler.Initialize(_iconSpriteRenderer);
    }

    public void Initialize(bool wasVisited)
    {
        // Set the visited state.
        _wasVisited = wasVisited;
        // Initialize the sprite and scale based on the type of location it is.
        CampaignInfo campaignInfo = Globals.GetCampaignInfo(GameManager.CurrentCampaignScene);
        LocationType locationType = campaignInfo.campaignLocations.Find((loc) => loc.locationType == LocationChoice);
        _iconSpriteRenderer.sprite = locationType.sprite;
        _iconSpriteRenderer.transform.localScale = new Vector2(locationType.iconScale, locationType.iconScale);
        // Play particle system if it's a miniboss encounter.
        if (LocationChoice == LocationChoice.MINIBOSS_ENCOUNTER)
        {
            _pSystem.gameObject.SetActive(true);
        }
    }

    // Sets whether or not the current sprite is interactable.
    // Second parameter controls whether or not all of the outlines
    // should change transparency or not.
    public void SetInteractable(bool isInteractable, bool shouldChangeVisuals = true)
    {
        _isInteractable = isInteractable;
        _mouseHoverScaler.SetIsInteractable(isInteractable);
        // If we shouldn't change the visuals, return early.
        if (!shouldChangeVisuals) { return; }
        // If it's interactable, make it a solid color.
        // Or else, make it transparent.
        if (isInteractable)
        {
            _iconSpriteRenderer.color = new Color(1, 1, 1, 1);
            // Make any icon except the boss icon pulse.
            if (LocationChoice != LocationChoice.BOSS_ENCOUNTER)
            {
                _optionAnimator.Play("Pulse");
            }
        }
        else
        {
            // If it's not selectable, alpha = 0.2f;
            _iconSpriteRenderer.color = new Color(1, 1, 1, 0.2f);
            _mouseHoverScaler.ResetScale();
        }
        // If it was visited, make it even more transparent.
        if (_wasVisited)
        {
            // If it's already visited, alpha = 0.05f;
            _iconSpriteRenderer.color = new Color(1, 1, 1, 0.05f);
        }
    }

    private void OnMouseDown()
    {
        // If we're hovering over something else or shouldn't have the
        // ability to interact, don't interact.
        if (_eventSystem.IsPointerOverGameObject() || !_isInteractable) return;
        // Choose the option in the CampaignController.
        CampaignController.Instance.ChooseOption(this);
    }

}
