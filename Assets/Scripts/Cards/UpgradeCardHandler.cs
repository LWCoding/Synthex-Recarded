using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CardHandler))]
public class UpgradeCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerClickHandler
{

    [Header("Object Assignments")]
    [SerializeField] private GameObject _upgradeContainerObject;
    [SerializeField] private GameObject _upgradeOverlayObject;
    [SerializeField] private GameObject _checkmarkOverlayObject;
    [SerializeField] private TextMeshProUGUI cardCostText;

    private CardHandler _parentCardHandler;
    private bool _isInteractable;
    public void SetInteractable(bool isInteractable) => _isInteractable = isInteractable;
    private bool _isSelected = false;
    private Transform _parentCardTransform;
    private Card _card;
    private int _cardIdx;
    private int _upgradeCost;
    public int GetCost() => _upgradeCost;

    private void Awake()
    {
        _parentCardHandler = GetComponent<CardHandler>();
        _parentCardTransform = _parentCardHandler.CardObject.transform;
    }

    private void Start()
    {
        _card = _parentCardHandler.card;
        _cardIdx = _parentCardHandler.GetCardIdx();
        _upgradeCost = _card.level * 3;
        _isInteractable = true;
        _upgradeContainerObject.SetActive(true);
        SetIsSelected(false);
        _parentCardHandler.SetTooltipPosition(TooltipPosition.LEFT);
    }

    private void SetPreviewInfo()
    {
        _upgradeCost = _card.level * 3;
        if (_card.IsMaxLevel())
        {
            cardCostText.text = "MAX";
            cardCostText.color = new Color(0, 0.8f, 1);
        }
        else
        {
            cardCostText.text = GetCost().ToString() + " XP";
            if (GetCost() <= GameController.GetXP())
            {
                // Can afford the card!
                cardCostText.color = new Color(0.3f, 1, 0);
            }
            else
            {
                // Cannot afford the card.
                cardCostText.color = new Color(1, 0.15f, 0.15f);
            }
        }
    }

    public void SetIsSelected(bool isSelected)
    {
        _isSelected = isSelected;
        _upgradeOverlayObject.SetActive(isSelected);
        _checkmarkOverlayObject.SetActive(isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        _parentCardHandler.SetSortingOrder(2);
        // Set the text it should show based on the card's info.
        SetPreviewInfo();
        // Update the console information.
        UpgradeController.Instance.UpdatePreviewConsole(_card, GetCost(), _isSelected);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        _parentCardHandler.SetSortingOrder(1);
        // Reset the console information to its default.
        UpgradeController.Instance.ResetConsolePreview();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // Toggle the selected property and play SFX.
        SetIsSelected(!_isSelected);
        SoundManager.Instance.PlaySFX(SoundEffect.CARD_HOVER);
        // Add or remove the card from the upgrade list.
        if (_isSelected)
        {
            UpgradeController.Instance.AddCardToUpgradeList(_card, GetCost(), _cardIdx);
        }
        else
        {
            UpgradeController.Instance.RemoveCardFromUpgradeList(_card, GetCost(), _cardIdx);
        }
        // Update the console information.
        UpgradeController.Instance.UpdatePreviewConsole(_card, GetCost(), _isSelected);
    }

}
