using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarCardController : MonoBehaviour
{

    [Header("Prefab Assignments")]
    [SerializeField] private GameObject cardPrefabObject;
    [Header("Object Assignments")]
    [SerializeField] private GameObject _deckOverlayContainer;
    public void HideDeckOverlay() => _deckOverlayContainer.SetActive(false);
    public void ShowDeckOverlay() => _deckOverlayContainer.SetActive(true);
    [SerializeField] private Image _deckOverlayImage;
    [SerializeField] private ScrollRect _cardPreviewScrollRect;
    [SerializeField] private CanvasGroup _scrollBarCanvasGroup;
    [SerializeField] private GraphicRaycaster _cardPreviewGraphicRaycaster;
    [SerializeField] private Transform _cardPreviewParentTransform;
    [SerializeField] private Button _showDeckButton;
    public int GetDeckButtonSortingOrder() => _showDeckButton.GetComponent<Canvas>().sortingOrder;
    public void SetDeckButtonSortingOrder(int order) => _showDeckButton.GetComponent<Canvas>().sortingOrder = order;
    [SerializeField] private Transform canvasTransform;

    private bool _isDeckPreviewButtonClickable = true;
    private Button _currentlySelectedButton = null;
    public bool IsCardPreviewShowing() => _currentlySelectedButton != null;
    private List<CardHandler> _cardPreviewHandlers = new List<CardHandler>();

    public void Initialize()
    {
        // Reset all child GameObjects in the card preview to their defaults.
        foreach (Transform child in _cardPreviewParentTransform)
        {
            Destroy(child.gameObject);
        }
        HideDeckOverlay();
        _currentlySelectedButton = null;
        _cardPreviewGraphicRaycaster.enabled = false;
        _showDeckButton.interactable = true;
    }

    // Spawn cards that goes from a certain position 
    // to the deck icon.
    public void AnimateCardsToDeck(Vector3 initialCanvasPosition, List<Card> cards, Vector3 initialScale)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            StartCoroutine(AnimateCardToDeckCoroutine(initialCanvasPosition, cards[i], initialScale, i));
        }
    }

    // Animates a single card going to the deck icon transform,
    // given an initial position, card type, and a
    // delay before the card appears.
    private IEnumerator AnimateCardToDeckCoroutine(Vector3 initialPosition, Card cardType, Vector3 initialScale, int delayInc = 0)
    {
        yield return new WaitForSeconds(delayInc * 0.08f);
        // Create and initialize the card object.
        GameObject cardObject = Instantiate(cardPrefabObject, canvasTransform);
        CanvasGroup cardCanvasGroup = cardObject.GetComponent<CanvasGroup>();
        cardObject.transform.position = initialPosition;
        cardObject.transform.localScale = initialScale;
        cardObject.GetComponent<Canvas>().sortingOrder = GetDeckButtonSortingOrder() + 1;
        cardObject.GetComponent<CardHandler>().Initialize(cardType);
        cardObject.GetComponent<CardHandler>().DisableInteractions();
        // Animate the card towards the deck icon.
        Vector3 targetPosition = _showDeckButton.transform.position;
        Vector3 targetScale = new Vector3(0.1f, 0.1f, 1); // Scale when the card reaches the deck icon.
        float currTime = 0;
        float timeToWait = 0.4f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            cardCanvasGroup.alpha = Mathf.Lerp(1, 0, currTime / timeToWait);
            cardObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, currTime / timeToWait);
            cardObject.transform.position = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }
        Destroy(cardObject);
    }
    public void DisableDeckPreviewButton()
    {
        _isDeckPreviewButtonClickable = false;
    }

    public void EnableDeckPreviewButton()
    {
        _isDeckPreviewButtonClickable = true;
    }

    // Toggle the card preview.
    public void ToggleCardOverlay(List<Card> cardsToShow, Button currButton)
    {
        if (!_isDeckPreviewButtonClickable) { return; }
        if (_currentlySelectedButton != null)
        {
            // Hide the cards if there is a previously selected button.
            StartCoroutine(HideCardsCoroutine());
            return;
        }
        else
        {
            // Show the cards if there's no previously selected button.
            _currentlySelectedButton = currButton;
            StartCoroutine(ShowCardsCoroutine(cardsToShow));
        }
    }

    // When given a list of cards, shows all of them in a navigable
    // menu.
    private IEnumerator ShowCardsCoroutine(List<Card> cardsToShow)
    {
        // Disable the scroll rect UNTIL all cards have animated in.
        _cardPreviewScrollRect.enabled = false;
        _currentlySelectedButton.interactable = false;
        // Enable the graphic raycaster so that the scrolling works.
        _cardPreviewGraphicRaycaster.enabled = true;
        // Set the sorting order of the cards to be over the overlay.
        int newSortingOrder = _deckOverlayImage.GetComponent<Canvas>().sortingOrder + 1;
        _currentlySelectedButton.GetComponent<Canvas>().sortingOrder = newSortingOrder;
        // Initialize information for card handlers.
        _cardPreviewHandlers = new List<CardHandler>();
        yield return ToggleDeckOverlayCoroutine(true);
        Transform horizontalTransform = null;
        int currCardIdx = 0;
        // Order the cards in alphabetical order, so the player
        // can't cheat and see the exact order.
        cardsToShow = cardsToShow.OrderBy((c) => c.GetCardStats().cardCost).ToList();
        // Recover a pooled object for each card.
        foreach (Card card in cardsToShow)
        {
            // If divisible by 5, create a new row of cards.
            if (currCardIdx % 5 == 0)
            {
                GameObject newRow = CreateNewCardRow();
                horizontalTransform = newRow.transform;
                horizontalTransform.SetParent(_cardPreviewParentTransform, false);
            }
            // Set the basic information for the card.
            GameObject cardObject = GetCardObjectFromPool();
            CardHandler cardController = cardObject.GetComponent<CardHandler>();
            cardObject.transform.SetParent(horizontalTransform, false);
            // We want the card to appear from nothing, so set the
            // initial showing to false.
            cardController.Initialize(card, false);
            currCardIdx++;
            _cardPreviewHandlers.Add(cardController);
        }
        // After all cards are created, animate them one-by-one.
        WaitForSeconds wfs = new WaitForSeconds(0.04f);
        foreach (CardHandler cc in _cardPreviewHandlers)
        {
            cc.CardAppear();
            yield return wfs;
        }
        // Animate the scrollbar in.
        StartCoroutine(ToggleScrollBarCoroutine(true));
        // In case sorting order was modified, re-set it again.
        _currentlySelectedButton.GetComponent<Canvas>().sortingOrder = _deckOverlayImage.GetComponent<Canvas>().sortingOrder + 1;
        _cardPreviewScrollRect.enabled = true;
        _currentlySelectedButton.interactable = true;
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewCardRow()
    {
        GameObject newRow = new GameObject("CardRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = true;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = 80;
        newRow.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 0);
        return newRow;
    }

    private IEnumerator HideCardsCoroutine()
    {
        _currentlySelectedButton.interactable = false;
        WaitForSeconds wfs = new WaitForSeconds(0.02f);
        // Disable the horizontal layout groups so that the cards don't teleport
        // together during destruction.
        foreach (Transform child in _cardPreviewParentTransform)
        {
            child.GetComponent<HorizontalLayoutGroup>().enabled = false;
        }
        // Animate the scrollbar out.
        StartCoroutine(ToggleScrollBarCoroutine(false));
        // Hide each card, if any exist.
        if (_cardPreviewHandlers.Count > 0)
        {
            GameObject lastCardToAnimate = _cardPreviewHandlers[0].gameObject;
            for (int i = _cardPreviewHandlers.Count - 1; i >= 0; i--)
            {
                CardHandler cardObjectRef = _cardPreviewHandlers[i];
                GameObject cardObject = cardObjectRef.gameObject;
                _cardPreviewHandlers.RemoveAt(i);
                StartCoroutine(cardObject.gameObject.GetComponent<CardHandler>().CardDisappearCoroutine(0.15f, CardAnimation.SHRINK, () =>
                {
                    ObjectPooler.Instance.ReturnObjectToPool(PoolableType.CARD, cardObject.gameObject);
                }));
                yield return wfs;
            }
            // Wait until the last card isn't shown anymore (has been deactivated).
            while (lastCardToAnimate != null && lastCardToAnimate.activeSelf)
            {
                yield return null;
            }
        }
        // Destroy each of the horizontal transforms.
        // This will only delete the rows; the card objects are pooled.
        foreach (Transform child in _cardPreviewParentTransform)
        {
            Destroy(child.gameObject);
        }
        // Disable the graphic raycaster so that the scrolling doesn't block the screen.
        _cardPreviewGraphicRaycaster.enabled = false;
        yield return ToggleDeckOverlayCoroutine(false);
        _currentlySelectedButton.interactable = true;
        _currentlySelectedButton = null;
    }

    // This Coroutine is called when a deck preview is currently being shown,
    // whether it be the Draw Pile, Discard Pile, or Whole Deck. It should
    // darken it all the way to allow for a scene transition.
    private IEnumerator ToggleDeckOverlayCoroutine(bool shouldShow)
    {
        // Set all of the initial values of the card to be partially visible.
        ShowDeckOverlay();
        Color initialColor = new Color(0, 0, 0, (shouldShow) ? 0 : 0.7f);
        Color targetColor = new Color(0, 0, 0, (shouldShow) ? 0.7f : 0);
        float currTime = 0;
        float timeToWait = 0.2f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _deckOverlayImage.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        // Make sure all values have been properly Lerped by setting them.
        _deckOverlayImage.color = targetColor;
        // Set active to false if the purpose is to hide.
        if (!shouldShow)
        {
            HideDeckOverlay();
        }
    }

    private IEnumerator ToggleScrollBarCoroutine(bool shouldShow)
    {
        float initialAlpha = (shouldShow) ? 0 : 1;
        float targetAlpha = (shouldShow) ? 1 : 0;
        float currTime = 0;
        float timeToWait = 0.2f; // Max # of frames calculated by 60 frames per second!
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _scrollBarCanvasGroup.alpha = Mathf.Lerp(initialAlpha, targetAlpha, currTime / timeToWait);
            yield return null;
        }
        // Make sure all values have been properly Lerped by setting them.
        _scrollBarCanvasGroup.alpha = targetAlpha;
    }

    private GameObject GetCardObjectFromPool()
    {
        // Return an already created card object.
        GameObject cardObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.CARD);
        CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
        cardObject.transform.localPosition = new Vector3(cardObject.transform.localPosition.x, cardObject.transform.localPosition.y, 0);
        cardObject.transform.localScale = new Vector2(0.55f, 0.55f);
        cardHandler.SetSortingOrder(22);
        cardHandler.ShouldScaleOnHover = true;
        cardHandler.HideCardInstantly(); // Hide the card instantly so we can animate it after.
        return cardObject;
    }

    // Toggles the visibility (on/off) of the card previews for
    // all cards in your deck.
    public void ToggleVisibilityOfCardsInDeck()
    {
        if (!_isDeckPreviewButtonClickable) { return; }
        ToggleCardOverlay(GameManager.GetHeroCards(), _showDeckButton);
    }

}
