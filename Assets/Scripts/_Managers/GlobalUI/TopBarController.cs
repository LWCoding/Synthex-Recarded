using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public enum TokenType
{
    COIN = 0, XP = 1
}

[RequireComponent(typeof(TopBarRelicController))]
[RequireComponent(typeof(TopBarCardController))]
[RequireComponent(typeof(TopBarItemController))]
public class TopBarController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static TopBarController Instance { get; private set; }
    [Header("Prefab Assignments")]
    [SerializeField] private GameObject tokenPrefabObject;
    [Header("Object Assignments")]
    [SerializeField] private Transform topBarParentTransform;
    public int GetTopBarSortingOrder() => GetComponent<Canvas>().sortingOrder;
    [SerializeField] private Image barBGImage;
    [SerializeField] private Image heroFrameImage;
    [SerializeField] private Image heroHeadshotImage;
    [SerializeField] private Image coinIconImage;
    [SerializeField] private Image xpIconImage;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI heroHealthText;
    [SerializeField] private TextMeshProUGUI heroCurrencyText;
    [SerializeField] private TextMeshProUGUI heroXPText;
    [SerializeField] private Image pauseIconImage;

    private bool _isMouseOverTopBar = false;
    public bool IsPlayerInteractingWithTopBar() => _isMouseOverTopBar;

    private TopBarItemController _topBarItemController;
    public void RenderItems() => _topBarItemController.RenderItems();
    public void FlashItemObject(int idx) => _topBarItemController.FlashItemObject(idx);
    public void UpdateItemVerifyText(bool isClickable, bool hasBeenClicked) => _topBarItemController.UpdateItemVerifyText(isClickable, hasBeenClicked);
    public void ShowTopBarItemTooltip(Item item) => _topBarItemController.ShowTopBarItemTooltip(item);
    public void HideTopBarItemTooltip() => _topBarItemController.HideTopBarItemTooltip();
    private TopBarRelicController _topBarRelicController;
    public void RenderRelics() => _topBarRelicController.RenderRelics();
    public void FlashRelicObject(RelicType r) => _topBarRelicController.FlashRelicObject(r);
    private TopBarCardController _topBarCardController;
    public void ToggleCardOverlay(List<Card> cards, Button btn) => _topBarCardController.ToggleCardOverlay(cards, btn);
    public void HideDeckOverlay() => _topBarCardController.HideDeckOverlay();
    public int GetDeckButtonSortingOrder() => _topBarCardController.GetDeckButtonSortingOrder();
    public void SetDeckButtonSortingOrder(int order) => _topBarCardController.SetDeckButtonSortingOrder(order);
    public bool IsCardPreviewShowing() => _topBarCardController.IsCardPreviewShowing();
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
        _topBarRelicController = GetComponent<TopBarRelicController>();
        _topBarCardController = GetComponent<TopBarCardController>();
        _topBarItemController = GetComponent<TopBarItemController>();
    }

    // Initializes all of the information in the top bar.
    public void Initialize()
    {
        ShowTopBar();
        StopAllCoroutines();
        // Update the information in the Top Bar.
        UpdateUIInformation();
        // Render the relics using the TopBarRelicController.
        RenderRelics();
        // Render the items using the TopBarItemController.
        RenderItems();
        // Initialize all of the card information in the TopBarCardController.
        InitializeCardController();
        // Set the colors of the UI depending on the current hero.
        HeroData heroData = GameController.GetHeroData();
        SetUIStyle(heroData.heroUIColor, heroData.uiHeadshotSprite);
    }

    public void HideTopBar()
    {
        topBarParentTransform.gameObject.SetActive(false);
    }

    public void ShowTopBar()
    {
        topBarParentTransform.gameObject.SetActive(true);
    }

    // Updates all information in the top bar, such as name, health, 
    // currency, etc.
    public void UpdateUIInformation()
    {
        topBarParentTransform.SetParent(GlobalUIController.Instance.GlobalCanvas.transform);
        topBarParentTransform.localScale = new Vector3(1, 1, 1);
        topBarParentTransform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        heroNameText.text = GameController.GetHeroData().characterName.ToUpper();
        UpdateHealthText(GameController.GetHeroHealth(), GameController.GetHeroMaxHealth(), false);
        UpdateCurrencyText();
    }

    // Set the color of the UI depending on the currently selected hero.
    public void SetUIStyle(Color color, Sprite headshotSprite)
    {
        barBGImage.color = color - new Color(0, 0, 0, 0.1f);
        heroFrameImage.color = color;
        heroHeadshotImage.color = color;
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
            // If we are healing, play the heal health sound effect.
            if (changeInHealth > 0)
            {
                SoundManager.Instance.PlaySFX(SoundEffect.HEAL_HEALTH);
            }
            if (changeInHealth != 0)
            {
                ObjectPooler.Instance.SpawnUIPopup((changeInHealth > 0) ? "+" + changeInHealth.ToString() : changeInHealth.ToString(), 36, heroHealthText.transform.position, (changeInHealth > 0) ? new Color(0.1f, 1, 0.1f) : new Color(1, 0.1f, 0.1f), GlobalUIController.Instance.GlobalCanvas.transform, 1, 1.4f, false);
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

    // Spawn multiple tokens that goes from a certain position 
    // to the currency icon and adds to the current balance.
    public void AnimateTokensToBalance(TokenType tokenType, Vector3 initialCanvasPosition, int totalRewardAmount)
    {
        int tokenAmount = (tokenType == TokenType.COIN) ? Mathf.Clamp(totalRewardAmount / 15, 4, 15) : Mathf.Clamp(totalRewardAmount / 3, 2, 3);
        float displacementAmount = 90;
        int rewardedAmount = 0;
        for (int i = 0; i < tokenAmount; i++)
        {
            Vector3 newPosition = initialCanvasPosition + new Vector3(Random.Range(-displacementAmount, displacementAmount), Random.Range(-displacementAmount, displacementAmount), 0);
            rewardedAmount += totalRewardAmount / tokenAmount;
            StartCoroutine(AnimateTokenToBalanceCoroutine(tokenType, newPosition, totalRewardAmount / tokenAmount, i));
        }
        // If we haven't rewarded all the currency, do it in a final currency token.
        if (rewardedAmount < totalRewardAmount)
        {
            Vector3 newPosition = initialCanvasPosition + new Vector3(Random.Range(-displacementAmount, displacementAmount), Random.Range(-displacementAmount, displacementAmount), 0);
            StartCoroutine(AnimateTokenToBalanceCoroutine(tokenType, newPosition, totalRewardAmount - rewardedAmount, tokenAmount));
        }
    }

    // Animates a single token going to one of the icon transforms.
    private IEnumerator AnimateTokenToBalanceCoroutine(TokenType tokenType, Vector3 initialPosition, int amount, int delayInc = 0)
    {
        yield return new WaitForSeconds(delayInc * 0.08f);
        GameObject tokenObject = Instantiate(tokenPrefabObject, GlobalUIController.Instance.GlobalCanvas.transform);
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

    public void OnPointerEnter(PointerEventData ped)
    {
        _isMouseOverTopBar = true;
    }

    public void OnPointerExit(PointerEventData ped)
    {
        _isMouseOverTopBar = false;
    }

}
