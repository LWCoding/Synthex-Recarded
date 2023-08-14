using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHoverScaler : MonoBehaviour
{

    private float _initialScale;
    private float _desiredScale;
    private bool _isInteractable = false;
    private EventSystem _eventSystem;

    private SpriteRenderer _spriteRendererToScale;

    public void SetIsInteractable(bool isInteractable) => _isInteractable = isInteractable;

    private void Awake()
    {
        _eventSystem = EventSystem.current;
    }

    public void Initialize(SpriteRenderer spriteRendererToScale)
    {
        _spriteRendererToScale = spriteRendererToScale;
        _initialScale = spriteRendererToScale.transform.localScale.x;
        _desiredScale = _initialScale;
    }

    public void ScaleTo(float desiredScale)
    {
        _desiredScale = desiredScale;
    }

    public void ResetScale()
    {
        _desiredScale = _initialScale;
    }

    public void OnMouseEnter()
    {
        if (!_isInteractable) { return; }
        _desiredScale = _initialScale + 0.1f;
    }

    private void OnMouseExit()
    {
        if (!_isInteractable) { return; }
        _desiredScale = _initialScale;
    }

    private void OnMouseDown()
    {
        // If we're hovering over something else or shouldn't have the
        // ability to interact, don't interact.
        if (_eventSystem.IsPointerOverGameObject() || !_isInteractable) return;
    }

    public void FixedUpdate()
    {
        if (_spriteRendererToScale == null)
        {
            Debug.LogError("MouseHoverScaler on " + gameObject.name + " requires a SpriteRenderer to scale.");
            Destroy(this);
        }
        float difference = Mathf.Abs(_spriteRendererToScale.transform.localScale.x - _desiredScale);
        if (difference > 0.011f)
        {
            if (_spriteRendererToScale.transform.localScale.x > _desiredScale)
            {
                if (difference < 0.05f)
                {
                    _spriteRendererToScale.transform.localScale -= new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    _spriteRendererToScale.transform.localScale -= new Vector3(0.03f, 0.03f, 0);
                }
            }
            else
            {
                if (difference < 0.05f)
                {
                    _spriteRendererToScale.transform.localScale += new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    _spriteRendererToScale.transform.localScale += new Vector3(0.03f, 0.03f, 0);
                }
            }
        }
    }

}
