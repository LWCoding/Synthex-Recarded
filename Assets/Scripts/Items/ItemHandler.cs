using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(ItemEffectRenderer))]
public class ItemHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    [Header("Object Assignments")]
    [SerializeField] private GameObject imageObject;
    [SerializeField] private GameObject tooltipParentObject;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private GameObject itemFlashObject;
    [SerializeField] private Image verifyChoiceImage;

    [HideInInspector] public Item itemInfo;
    private ItemEffectRenderer _itemEffectRenderer;
    private Image _itemFlashImage;
    private int _timesItemClicked = 0; // 1 = show checkmark, 2 = use item
    private bool _canClickToUse = true;
    private bool _showLocalTooltipOnHover;
    private bool _showUITooltipOnHover;
    private float _initialScale;
    private float _desiredScale;
    private int _itemIndex; // Index in the player's inventory.
    private IEnumerator _itemFlashCoroutine = null;
    private Canvas _itemCanvas;
    private Canvas _tooltipCanvas;

    private void Awake()
    {
        _itemCanvas = GetComponent<Canvas>();
        _itemFlashImage = itemFlashObject.GetComponent<Image>();
        _tooltipCanvas = tooltipParentObject.GetComponent<Canvas>();
        _itemEffectRenderer = GetComponent<ItemEffectRenderer>();
    }

    // Initialize the item's information.
    public void Initialize(Item item, bool showLocalTooltipOnHover, bool showUITooltipOnHover, int itemIndex = -1)
    {
        // Set the item's index in the inventory (if necessary)
        _itemIndex = itemIndex;
        // Make item show properly.
        SetItemImageScale(1, 1);
        // Set all of the basic properties
        tooltipParentObject.SetActive(false);
        itemFlashObject.SetActive(false);
        GetComponent<ShopItemHandler>().enabled = false;
        HideVerifyChoiceImage();
        _initialScale = imageObject.transform.localScale.x;
        _desiredScale = _initialScale;
        _showLocalTooltipOnHover = showLocalTooltipOnHover;
        _showUITooltipOnHover = showUITooltipOnHover;
        // Set the item information
        itemInfo = item;
        nameText.text = item.itemName;
        // Set the description text correctly after replacing variable values.
        string desc = item.itemDesc;
        for (int i = 0; i < item.variables.Count; i++)
        {
            desc = desc.Replace("[" + i.ToString() + "]", item.variables[i].ToString());
        }
        descText.text = desc;
        imageObject.GetComponent<Image>().sprite = item.itemImage;
        _itemFlashImage.sprite = item.itemImage;
    }

    // Flash the item in an animation.
    public void FlashItem()
    {
        if (_itemFlashCoroutine != null)
        {
            StopCoroutine(_itemFlashCoroutine);
        }
        _itemFlashCoroutine = FlashItemCoroutine();
        StartCoroutine(_itemFlashCoroutine);
    }

    public void DisableTooltip()
    {
        _showLocalTooltipOnHover = false;
        tooltipParentObject.SetActive(false);
    }

    private IEnumerator FlashItemCoroutine()
    {
        // Calculate frames and initial values for linear interpolation.
        float currTime = 0;
        float timeToWait = 0.7f;
        itemFlashObject.SetActive(true);
        Vector3 initialFlashScale = new Vector3(_initialScale + 0.3f, _initialScale + 0.3f, 1);
        Vector3 targetFlashScale = new Vector3(_initialScale, _initialScale, 1);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _itemFlashImage.transform.localScale = Vector3.Lerp(initialFlashScale, targetFlashScale, currTime / timeToWait);
            yield return null;
        }
        itemFlashObject.SetActive(false);
        _itemFlashCoroutine = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // If we're not a valid item, don't show any tooltip.
        if (itemInfo.type == ItemType.NONE) { return; }
        // Or else, show the local OR UI tooltip depending on the circumstances.
        if (_showLocalTooltipOnHover) { tooltipParentObject.SetActive(true); }
        if (_showUITooltipOnHover)
        {
            TopBarController.Instance.ShowTopBarItemTooltip(itemInfo);
            TopBarController.Instance.UpdateItemVerifyText(GetIsItemPlayable(), false);
        }
        if (_itemFlashCoroutine != null) { return; }
        _desiredScale = _initialScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemInfo.type == ItemType.NONE) { return; }
        _timesItemClicked = 0;
        HideVerifyChoiceImage();
        tooltipParentObject.SetActive(false);
        TopBarController.Instance.HideTopBarItemTooltip();
        if (_itemFlashCoroutine != null) { return; }
        _desiredScale = _initialScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GetIsItemPlayable()) { return; }
        if (itemInfo.type != ItemType.NONE && _canClickToUse)
        {
            _timesItemClicked++;
            // If we've clicked the first time, prompt to confirm.
            if (_timesItemClicked == 1)
            {
                ShowVerifyChoiceImage();
                TopBarController.Instance.UpdateItemVerifyText(true, true);
                SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER, 1.6f);
            }
            // If we've clicked a second time, use the item!
            if (_timesItemClicked >= 2)
            {
                _itemEffectRenderer.RenderEffects(itemInfo);
                GameController.RemoveItemInInventory(_itemIndex);
                TopBarController.Instance.RenderItems();
            }
        }
    }

    public bool GetIsItemPlayable()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // If the item is a placeholder, don't even work.
        if (itemInfo.type == ItemType.NONE)
        {
            return false;
        }
        if (itemInfo.useCase == ItemUseCase.ONLY_IN_BATTLE && sceneName != "Battle")
        {
            return false;
        }
        return true;
    }

    public void SetItemImageScale(float scale, float tooltipScale)
    {
        _initialScale = scale;
        _desiredScale = scale;
        imageObject.transform.localScale = new Vector3(scale, scale, 1);
        tooltipParentObject.transform.localScale = new Vector3(tooltipScale, tooltipScale, 1);
    }

    public void SetSortingOrder(int sortingOrder)
    {
        _itemCanvas.sortingOrder = sortingOrder;
        _tooltipCanvas.sortingOrder = sortingOrder + 1;
    }

    public void EnableShopFunctionality()
    {
        _canClickToUse = false;
        GetComponent<ShopItemHandler>().enabled = true;
        tooltipParentObject.transform.localPosition = new Vector2(0, tooltipParentObject.transform.localPosition.y);
        tooltipParentObject.transform.localScale = new Vector3(0.6f, 0.6f);
    }

    public void ShowVerifyChoiceImage()
    {
        verifyChoiceImage.enabled = true;
    }

    public void HideVerifyChoiceImage()
    {
        verifyChoiceImage.enabled = false;
    }

    public void FixedUpdate()
    {
        if (_itemFlashCoroutine != null) { return; }
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
