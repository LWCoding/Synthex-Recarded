using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class CampaignController : MonoBehaviour
{

    public static CampaignController Instance;
    [Header("Prefab Assignments")]
    [SerializeField] private GameObject _mapOptionPrefab;
    [Header("Object Assignments")]
    [SerializeField] private Transform _playerIconTransform;
    [SerializeField] private ParticleSystem _playerParticleSystem;
    [SerializeField] private TextMeshPro _introBannerText;
    [SerializeField] private GameObject _firstMapLocationTransform;
    [SerializeField] private List<CampaignOptionController> _initialLevelOptions;
    [Header("Audio Assignments")]
    [SerializeField] private AudioClip _footstepsSFX;

    private CampaignSave _currCampaignSave;
    private List<GlobalObjectId> _visitedLevelIDs;
    private List<CampaignOptionController> _levelOptions;

    // Make singleton instance of this class.
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    private void Start()
    {
        // Find all level objects in scene.
        LoadAllLevels();
        // Make the game fade from black to clear.
        TransitionManager.Instance.ShowScreen(1.25f);
        // Play game music!
        SoundManager.Instance.PlayOnLoop(MusicType.MAP_MUSIC);
        // Load campaign or create a new one.
        if (GameManager.GetCampaignSave() == null)
        {
            // If we didn't find a saved campaign, create a new campaign.
            _currCampaignSave = new CampaignSave();
            _currCampaignSave.currScene = GameManager.GetGameScene();
            _currCampaignSave.heroMapPosition = _playerIconTransform.position;
            GameManager.SetCampaignSave(_currCampaignSave);
        }
        else
        {
            // If we found a saved campaign, load it.
            _currCampaignSave = GameManager.GetCampaignSave();
            _playerIconTransform.transform.position = _currCampaignSave.heroMapPosition;
            GameManager.SetGameScene(_currCampaignSave.currScene);
        }
        // Try to find the current level controller that the player is at.
        CampaignOptionController currentLevel = _levelOptions.Find((coc) => coc.transform.position == _playerIconTransform.position);
        if (currentLevel == null)
        {
            // If we didn't find it, then the player is at the start.
            SetAvailableLevelOptions(_initialLevelOptions);
        }
        else
        {
            // If we found it, then set the available connected levels.
            SetAvailableLevelOptions(currentLevel.ConnectedLevels);
        }
        // Save the game.
        GameManager.SaveGame();
    }

    // Find all levels and load them into the _mapOptions list.
    // This is performance-heavy, so it should be done only on Start.
    private void LoadAllLevels()
    {
        _levelOptions = new List<CampaignOptionController>(GameObject.FindObjectsOfType<CampaignOptionController>());
    }

    private void SetAvailableLevelOptions(List<CampaignOptionController> availableLevelOptions)
    {
        // Initially disable all level options.
        foreach (CampaignOptionController coc in _levelOptions)
        {
            coc.Initialize(false);
            coc.SetInteractable(false);
        }
        // Then, re-enable all the ones that are valid.
        foreach (CampaignOptionController coc in availableLevelOptions)
        {
            coc.SetInteractable(true);
        }
    }

    // Select an option given a CampaignOptionController.
    public void ChooseOption(CampaignOptionController loc)
    {
        StartCoroutine(ChooseOptionCoroutine(loc));
    }

    private IEnumerator ChooseOptionCoroutine(CampaignOptionController loc)
    {
        // Prevent the player from selecting another option.
        foreach (CampaignOptionController option in _levelOptions)
        {
            option.SetInteractable(false, false);
        }
        // Make the character animate towards the next thing.
        yield return HeroTraverseToPositionCoroutine(loc.transform.position);
        LocationChoice locationChoice = loc.LocationChoice;
        // Serialize current choice.
        _currCampaignSave.heroMapPosition = _playerIconTransform.position;
        _currCampaignSave.visitedLevels.Add(loc.transform.position);
        GameManager.SetCampaignSave(_currCampaignSave);
        // Render the appropriate actions based on the location.
        switch (locationChoice)
        {
            case LocationChoice.SHOP:
                TransitionManager.Instance.HideScreen("Shop", 0.75f);
                break;
            case LocationChoice.TREASURE:
                TreasureController.Instance.ShowChest();
                break;
            case LocationChoice.BASIC_ENCOUNTER:
            case LocationChoice.MINIBOSS_ENCOUNTER:
            case LocationChoice.BOSS_ENCOUNTER:
                Encounter newEncounter = new Encounter();
                newEncounter.enemies = loc.EnemiesToRender;
                GameManager.AddSeenEnemies(newEncounter);
                GameManager.nextBattleEnemies = newEncounter.enemies;
                TransitionManager.Instance.HideScreen("Battle", 0.75f);
                break;
            case LocationChoice.UPGRADE_MACHINE:
                TransitionManager.Instance.HideScreen("Upgrade", 0.75f);
                break;
        }
    }

    // Makes the player icon move towards a certain position.
    private IEnumerator HeroTraverseToPositionCoroutine(Vector3 targetPosition)
    {
        Vector3 initialPosition = _playerIconTransform.localPosition;
        float walkDuration = 1; // Amount of seconds to reach end of path.
        float timeElapsed = 0;
        float timeSinceLastParticle = 0;
        float particleCooldown = 0.15f;
        SoundManager.Instance.PlayOneShot(_footstepsSFX, 0.22f);
        while (timeElapsed < walkDuration)
        {
            _playerIconTransform.localPosition = Vector3.Lerp(initialPosition, targetPosition, timeElapsed / walkDuration);
            timeElapsed += Time.deltaTime;
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

}
