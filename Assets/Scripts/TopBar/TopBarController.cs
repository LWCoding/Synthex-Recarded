using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum TokenType
{
    COIN = 0, XP = 1
}

[RequireComponent(typeof(TopBarRelicController))]
[RequireComponent(typeof(TopBarCardController))]
public class TopBarController : MonoBehaviour
{

    public static TopBarController Instance { get; private set; }
    [Header("Prefab Assignments")]
    [SerializeField] private GameObject tokenPrefabObject;
    [Header("Object Assignments")]
    [SerializeField] private Image barBGImage;
    [SerializeField] private Image heroFrameImage;
    [SerializeField] private Image pauseIconImage;
    [SerializeField] private Image heroHeadshotImage;
    [SerializeField] private GameObject topBarParentObject;
    [SerializeField] private Image coinIconImage;
    [SerializeField] private Image xpIconImage;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI heroHealthText;
    [SerializeField] private TextMeshProUGUI heroCurrencyText;
    [SerializeField] private TextMeshProUGUI heroXPText;

    private Transform _canvasTransform;
    private TopBarRelicController _topBarRelicController;
    public void RenderRelics() => _topBarRelicController.RenderRelics();
    public void FlashRelicObject(RelicType r) => _topBarRelicController.FlashRelicObject(r);
    private TopBarCardController _topBarCardController;
    public void EnableShowAllCardsButton() => _topBarCardController.EnableShowAllCardsButton();
    public void DisableShowAllCardsButton() => _topBarCardController.DisableShowAllCardsButton();
    public void AnimateCardsToDeck(Vector3 initialCanvasPosition, List<Card> cards, Vector3 initialScale) => _topBarCardController.AnimateCardsToDeck(initialCanvasPosition, cards, initialScale);

    public void InitializeCardController() => _topBarCardController.Initialize();

    // This Awake function runs on the first time the bar is instantiated.
    private void Awake()
    {
        // Set this to the Instance if it is the first one.
        // Or else, destroy this.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
        // Make sure this object isn't destroyed.
        DontDestroyOnLoad(this.gameObject);
        _topBarRelicController = GetComponent<TopBarRelicController>();
        _canvasTransform = GameObject.Find("GlobalTopBarCanvas").transform;
    }

    // Initializes all of the information in the top bar.
    // This should be called when a scene is first loaded.
    public void InitializeTopBar()
    {
        ShowTopBar();
        StopAllCoroutines();
        topBarParentObject.transform.SetParent(GameObject.Find("GlobalTopBarCanvas").transform);
        topBarParentObject.transform.localScale = new Vector3(1, 1, 1);
        topBarParentObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        heroNameText.text = GameController.GetHeroData().characterName.ToUpper();
        UpdateHealthText(GameController.GetHeroHealth(), GameController.GetHeroMaxHealth(), false);
        UpdateCurrencyText();
        // Spawn all relic objects at top.
        RenderRelics();
        // Do stuff with cards.
        InitializeCardController();
        // Set the colors of the UI depending on the current hero.
        HeroData heroData = GameController.GetHeroData();
        SetUIStyle(heroData.heroUIColor, heroData.heroHeadshotSprite);
    }

    public void HideTopBar()
    {
        topBarParentObject.SetActive(false);
    }

    public void ShowTopBar()
    {
        topBarParentObject.SetActive(true);
    }

    // Set the color of the UI depending on the currently selected hero.
    public void SetUIStyle(Color color, Sprite headshotSprite)
    {
        barBGImage.color = color - new Color(0, 0, 0, 0.1f);
        heroFrameImage.color = color;
        pauseIconImage.color = color + new Color(0.2f, 0.2f, 0.2f);
        heroHeadshotImage.sprite = headshotSprite;
    }

    // Sets the hero's health on the top bar (UI).
    public void UpdateHealthText(int newHealth, int maxHealth, bool animateText = true)
    {
        // Animate the text if we're not in the battle scene (or else we already
        // see the health change via the hero).
        if (animateText && SceneManager.GetActiveScene().name != "Battle")
        {
            int prevHealth = int.Parse(heroHealthText.text.Split('/')[0]);
            int changeInHealth = newHealth - prevHealth;
            if (changeInHealth != 0)
            {
                ObjectPooler.Instance.SpawnUIPopup((changeInHealth > 0) ? "+" + changeInHealth.ToString() : changeInHealth.ToString(), 36, heroHealthText.transform.position, (changeInHealth > 0) ? new Color(0.1f, 1, 0.1f) : new Color(1, 0.1f, 0.1f), _canvasTransform, 1, 1.4f, false);
            }
        }
        heroHealthText.text = newHealth.ToString() + "/" + maxHealth;
    }

    // Sets the hero's currency on the top bar (UI).
    public void UpdateCurrencyText()
    {
        heroCurrencyText.text = GameController.GetMoney().ToString();
        heroXPText.text = GameController.GetXP().ToString() + "/100";
    }

    // Spawn multiple coins that goes from a certain position 
    // to the currency icon and adds to the current balance.
    public void AnimateCoinsToBalance(Vector3 initialCanvasPosition, int totalRewardAmount)
    {
        int coinAmount = Mathf.Clamp(totalRewardAmount / 15, 4, 15);
        float displacementAmount = 90;
        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 newPosition = initialCanvasPosition + new Vector3(Random.Range(-displacementAmount, displacementAmount), Random.Range(-displacementAmount, displacementAmount), 0);
            StartCoroutine(AnimateTokenToBalanceCoroutine(TokenType.COIN, newPosition, totalRewardAmount / coinAmount, i));
        }
    }

    // Spawn multiple XP tokens that goes from a certain position 
    // to the XP icon and adds to the current balance.
    public void AnimateXPToBalance(Vector3 initialCanvasPosition, int totalRewardAmount)
    {
        int xpAmount = Mathf.Clamp(totalRewardAmount / 15, 4, 15);
        float displacementAmount = 90;
        for (int i = 0; i < xpAmount; i++)
        {
            Vector3 newPosition = initialCanvasPosition + new Vector3(Random.Range(-displacementAmount, displacementAmount), Random.Range(-displacementAmount, displacementAmount), 0);
            StartCoroutine(AnimateTokenToBalanceCoroutine(TokenType.XP, newPosition, totalRewardAmount / xpAmount, i));
        }
    }

    // Animates a single token going to one of the icon transforms.
    private IEnumerator AnimateTokenToBalanceCoroutine(TokenType tokenType, Vector3 initialPosition, int amount, int delayInc = 0)
    {
        yield return new WaitForSeconds(delayInc * 0.08f);
        GameObject tokenObject = Instantiate(tokenPrefabObject, _canvasTransform);
        tokenObject.transform.position = initialPosition;
        tokenObject.GetComponent<Image>().sprite = (tokenType == TokenType.COIN) ? coinIconImage.sprite : xpIconImage.sprite;
        Vector3 targetPosition = (tokenType == TokenType.COIN) ? coinIconImage.transform.position : xpIconImage.transform.position;
        float currTime = 0;
        float timeToWait = 0.3f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            tokenObject.transform.position = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }
        // Play the SFX.
        SoundManager.Instance.PlaySFX(SoundEffect.COIN_OBTAIN);
        // Add the money to the player's bank.
        if (tokenType == TokenType.COIN) { GameController.AddMoney(amount); }
        if (tokenType == TokenType.XP) { GameController.AddXP(amount); }
        UpdateCurrencyText();
        Destroy(tokenObject);
    }

}
