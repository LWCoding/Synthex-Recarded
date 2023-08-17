using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class CardChoiceController : MonoBehaviour
{

    [HideInInspector] public static CardChoiceController Instance;
    [Header("Object Assignments")]
    public GameObject cardPrefab;
    public Image bgFadeImage;
    public TextMeshProUGUI bgText;
    public Transform cardParentTransform;
    public CanvasGroup skipCardCanvasGroup;
    private List<GameObject> _cardChoiceObjects = new List<GameObject>();
    private GameObject _cardObjectChosen;

    private void Awake()
    {
        Instance = GetComponent<CardChoiceController>();
    }

    // Shows the card choices on the screen.
    public void ShowCardChoices(int numCards, Action codeToRunAfter)
    {
        // Find X random cards from that list, depending on `numCards`.
        List<Card> cardOptions = new List<Card>();
        for (int i = 0; i < numCards; i++)
        {
            Card c = GameManager.GetRandomCard(cardOptions);
            // If there are no possible cards to draw, don't draw any!
            if (c == null)
            {
                continue;
            }
            cardOptions.Add(c);
        }
        // Render the possible cards on the screen.
        StartCoroutine(ShowCardChoicesCoroutine(cardOptions, codeToRunAfter));
    }

    private IEnumerator ShowCardChoicesCoroutine(List<Card> cardsToShow, Action codeToRunAfter)
    {
        // Make sure the deck preview icon is overlayed on top of the fade bg.
        TopBarController.Instance.SetDeckButtonSortingOrder(bgFadeImage.GetComponent<Canvas>().sortingOrder + 1);
        // Make the background fade out to black (slightly).
        bgFadeImage.gameObject.SetActive(true);
        bgText.gameObject.SetActive(true);
        bgFadeImage.color = new Color(0, 0, 0, 0);
        bgText.color = new Color(1, 1, 1, 0);
        skipCardCanvasGroup.alpha = 0;
        WaitForSeconds wfs = new WaitForSeconds(0.03f);
        for (int i = 0; i < 15; i++)
        {
            bgFadeImage.color += new Color(0, 0, 0, 0.05f);
            bgText.color += new Color(0, 0, 0, 0.07f);
            yield return wfs;
        }
        // Spawn the card objects and show them on the screen.
        int numCards = cardsToShow.Count;
        float canvasScale = GameObject.Find("Canvas").transform.localScale.x;
        float positionDifference = 350f * canvasScale;
        float startPosition = ((numCards % 2 == 0) ? -Mathf.Abs(-(Mathf.Floor(numCards / 2) - 0.5f)) : -Mathf.Abs(-Mathf.Floor(numCards / 2))) * positionDifference;
        for (int i = 0; i < numCards; i++)
        {
            GameObject cardObject = GetCardObjectFromPool();
            CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
            Card c = cardsToShow[i];
            cardObject.transform.position = cardParentTransform.position + new Vector3(startPosition + i * positionDifference, 0, 0);
            cardHandler.Initialize(c, false);
            cardHandler.CardAppear();
            _cardChoiceObjects.Add(cardObject);
            yield return new WaitForSeconds(0.16f);
        }
        skipCardCanvasGroup.gameObject.SetActive(true);
        for (int i = 0; i < 15; i++)
        {
            skipCardCanvasGroup.alpha += 0.07f;
            yield return wfs;
        }
        // Only allow player to select cards after they've all been rendered.
        for (int i = 0; i < numCards; i++)
        {
            CardHandler cardHandler = _cardChoiceObjects[i].GetComponent<CardHandler>();
            CardClickHandler cardClickHandler = cardHandler.GetComponent<CardClickHandler>();
            cardClickHandler.enabled = true;
            cardClickHandler.OnCardClick.AddListener((obj) =>
            {
                SoundManager.Instance.PlaySFX(SoundEffect.NEW_CARD_SELECT);
                _cardObjectChosen = obj;
                CardChoiceController.Instance.HideUnselectedCards(true, codeToRunAfter);
            });
        }
    }

    public void HideUnselectedCards(bool didPlayerChooseCard, Action codeToRunAfter)
    {
        StartCoroutine(HideUnselectedCardsCoroutine(didPlayerChooseCard, codeToRunAfter));
    }

    private IEnumerator HideUnselectedCardsCoroutine(bool didPlayerChooseCard, Action codeToRunAfter)
    {
        // Hide all of the options that weren't selected.
        skipCardCanvasGroup.GetComponent<GraphicRaycaster>().enabled = false;
        skipCardCanvasGroup.alpha = 0.3f; // Fade out Skip Card button.
        for (int i = _cardChoiceObjects.Count - 1; i >= 0; i--)
        {
            GameObject c = _cardChoiceObjects[i];
            CardHandler cc = c.GetComponent<CardHandler>();
            cc.DisableInteractions();
            if (cc.gameObject != _cardObjectChosen)
            {
                StartCoroutine(c.GetComponent<CardHandler>().CardDisappearCoroutine(0.4f, CardAnimation.TRANSLATE_DOWN, () =>
                {
                    ObjectPooler.Instance.ReturnObjectToPool(PoolableType.CARD, c);
                }));
                _cardChoiceObjects.RemoveAt(i);
            }
        }
        yield return new WaitForSeconds(1);
        // Hide remaining card. If there is none, ignore this.
        if (_cardChoiceObjects.Count != 0)
        {
            GameObject remainingCard = _cardChoiceObjects[0];
            CardHandler remainingCardHandler = remainingCard.GetComponent<CardHandler>();
            remainingCard.GetComponent<Canvas>().enabled = false;
            TopBarController.Instance.AnimateCardsToDeck(remainingCard.transform.position, new List<Card> { remainingCardHandler.card }, remainingCardHandler.transform.localScale);
            GameManager.AddCardToDeck(remainingCardHandler.card); // Add card to deck.
            SoundManager.Instance.PlaySFX(SoundEffect.CARD_OBTAIN); // Play card chosen sound!
        }
        yield return new WaitForSeconds(0.4f);
        if (codeToRunAfter != null) { codeToRunAfter.Invoke(); }
    }

    private GameObject GetCardObjectFromPool()
    {
        GameObject cardObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.CARD);
        // Return an already created card object.
        CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
        // Set all of the stuff for the created card object.
        cardObject.SetActive(true);
        cardObject.transform.localScale = new Vector3(0.7f, 0.7f, 1);
        cardObject.transform.SetParent(cardParentTransform, false);
        cardObject.transform.localScale = new Vector3(0.7f, 0.7f, 1);
        cardHandler.SetSortingOrder(14); // Set to a sorting order greater than overlay
        cardHandler.ModifyHoverBehavior(true, false, false, false);
        cardObject.GetComponent<Canvas>().sortingOrder = bgFadeImage.GetComponent<Canvas>().sortingOrder + 1;
        cardHandler.HideCardInstantly(); // Hide the card instantly so we can animate it after.
        return cardObject;
    }

}
