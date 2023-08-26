using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SecretTitleController : MonoBehaviour
{

    public UITitleButtonHandler quitButtonHandler;
    public GameObject secretTrophyObject;
    public Sprite secretButtonSprite;

    private void Start()
    {
        // TODO: This is just a secret for the demo!
        if (PlayerPrefs.GetInt("BeatGame") == 1)
        {
            EnableSecret();
        }
        secretTrophyObject?.SetActive(PlayerPrefs.GetInt("BeatBoykisser") >= 1);
    }

    private void EnableSecret()
    {
        quitButtonHandler.transform.GetChild(0).GetComponent<Image>().sprite = secretButtonSprite;
        quitButtonHandler.OnClick = new UnityEngine.Events.UnityEvent();
        quitButtonHandler.OnClick.AddListener(() =>
        {
            StartNewGame();
        });
    }

    // Starts a new game by setting all of the variables in GameManager
    // and initializing a starting relic. Optionally, start in a different
    // scene by supplying the mapScene parameter.
    public void StartNewGame()
    {
        // Initialize the hero with base information.
        GameManager.SetChosenHero(Globals.GetBaseHero(HeroTag.JACK));
        GameManager.SetSeenEnemies(new List<Encounter>());
        GameManager.SetGameScene(GameScene.FOREST);
        // GameManager.SetMapScene(MapScene.AERICHO);
        GameManager.SetMapObject(null);
        GameManager.SetMoney(150);
        GameManager.SetXP(15);
        GameManager.saveFileName = "Save.ass"; // TODO: Make this vary!
        GameManager.alreadyPlayedTutorials.Add("Battle");
        GameManager.alreadyPlayedTutorials.Add("Shop");
        // Start the game.
        StartGame();
    }

    private void StartGame()
    {
        // Make sure the map starts in the forest.
        GameManager.IsInCampaign = false;
        TransitionManager.Instance.HideScreen("Map", 1.5f);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.K))
        {
            PlayerPrefs.SetInt("BeatGame", 1);
            StartCoroutine(WaitUntilNoKeyCodesThenRefresh());
        }
    }

    private IEnumerator WaitUntilNoKeyCodesThenRefresh()
    {
        yield return new WaitUntil(() => !Input.anyKey);
        SceneManager.LoadScene("Title");
    }

}
