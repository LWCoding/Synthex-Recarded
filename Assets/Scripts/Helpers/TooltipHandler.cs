using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipHandler : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private GameObject _tooltipParentObject;
    [SerializeField] private TextMeshPro _tooltipMainText;
    [SerializeField] private TextMeshPro _tooltipSubText;

    private bool _showTooltipOnHover = true;

    public void SetTooltipText(string text) => _tooltipMainText.text = text;
    public void SetTooltipSubText(string text) => _tooltipSubText.text = text;

    private void Awake()
    {
        if (GetComponent<Collider2D>() == null) { Debug.LogError(gameObject.name + " requires Collider2D for tooltip to work!"); }
        if (_tooltipParentObject == null) Debug.LogError("Tooltip obj on " + gameObject.name + " was not assigned!");
    }

    public void OnMouseEnter()
    {
        if (_showTooltipOnHover)
        {
            ShowTooltip();
        }
    }

    public void OnMouseExit()
    {
        HideTooltip();
    }

    ///<summary>
    /// Shows the tooltip instantly.
    ///</summary>
    public void ShowTooltip(bool forceSkipAnimation = false)
    {
        _tooltipParentObject.SetActive(true);
    }

    public void HideTooltip()
    {
        _tooltipParentObject.SetActive(false);
    }

    public void SetTooltipInteractibility(bool isInteractable)
    {
        _showTooltipOnHover = isInteractable;
        if (!isInteractable)
        {
            HideTooltip();
        }
    }

}
