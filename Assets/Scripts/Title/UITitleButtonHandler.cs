using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class UITitleButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{

    [Header("Object Assignments")]
    public UnityEvent OnClick = new UnityEvent();
    [Header("Sound Assignments")]
    public AudioClip buttonSelectSFX;

    private bool _isClickable = true;
    private Animator _buttonAnimator;

    // When the title screen loads, make sure we don't have a title button already selected.
    private void Awake()
    {
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
        if (TitleController.Instance.CanSelectTitleButton || !_isClickable) { return; }
        _buttonAnimator.Play("Glow", -1, 0f);
        SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
    }

    public void OnPointerExit(PointerEventData ped)
    {
        if (TitleController.Instance.CanSelectTitleButton || !_isClickable) { return; }
        _buttonAnimator.Play("Unglow", -1, 0f);
    }

    public void OnPointerClick(PointerEventData ped)
    {
        if (TitleController.Instance.CanSelectTitleButton || !_isClickable) { return; }
        TitleController.Instance.CanSelectTitleButton = true;
        StartCoroutine(OnPointerClickCoroutine());
        SoundManager.Instance.PlayOneShot(buttonSelectSFX);
    }

    public void PlayAnimation(string animationName)
    {
        _buttonAnimator.enabled = true;
        _buttonAnimator.Play(animationName);
    }

    private IEnumerator OnPointerClickCoroutine()
    {
        _buttonAnimator.Play("Glow", -1, 1f);
        yield return new WaitForEndOfFrame();
        foreach (UITitleButtonHandler titleButton in TitleController.Instance.AllTitleButtons)
        {
            if (titleButton != this)
            {
                titleButton.PlayAnimation("Disappear");
            }
            else
            {
                titleButton.PlayAnimation("Flash");
            }
        }
        yield return new WaitForSeconds(1.2f);
        PlayAnimation("Disappear");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsButtonAnimating());
        yield return new WaitForSeconds(0.1f);
        OnClick.Invoke();
    }

    /*
        Returns a boolean representing whether or not the specified animator is
        playing an animation clip with the specified name.
    */
    public bool IsButtonAnimating()
    {
        return _buttonAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

}
