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

    private CardHandler _parentCardHandler;
    private bool _isInteractable;
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
        // Initialize starting values.
        _parentCardHandler.SetSortingOrder(1);
        _card = _parentCardHandler.card;
        _cardIdx = _parentCardHandler.GetCardIdx();
        _upgradeCost = _card.level * 3;
        _isInteractable = true;
        _upgradeContainerObject.SetActive(true);
        // Make sure card starts unselected with a tooltip slanting left.
        SetIsSelected(false);
        _parentCardHandler.SetTooltipPosition(TooltipPosition.LEFT);
        // Update information about upgrading this card.
        _upgradeCost = _card.level * 3;
        if (_card.IsMaxLevel())
        {
            _isInteractable = false;
            _upgradeOverlayObject.SetActive(true);
        }
    }

    // Set card to be interactable or not.
    // However, if card is max level, may not always work.
    public void SetInteractable(bool isInteractable)
    {
        if (_card.IsMaxLevel()) { return; }
        _isInteractable = isInteractable;
    }

    public void SetIsSelected(bool isSelected)
    {
        _isSelected = isSelected;
        _upgradeOverlayObject.SetActive(isSelected);
        _checkmarkOverlayObject.SetActive(isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Update the console information.
        UpgradeController.Instance.UpdatePreviewConsole(_card, GetCost(), _isSelected);
        // Make the card go above other cards.
        _parentCardHandler.SetSortingOrder(2);
        if (!_isInteractable) { return; }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset the console information to its default.
        UpgradeController.Instance.ResetConsolePreview();
        // Make the card go back to an initial sorting order.
        _parentCardHandler.SetSortingOrder(1);
        if (!_isInteractable) { return; }
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
