using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(UITooltipHandler))]
public class RelicHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Object Assignments")]
    [HideInInspector] public Relic relicInfo;
    [SerializeField] private GameObject imageObject;
    [SerializeField] private GameObject shinyBGObject;
    [SerializeField] private GameObject relicFlashObject;

    private UITooltipHandler _uiTooltipHandler;
    private Image _relicFlashImage;
    private float _initialScale;
    private float _desiredScale;
    private IEnumerator _relicFlashCoroutine = null;
    private IEnumerator _shinySpinCoroutine = null;
    private Canvas _relicCanvas;

    public bool ToggleShopFunctionality(bool isPurchaseable) => GetComponent<BuyableObject>().enabled = isPurchaseable;
    public void DisableTooltip() => _uiTooltipHandler.SetTooltipInteractibility(false);

    private void Awake()
    {
        _relicCanvas = GetComponent<Canvas>();
        _uiTooltipHandler = GetComponent<UITooltipHandler>();
        _relicFlashImage = relicFlashObject.GetComponent<Image>();
    }

    // Initialize the relic's information.
    public void Initialize(Relic r, bool showTooltipOnHover)
    {
        SetRelicImageScale(1, 1);
        SetSortingOrder(11);
        // Set all of the basic properties
        _uiTooltipHandler.HideTooltip();
        relicFlashObject.SetActive(false);
        GetComponent<TreasureRelicHandler>().enabled = false;
        _initialScale = imageObject.transform.localScale.x;
        _desiredScale = _initialScale;
        // Disable external functionalities
        ToggleShopFunctionality(false);
        // Set the relic information
        relicInfo = r;
        _uiTooltipHandler.SetTooltipText(r.relicName);
        _uiTooltipHandler.SetTooltipSubText(r.relicDesc);
        imageObject.GetComponent<Image>().sprite = r.relicImage;
        _relicFlashImage.sprite = r.relicImage;
    }

    // Flash the relic in an animation. This should happen
    // when the relic is used.
    public void FlashRelic()
    {
        if (_relicFlashCoroutine != null)
        {
            StopCoroutine(_relicFlashCoroutine);
        }
        _relicFlashCoroutine = FlashRelicCoroutine();
        StartCoroutine(_relicFlashCoroutine);
    }

    private IEnumerator FlashRelicCoroutine()
    {
        // Calculate frames and initial values for linear interpolation.
        float currTime = 0;
        float timeToWait = 0.7f;
        relicFlashObject.SetActive(true);
        Vector3 initialFlashScale = new Vector3(_initialScale + 0.4f, _initialScale + 0.4f, 1);
        Vector3 targetFlashScale = new Vector3(_initialScale, _initialScale, 1);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _relicFlashImage.transform.localScale = Vector3.Lerp(initialFlashScale, targetFlashScale, currTime / timeToWait);
            yield return null;
        }
        relicFlashObject.SetActive(false);
        _relicFlashCoroutine = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_relicFlashCoroutine != null) { return; }
        _desiredScale = _initialScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_relicFlashCoroutine != null) { return; }
        _desiredScale = _initialScale;
    }

    public void SetRelicImageScale(float scale, float tooltipScale)
    {
        _initialScale = scale;
        _desiredScale = scale;
        imageObject.transform.localScale = new Vector3(scale, scale, 1);
        shinyBGObject.transform.localScale = new Vector3(scale, scale, 1);
        _uiTooltipHandler.SetTooltipScale(new Vector2(tooltipScale, tooltipScale));
    }

    public void SetSortingOrder(int sortingOrder)
    {
        shinyBGObject.GetComponent<Canvas>().sortingOrder = sortingOrder - 1;
        _relicCanvas.sortingOrder = sortingOrder;
        _uiTooltipHandler.SetTooltipSortingOrder(sortingOrder + 1);
    }

    // Calling this function allows the user to click the relic to add
    // it to their inventory.
    public void EnableTreasureFunctionality()
    {
        GetComponent<TreasureRelicHandler>().enabled = true;
        SetRelicImageScale(3, 1.5f);
        SetSortingOrder(30);
        StartSpinShinyObject();
    }

    public void StartSpinShinyObject()
    {
        shinyBGObject.SetActive(true);
        _shinySpinCoroutine = RotateShinyBGCoroutine();
        StartCoroutine(_shinySpinCoroutine);
    }

    public void StopSpinShinyObject()
    {
        if (_shinySpinCoroutine != null)
        {
            StopCoroutine(_shinySpinCoroutine);
        }
        shinyBGObject.SetActive(false);
    }

    private IEnumerator RotateShinyBGCoroutine()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.01f);
        while (true)
        {
            shinyBGObject.transform.Rotate(0, 0, 2);
            yield return wfs;
        }
    }

    public void FixedUpdate()
    {
        if (_relicFlashCoroutine != null) { return; }
        float difference = Mathf.Abs(imageObject.transform.localScale.x - _desiredScale);
        if (difference > 0.011f)
        {
            if (imageObject.transform.localScale.x > _desiredScale)
            {
                if (difference < 0.04f)
                {
                    imageObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    imageObject.transform.localScale -= new Vector3(0.03f, 0.03f, 0);
                }
            }
            else
            {
                if (difference < 0.04f)
                {
                    imageObject.transform.localScale += new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    imageObject.transform.localScale += new Vector3(0.03f, 0.03f, 0);
                }
            }
        }
    }

}
