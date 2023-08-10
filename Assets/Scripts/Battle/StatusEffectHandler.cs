using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusEffectHandler : MonoBehaviour
{

    public Effect effectType;
    [SerializeField] private SpriteRenderer _statusIconSprite;
    [SerializeField] private SpriteRenderer _statusIconAnimationSprite;
    [SerializeField] private TextMeshPro _statusCountText;
    [SerializeField] private GameObject _tooltipParentObject;
    [SerializeField] private TextMeshPro _statusDescription;

    private float _initialScale;
    private IEnumerator _flashIconCoroutine = null;

    private void Awake()
    {
        _tooltipParentObject.SetActive(false);
    }

    public void UpdateStatus(StatusEffect s)
    {
        effectType = s.statusInfo.type;
        _statusDescription.text = "<b>" + s.statusInfo.statusName + " " + s.amplifier.ToString() + ":</b>\n" + s.statusInfo.statusDescription.Replace("[X]", s.amplifier.ToString()).Replace("[S]", s.specialValue);
        _statusIconSprite.sprite = s.statusInfo.statusIcon;
        _statusIconAnimationSprite.sprite = s.statusInfo.statusIcon;
        _statusCountText.text = s.amplifier.ToString();
        _statusIconSprite.transform.localScale = s.statusInfo.iconSpriteScale;
        _initialScale = s.statusInfo.iconSpriteScale.x;
        // If the icon should flash, do it, and make sure it doesn't flash again
        // until updated.
        if (s.shouldActivate)
        {
            FlashIcon(s);
            s.shouldActivate = false;
        }
        else if (_flashIconCoroutine == null)
        {
            _statusIconAnimationSprite.color = new Color(1, 1, 1, 0);
        }
    }

    public void FlashIcon(StatusEffect s)
    {
        if (!gameObject.activeInHierarchy) { return; }
        if (_flashIconCoroutine != null)
        {
            StopCoroutine(_flashIconCoroutine);
        }
        _flashIconCoroutine = FlashIconCoroutine(s);
        StartCoroutine(_flashIconCoroutine);
    }

    private IEnumerator FlashIconCoroutine(StatusEffect s)
    {
        Color animInitialColor = new Color(1, 1, 1, 1);
        Color animTargetColor = new Color(1, 1, 1, 0);
        Vector3 animInitialScale = new Vector3(_initialScale + 0.3f, _initialScale + 0.3f, 1);
        Vector3 animTargetScale = new Vector3(_initialScale, _initialScale);
        float currTime = 0;
        float targetTime = 0.6f;
        while (currTime < targetTime)
        {
            currTime += Time.deltaTime;
            _statusIconAnimationSprite.color = Color.Lerp(animInitialColor, animTargetColor, currTime / targetTime);
            _statusIconAnimationSprite.transform.localScale = Vector3.Lerp(animInitialScale, animTargetScale, currTime / targetTime);
            yield return null;
        }
    }

    public void OnMouseEnter()
    {
        _tooltipParentObject.SetActive(true);
    }

    public void OnMouseExit()
    {
        _tooltipParentObject.SetActive(false);
    }

}
