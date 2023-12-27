using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CampaignController : MonoBehaviour
{

    public static CampaignController Instance;
    [Header("Object Assignments")]
    [SerializeField] private Transform _playerIconTransform;
    [SerializeField] private ParticleSystem _playerParticleSystem;
    [SerializeField] private Transform _firstMapLocationTransform;
    [Header("Audio Assignments")]
    [SerializeField] private AudioClip _footstepsSFX;

    public List<Transform> HeroFollowerTransforms; // Objects to follow player as they move.
    public bool CanPlayerChooseLevel() => !_isPlayerMoving && CampaignEventController.Instance.AreEventsComplete() && !_eventSystem.IsPointerOverGameObject();

    private void FindAndStoreAllLevelOptions() => _levelOptions = new List<CampaignOptionController>(GameObject.FindObjectsOfType<CampaignOptionController>());
    private CampaignSave _currCampaignSave;
    private EventSystem _eventSystem;
    private bool _isPlayerMoving = false;
    private List<CampaignOptionController> _levelOptions;

    public CampaignOptionController CurrentLevel;

    public void RegisterVisitedLevel(Vector3 levelPosition) => _currCampaignSave.visitedLevels.Add(levelPosition);


    // Make singleton instance of this class.
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        _eventSystem = EventSystem.current;
        // Store and initialize all levels.
        FindAndStoreAllLevelOptions();
        // Initialize any save information.
        CreateOrLoadSave();
    }

    private void Start()
    {
        // Initialize the UI.
        GlobalUIController.Instance.InitializeUI();
        // Play game music!
        SoundManager.Instance.PlayOnLoop(MusicType.MAP_MUSIC);
        // Move the player to the current hero location.
        CampaignCameraController.Instance.MoveCameraToPosition(_playerIconTransform.position);
        // Save the game.
        GameManager.SaveGame();
        // Initialize information for current level.
        // We do this AFTER the save so that dialogue will replay if the player leaves.
        CurrentLevel.SelectLevel();
    }

    ///<summary>
    /// If we already have save data, load the information into this script.
    /// Or else, create that information and save it as new.
    ///</summary>
    private void CreateOrLoadSave()
    {
        if (GameManager.GetCampaignSave() == null)
        {
            // If we didn't find a saved campaign, create a new campaign.
            _currCampaignSave = new CampaignSave
            {
                currScene = GameManager.GetGameScene()
            };
            _playerIconTransform.position = _firstMapLocationTransform.position;
            _currCampaignSave.heroMapPosition = _playerIconTransform.position;
            GameManager.SetCampaignSave(_currCampaignSave);
            // Set all levels to be non-visited.
            foreach (CampaignOptionController coc in _levelOptions)
            {
                coc.Initialize();
                coc.WasVisited = false;
            }
        }
        else
        {
            // If we found a saved campaign, load it.
            _currCampaignSave = GameManager.GetCampaignSave();
            _playerIconTransform.transform.position = _currCampaignSave.heroMapPosition;
            // Set visited states for all level options.
            List<Vector3> visitedLevels = _currCampaignSave.visitedLevels;
            foreach (CampaignOptionController coc in _levelOptions)
            {
                coc.Initialize();
                coc.WasVisited = visitedLevels.Contains(coc.transform.position);
            }
        }
    }

    // Select an option given a CampaignOptionController.
    public void ChooseOption(CampaignOptionController loc)
    {
        StartCoroutine(ChooseOptionCoroutine(loc, loc.ShouldActivateWhenVisited()));
    }

    private IEnumerator ChooseOptionCoroutine(CampaignOptionController loc, bool shouldRenderAreaEffects)
    {
        _isPlayerMoving = true;
        yield return new WaitForEndOfFrame();
        // Prevent the player from selecting another option.
        foreach (CampaignOptionController option in _levelOptions)
        {
            option.SetInteractable(false, false);
        }
        // If there is a current level, make sure that one is no longer set to the current level.
        CurrentLevel.DeselectLevel();
        // Make the character animate towards the next thing.
        CampaignCameraController.Instance.LerpCameraToPosition(loc.transform.position, 1.2f);
        // StartCoroutine(MoveHeroToPositionCoroutine(loc.transform.position, 0.8f));
        yield return CampaignCameraController.Instance.LerpCameraToPositionCoroutine(loc.transform.position, 1.3f);
        // Serialize current choice.
        _currCampaignSave.heroMapPosition = _playerIconTransform.position;
        GameManager.SetCampaignSave(_currCampaignSave);
        // Render first-time load if necessary.
        if (!loc.WasVisited)
        {
            loc.OnSelectFirstTime.Invoke();
            CampaignEventController.Instance.RenderAllQueuedEvents();
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => CampaignEventController.Instance.AreEventsComplete());
        // Get current location choice.
        LocationChoice locationChoice = loc.LocationChoice;
        // If we should render the effects of the location, render it.
        // Or else, just make the character move there.
        if (shouldRenderAreaEffects)
        {
            switch (locationChoice)
            {
                case LocationChoice.TREASURE:
                    TreasureController.Instance.ShowChest();
                    break;
                case LocationChoice.BASIC_ENCOUNTER:
                case LocationChoice.MINIBOSS_ENCOUNTER:
                case LocationChoice.BOSS_ENCOUNTER:
                    Encounter newEncounter = new() { enemies = loc.EnemiesToRenderInBattle };
                    GameManager.AddSeenEnemies(newEncounter);
                    GameManager.nextBattleEnemies = newEncounter.enemies;
                    TransitionManager.Instance.HideScreen("Battle", 0.75f);
                    break;
                case LocationChoice.NONE:
                    // If we're at a random path, just initialize the path from the
                    // current position.
                    CurrentLevel.SelectLevel();
                    break;
            }
        }
        else
        {
            CurrentLevel.SelectLevel();
        }
        _isPlayerMoving = false;
    }

    /*
    // Makes the player icon move towards a certain position.
    private IEnumerator MoveHeroToPositionCoroutine(Vector3 targetPosition, float timeToWait)
    {
        // Initialize information for main hero.
        Vector3 initialPosition = _playerIconTransform.localPosition;
        float currTime = 0;
        float timeSinceLastParticle = 0;
        float particleCooldown = 0.15f;
        // Initialize information for followers.
        List<Vector3> followerInitialPositions = new();
        List<Vector3> followerTargetPositions = new();
        for (int i = 0; i < HeroFollowerTransforms.Count; i++)
        {
            followerInitialPositions.Add(HeroFollowerTransforms[i].position);
            followerTargetPositions.Add(Vector3.Lerp(initialPosition, targetPosition, 1 - 0.33f * (i + 1)));
        }
        SoundManager.Instance.PlayOneShot(_footstepsSFX, 0.22f);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _playerIconTransform.localPosition = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            for (int i = 0; i < HeroFollowerTransforms.Count; i++)
            {
                HeroFollowerTransforms[i].localPosition = Vector3.Lerp(followerInitialPositions[i], followerTargetPositions[i], currTime / timeToWait);
            }
            timeSinceLastParticle += Time.deltaTime;
            if (timeSinceLastParticle > particleCooldown)
            {
                _playerParticleSystem.Emit(1);
                timeSinceLastParticle = 0;
            }
            yield return null;
        }
        _playerIconTransform.localPosition = targetPosition;
        yield return new WaitForSeconds(0.5f);
    }
    */

}
