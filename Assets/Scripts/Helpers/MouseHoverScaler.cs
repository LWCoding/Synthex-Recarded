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

    private Transform _transformToScale;

    /// <summary>
    /// Toggles the functionality of scaling the object when the mouse is over it.
    /// </summary>
    /// <param name="isInteractable">Boolean representing if the object will scale when hovered over.</param>
    public void SetIsInteractable(bool isInteractable) => _isInteractable = isInteractable;

    public void Initialize(Transform transformToScale)
    {
        _transformToScale = transformToScale;
        _initialScale = transformToScale.transform.localScale.x;
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
        _desiredScale = _initialScale * 1.08f;
    }

    public void OnMouseExit()
    {
        if (!_isInteractable) { return; }
        _desiredScale = _initialScale;
    }

    public void FixedUpdate()
    {
        Debug.Assert(_transformToScale != null, "Transform to scale not found!", this);
        float difference = Mathf.Abs(_transformToScale.localScale.x - _desiredScale);
        if (difference > 0.011f)
        {
            if (_transformToScale.localScale.x > _desiredScale)
            {
                if (difference < 0.05f)
                {
                    _transformToScale.localScale -= new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    _transformToScale.localScale -= new Vector3(0.03f, 0.03f, 0);
                }
            }
            else
            {
                if (difference < 0.05f)
                {
                    _transformToScale.localScale += new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    _transformToScale.localScale += new Vector3(0.03f, 0.03f, 0);
                }
            }
        }
    }

}
