using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RelicHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Object Assignments")]
    [HideInInspector] public Relic relicInfo;
    public GameObject imageObject;
    public GameObject relicFlashObject;
    public GameObject tooltipParentObject;
    public GameObject shinyBGObject;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image relicFlashImage;
    public bool showTooltipOnHover;
    private float _initialScale;
    private float _desiredScale;
    private IEnumerator _relicFlashCoroutine = null;
    private IEnumerator _shinySpinCoroutine = null;
    private Canvas _relicCanvas;
    private Canvas _tooltipCanvas;

    private void Awake()
    {
        _relicCanvas = GetComponent<Canvas>();
        _tooltipCanvas = tooltipParentObject.GetComponent<Canvas>();
    }

    // Initialize the relic's information.
    public void Initialize(Relic r, bool showTooltipOnHover)
    {
        SetRelicImageScale(1, 1);
        SetSortingOrder(11);
        // Set all of the basic properties
        tooltipParentObject.SetActive(false);
        relicFlashObject.SetActive(false);
        GetComponent<TreasureRelicHandler>().enabled = false;
        GetComponent<ShopRelicHandler>().enabled = false;
        _initialScale = imageObject.transform.localScale.x;
        _desiredScale = _initialScale;
        this.showTooltipOnHover = showTooltipOnHover;
        // Set the relic information
        relicInfo = r;
        nameText.text = r.relicName;
        descText.text = r.relicDesc;
        imageObject.GetComponent<Image>().sprite = r.relicImage;
        relicFlashImage.sprite = r.relicImage;
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
        float frames = 0;
        float maxFrames = 0.8f * 60; // Max # of frames calculated by 60 frames per second!
        relicFlashObject.SetActive(true);
        Vector3 initialFlashScale = new Vector3(_initialScale + 0.5f, _initialScale + 0.5f, 1);
        Vector3 targetFlashScale = new Vector3(_initialScale, _initialScale, 1);
        while (frames < maxFrames)
        {
            relicFlashImage.transform.localScale = Vector3.Lerp(initialFlashScale, targetFlashScale, frames / maxFrames);
            frames++;
            yield return null;
        }
        relicFlashObject.SetActive(false);
        _relicFlashCoroutine = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showTooltipOnHover)
        {
            tooltipParentObject.SetActive(true);
        }
        if (_relicFlashCoroutine != null) { return; }
        _desiredScale = _initialScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipParentObject.SetActive(false);
        if (_relicFlashCoroutine != null) { return; }
        _desiredScale = _initialScale;
    }

    public void SetRelicImageScale(float scale, float tooltipScale)
    {
        _initialScale = scale;
        _desiredScale = scale;
        imageObject.transform.localScale = new Vector3(scale, scale, 1);
        shinyBGObject.transform.localScale = new Vector3(scale, scale, 1);
        tooltipParentObject.transform.localScale = new Vector3(tooltipScale, tooltipScale, 1);
    }

    public void SetSortingOrder(int sortingOrder)
    {
        shinyBGObject.GetComponent<Canvas>().sortingOrder = sortingOrder - 1;
        _relicCanvas.sortingOrder = sortingOrder;
        _tooltipCanvas.sortingOrder = sortingOrder + 1;
    }

    // Calling this function allows the user to click the relic to add
    // it to their inventory.
    public void EnableTreasureFunctionality()
    {
        GetComponent<TreasureRelicHandler>().enabled = true;
        SetRelicImageScale(3, 1.5f);
        SetSortingOrder(16);
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

    public void EnableShopFunctionality()
    {
        GetComponent<ShopRelicHandler>().enabled = true;
        tooltipParentObject.transform.localPosition = new Vector2(0, tooltipParentObject.transform.localPosition.y);
        tooltipParentObject.transform.localScale = new Vector3(0.6f, 0.6f);
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
