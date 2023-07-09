using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusController : MonoBehaviour
{

    public Effect effectType;
    public SpriteRenderer statusIconSprite;
    public SpriteRenderer statusIconAnimationSprite;
    public TextMeshPro statusCountText;
    public GameObject tooltipParentObject;
    public TextMeshPro statusDescription;
    private float initialScale;
    private IEnumerator _flashIconCoroutine = null;

    private void Awake()
    {
        tooltipParentObject.SetActive(false);
    }

    public void UpdateStatus(StatusEffect s)
    {
        effectType = s.statusInfo.type;
        statusDescription.text = "<b>" + s.statusInfo.statusName + " " + s.amplifier.ToString() + ":</b>\n" + s.statusInfo.statusDescription.Replace("[X]", s.amplifier.ToString()).Replace("[S]", s.specialValue);
        statusIconSprite.sprite = s.statusInfo.statusIcon;
        statusIconAnimationSprite.sprite = s.statusInfo.statusIcon;
        statusCountText.text = s.amplifier.ToString();
        statusIconSprite.transform.localScale = s.statusInfo.iconSpriteScale;
        initialScale = s.statusInfo.iconSpriteScale.x;
        // If the icon should flash, do it, and make sure it doesn't flash again
        // until updated.
        if (s.shouldActivate)
        {
            FlashIcon(s);
            s.shouldActivate = false;
        }
        else if (_flashIconCoroutine == null)
        {
            statusIconAnimationSprite.color = new Color(1, 1, 1, 0);
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
        Vector3 animInitialScale = new Vector3(initialScale + 0.3f, initialScale + 0.3f, 1);
        Vector3 animTargetScale = new Vector3(initialScale, initialScale);
        float currTime = 0;
        float targetTime = 0.6f;
        while (currTime < targetTime)
        {
            currTime += Time.deltaTime;
            statusIconAnimationSprite.color = Color.Lerp(animInitialColor, animTargetColor, currTime / targetTime);
            statusIconAnimationSprite.transform.localScale = Vector3.Lerp(animInitialScale, animTargetScale, currTime / targetTime);
            yield return null;
        }
    }

    public void OnMouseEnter()
    {
        tooltipParentObject.SetActive(true);
    }

    public void OnMouseExit()
    {
        tooltipParentObject.SetActive(false);
    }

}
