using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public enum TooltipPosition
{
    LEFT = 0, CENTER = 1, RIGHT = 2
}

public class UITooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Object Assignments")]
    [SerializeField] private GameObject _tooltipParentObject;
    [SerializeField] private TextMeshProUGUI _tooltipMainText;
    [SerializeField] private TextMeshProUGUI _tooltipSubText;
    [Header("Tooltip Properties")]
    [SerializeField] private bool _shouldAnimateTooltip = false;
    [SerializeField] private float _tooltipFadeInDelay = 0;

    private bool _showTooltipOnHover = true;
    private Vector2 _tooltipInitialLocalPosition;
    private Canvas _tooltipCanvas;
    private CanvasGroup _tooltipCanvasGroup;
    private IEnumerator _tooltipShowCoroutine = null;

    public void SetTooltipDelay(float delay) => _tooltipFadeInDelay = delay;
    public void SetTooltipText(string text) => _tooltipMainText.text = text;
    public void SetTooltipSubText(string text) => _tooltipSubText.text = text;

    private void Awake()
    {
        if (_tooltipParentObject == null) Debug.LogError("Tooltip obj on " + gameObject.name + " was not assigned!");
        _tooltipInitialLocalPosition = _tooltipParentObject.transform.localPosition;
        _tooltipCanvas = _tooltipParentObject.GetComponent<Canvas>();
        _tooltipCanvasGroup = _tooltipParentObject.GetComponent<CanvasGroup>();
        if (_shouldAnimateTooltip && _tooltipCanvasGroup == null) Debug.LogError("Tooltip obj on " + gameObject.name + " needs CanvasGroup component!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_showTooltipOnHover)
        {
            ShowTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    ///<summary>
    /// Shows the tooltip. If there is an animation that must be played,
    /// play it. Optional forceSkipAnimation parameter to skip this animation
    /// regardless of if the tooltip needs to animate.
    ///</summary>
    public void ShowTooltip(bool forceSkipAnimation = false)
    {
        if (!_shouldAnimateTooltip || forceSkipAnimation)
        {
            _tooltipParentObject.SetActive(true);
        }
        else
        {
            bool hasDelay = _tooltipFadeInDelay > 0;
            if (!hasDelay)
            {
                _tooltipShowCoroutine = AnimateTooltipInCoroutine();
                StartCoroutine(_tooltipShowCoroutine);
            }
            else
            {
                _tooltipShowCoroutine = PromptAnimateTooltipInCoroutine();
                StartCoroutine(_tooltipShowCoroutine);
            }
        }
    }

    public void HideTooltip()
    {
        if (!_shouldAnimateTooltip)
        {
            _tooltipParentObject.SetActive(false);
        }
        else
        {
            if (_tooltipShowCoroutine != null)
            {
                StopCoroutine(_tooltipShowCoroutine);
            }
            _tooltipParentObject.SetActive(false);
        }
    }

    public void SetTooltipSortingOrder(int sortingOrder)
    {
        if (_tooltipCanvas == null) Debug.LogError("Tooltip obj on " + gameObject.name + " needs Canvas component!");
        _tooltipCanvas.sortingOrder = sortingOrder;
    }

    public void SetTooltipInteractibility(bool isInteractable)
    {
        _showTooltipOnHover = isInteractable;
        if (!isInteractable)
        {
            HideTooltip();
        }
    }

    public void SetTooltipLocalPosition(Vector2 localPosition)
    {
        _tooltipParentObject.transform.localPosition = localPosition;
    }

    public void SetTooltipScale(Vector2 scale)
    {
        _tooltipParentObject.transform.localScale = scale;
    }

    // Updates the position of a tooltip when the card is hovered over.
    public void SetTooltipPosition(TooltipPosition tooltipPosition)
    {
        switch (tooltipPosition)
        {
            case TooltipPosition.LEFT:
                _tooltipParentObject.transform.localPosition = new Vector3(-275, _tooltipInitialLocalPosition.y, 0);
                break;
            case TooltipPosition.CENTER:
                _tooltipParentObject.transform.localPosition = _tooltipInitialLocalPosition;
                break;
            case TooltipPosition.RIGHT:
                _tooltipParentObject.transform.localPosition = new Vector3(275, _tooltipInitialLocalPosition.y, 0);
                break;
        }
    }

    // Animate the tooltip in after the delay has been waited.
    private IEnumerator PromptAnimateTooltipInCoroutine()
    {
        yield return new WaitForSeconds(_tooltipFadeInDelay);
        ShowTooltip(true);
        float currTime = 0f;
        float timeToWait = 0.2f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _tooltipCanvasGroup.alpha = Mathf.Lerp(0, 1, currTime / timeToWait);
            yield return null;
        }
    }

    // Animate the tooltip in.
    private IEnumerator AnimateTooltipInCoroutine()
    {
        ShowTooltip();
        float currTime = 0f;
        float timeToWait = 0.2f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _tooltipCanvasGroup.alpha = Mathf.Lerp(0, 1, currTime / timeToWait);
            yield return null;
        }
    }

}
