using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TitleUIButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{

    public static bool IsTitleButtonSelected = false;
    [Header("Object Assignments")]
    public UnityEvent OnClick = new UnityEvent();
    [Header("Sound Assignments")]
    public AudioClip buttonHoverSFX;
    public AudioClip buttonSelectSFX;
    private bool _isClickable = true;
    private Animator _buttonAnimator;

    // When the title screen loads, make sure we don't have a title button already selected.
    private void OnEnable()
    {
        IsTitleButtonSelected = false;
        _buttonAnimator = GetComponent<Animator>();
    }

    public void SetIsClickable(bool isButtonClickable)
    {
        _isClickable = isButtonClickable;
        if (!isButtonClickable)
        {
            _buttonAnimator.Play("Unselectable", -1, 0f);
        }
    }

    public void OnPointerEnter(PointerEventData ped)
    {
        if (IsTitleButtonSelected || !_isClickable) { return; }
        _buttonAnimator.Play("Glow", -1, 0f);
        SoundManager.Instance.PlayOneShot(buttonHoverSFX, 1);
    }

    public void OnPointerExit(PointerEventData ped)
    {
        if (IsTitleButtonSelected || !_isClickable) { return; }
        _buttonAnimator.Play("Unglow", -1, 0f);
    }

    public void OnPointerClick(PointerEventData ped)
    {
        if (IsTitleButtonSelected || !_isClickable) { return; }
        IsTitleButtonSelected = true;
        StartCoroutine(OnPointerClickCoroutine());
        SoundManager.Instance.PlayOneShot(buttonSelectSFX);
    }

    private IEnumerator OnPointerClickCoroutine()
    {
        _buttonAnimator.Play("Glow", -1, 1f);
        yield return new WaitForEndOfFrame();
        foreach (Animator anim in TitleController.Instance.allButtonAnimators)
        {
            anim.enabled = true;
            if (anim != _buttonAnimator)
            {
                anim.Play("Disappear");
            }
            else
            {
                anim.Play("Flash");
            }
        }
        yield return new WaitForSeconds(1.2f);
        _buttonAnimator.Play("Disappear");
        yield return new WaitForSeconds(0.2f);
        OnClick.Invoke();
    }

}
