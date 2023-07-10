using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalUIController : MonoBehaviour
{

    public static GlobalUIController Instance;
    public GameObject _saveIconContainer;

    // This Awake function runs on the first time the bar is instantiated.
    private void Awake()
    {
        // Set this to the Instance if it is the first one.
        // Or else, destroy this.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
        // Make sure this object isn't destroyed.
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        _saveIconContainer.SetActive(false);
    }

    public void PlaySaveIconAnimation()
    {
        StartCoroutine(SaveIconAnimationCoroutine());
    }

    private IEnumerator SaveIconAnimationCoroutine()
    {
        _saveIconContainer.SetActive(true);
        Animator iconAnimator = _saveIconContainer.GetComponent<Animator>();
        // Start the animation.
        iconAnimator.enabled = true;
        iconAnimator.speed = 1;
        iconAnimator.Play("FadeIn");
        iconAnimator.Play("Shake");
        yield return new WaitForSeconds(3);
        // Fade out the animation.
        iconAnimator.Play("FadeOut");
        iconAnimator.speed = 0.6f;
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying(iconAnimator, 1));
        // Stop the animation.
        iconAnimator.enabled = false;
        _saveIconContainer.SetActive(false);
    }

    /*
        Returns a boolean representing whether or not the specified animator is
        playing an animation clip with the specified name.
    */
    public bool IsPlaying(Animator animator, int layer)
    {
        return animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f;
    }

}
