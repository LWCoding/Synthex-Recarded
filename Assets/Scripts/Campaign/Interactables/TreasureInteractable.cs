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
    [SerializeField] private Sprite _unlockedSprite;
    [SerializeField] private Sprite _lockedSprite;

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
        TreasureController.Instance.ShowChest();
        EventManager.CompleteEvent(_chestEvent);
        SetSpriteBasedOnState();
    }

    public void OnLocationEnter()
    {
        _isInteractable = !EventManager.IsEventComplete(_chestEvent);
        if (!_isInteractable) { return; }
        _popupObject.SetActive(true);
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
        _spriteRendererToChange.sprite = EventManager.IsEventComplete(_chestEvent) ? _unlockedSprite : _lockedSprite;
    }

}
