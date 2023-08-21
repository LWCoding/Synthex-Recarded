using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class CampaignArrowHandler : MonoBehaviour
{

    [HideInInspector] public UnityEvent OnClick;

    private SpriteRenderer _arrowSpriteRenderer;
    private IEnumerator _alphaChangingCoroutine = null;

    private void Awake()
    {
        _arrowSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InstantlyHideArrow()
    {
        Color currColor = _arrowSpriteRenderer.color;
        _arrowSpriteRenderer.color = new Color(currColor.r, currColor.g, currColor.a, 0);
    }

    public void ShowArrow()
    {
        if (_alphaChangingCoroutine != null)
        {
            StopCoroutine(_alphaChangingCoroutine);
        }
        _alphaChangingCoroutine = LerpArrowAlphaCoroutine(1, 0.6f);
        StartCoroutine(_alphaChangingCoroutine);
    }

    public void HideArrow()
    {
        if (_alphaChangingCoroutine != null)
        {
            StopCoroutine(_alphaChangingCoroutine);
        }
        _alphaChangingCoroutine = LerpArrowAlphaCoroutine(0, 0.2f);
        StartCoroutine(_alphaChangingCoroutine);
    }

    public void OnMouseDown()
    {
        OnClick.Invoke();
    }

    private IEnumerator LerpArrowAlphaCoroutine(float targetAlpha, float timeToWait)
    {
        Color initialColor = _arrowSpriteRenderer.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);
        float currTime = 0;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _arrowSpriteRenderer.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        _arrowSpriteRenderer.color = targetColor;
    }

}
