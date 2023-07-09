using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopIconHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerClickHandler
{

    public float _initialScale = 1;
    public float _desiredScale;
    public Color normalColor;
    public Color selectedColor;
    public Color hoverColor;
    public ShopTab shopTab;
    public GameObject scrollTransform;
    public bool isInteractable = false;
    private Image _iconImage;

    private void Awake()
    {
        _iconImage = GetComponent<Image>();
    }

    private void Start()
    {
        GetComponent<Image>().enabled = true;
        transform.localScale = new Vector3(_initialScale, _initialScale, 1);
        _desiredScale = _initialScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) { return; }
        _desiredScale = _initialScale + 0.1f;
        _iconImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) { return; }
        _desiredScale = _initialScale;
        _iconImage.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) { return; }
        ShopController.Instance.SwitchShopTabTo(shopTab);
    }

    public void ChooseButton()
    {
        _iconImage.color = selectedColor;
        scrollTransform.SetActive(true);
        ShopController.Instance.holoScrollRect.content = scrollTransform.GetComponent<RectTransform>();
    }

    public void UnchooseButton()
    {
        _iconImage.color = normalColor;
        scrollTransform.SetActive(false);
    }

    public void FixedUpdate()
    {
        float difference = Mathf.Abs(transform.localScale.x - _desiredScale);
        if (difference > 0.011f)
        {
            if (transform.localScale.x > _desiredScale)
            {
                if (difference < 0.04f)
                {
                    transform.localScale -= new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    transform.localScale -= new Vector3(0.03f, 0.03f, 0);
                }
            }
            else
            {
                if (difference < 0.04f)
                {
                    transform.localScale += new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    transform.localScale += new Vector3(0.03f, 0.03f, 0);
                }
            }
        }
    }

}
