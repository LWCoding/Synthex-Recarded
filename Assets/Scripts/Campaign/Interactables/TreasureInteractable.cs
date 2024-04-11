using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureInteractable : MonoBehaviour, IInteractable
{

    [Header("Object Assignments")]
    [SerializeField] private MouseHoverScaler _spriteObjectScaler;
    [SerializeField] private SpriteRenderer _spriteRendererToChange;
    [SerializeField] private GameObject _popupObject;
    [Header("Chest Assignments")]
    [SerializeField] private EventType _chestEvent;
    [Tooltip("Sprite that the chest will appear as if already opened")]
    [SerializeField] private Sprite _alreadyOpenedSprite;
    [Tooltip("Sprite that the chest will appear as if not opened yet")]
    [SerializeField] private Sprite _notYetOpenedSprite;

    private bool _isInteractable = false;

    public void Awake()
    {
        _spriteObjectScaler.Initialize(_spriteObjectScaler.transform);
        _spriteObjectScaler.SetIsInteractable(true);
        SetSpriteBasedOnState();
        OnLocationExit();
    }

    public void OnInteract()
    {
        if (!_isInteractable) { return; }
        _isInteractable = false;
        EventManager.CompleteEvent(_chestEvent);
        TreasureController.Instance.ShowChest();
    }

    public void OnLocationEnter()
    {
        // Enable interactions with chest IF player hasn't opened it before
        _isInteractable = !EventManager.IsEventComplete(_chestEvent);
        _popupObject.SetActive(_isInteractable);
        if (!_isInteractable) { return; }
        StartCoroutine(CheckForInteractCoroutine());
    }

    public void OnLocationExit()
    {
        _popupObject.SetActive(false);
        _isInteractable = false;
        _spriteObjectScaler.ResetScale();
        StopAllCoroutines();
    }

    private IEnumerator CheckForInteractCoroutine()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInteract();
            }
            yield return null;
        }
    }

    public void OnMouseDown()
    {
        if (_isInteractable)
        {
            OnInteract();
        }
    }

    public void OnMouseOver()
    {
        if (_isInteractable)
        {
            _spriteObjectScaler.OnMouseEnter();
        }
    }

    public void OnMouseExit()
    {
        if (_isInteractable)
        {
            _spriteObjectScaler.OnMouseExit();
        }
    }

    private void SetSpriteBasedOnState()
    {
        _spriteRendererToChange.sprite = EventManager.IsEventComplete(_chestEvent) ? _alreadyOpenedSprite : _notYetOpenedSprite;
    }

}
