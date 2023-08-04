using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct UpgradeTutorialInfo
{
    public string tutorialText;
    public Sprite tutorialIcon;
    public Vector2 tutorialIconScale;
}

public class UpgradeIntroController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private GameObject _introScreen;
    [SerializeField] private Animator _introAnimator;
    [SerializeField] private Animator _introLogoAnimator;
    [SerializeField] private Image _introInfoSprite;
    [SerializeField] private TextMeshProUGUI _introInfoText;
    [Header("Cutscene Assignments")]
    [SerializeField] private List<UpgradeTutorialInfo> _tutorialInfo;

    private int _currInfoIdx = 0;

    // When the scene starts, show the intro screen and play the start animation.
    private void Start()
    {
        _introScreen.SetActive(false);
        StartCoroutine(PlayIntroAnimationCoroutine());
    }

    private IEnumerator PlayIntroAnimationCoroutine()
    {
        yield return ShowIntroScreenCoroutine();
        if (!GameController.visitedUpgradeBefore)
        {
            // If we haven't visited the upgrades before, render the first-time animation.
            GameController.visitedUpgradeBefore = true;
            StartCoroutine(WaitForContinue());
        }
        else
        {
            // Or else, just hide the intro screen after showing it.
            StartCoroutine(HideIntroScreenCoroutine());
        }
    }

    private IEnumerator WaitForContinue()
    {
        // Wait for the animation to finish (if any is playing).
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Fade the information out so we can change it.
        _introAnimator.Play("FadeInfoOut");
        // Wait for the animation to finish.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Render the current information, if it exists.
        if (_currInfoIdx < _tutorialInfo.Count)
        {
            UpgradeTutorialInfo info = _tutorialInfo[_currInfoIdx];
            _introInfoSprite.sprite = info.tutorialIcon;
            _introInfoSprite.transform.localScale = info.tutorialIconScale;
            _introInfoText.text = info.tutorialText;
            _introInfoSprite.SetNativeSize();
        }
        // Fade the information back in after it is changed.
        _introAnimator.Play("FadeInfoIn");
        // Wait for the animation to finish.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Wait until time expires OR the user left-clicks in the next frame.
        float currTime = Time.time;
        yield return new WaitUntil(() => { return Time.time - currTime > 3 || (!TopBarController.Instance.IsPlayerInteractingWithTopBar() && !TopBarController.Instance.IsCardPreviewShowing() && !JournalManager.Instance.IsJournalShowing() && !SettingsManager.Instance.IsGamePaused() && Input.GetMouseButtonDown(0)); });
        // Increment the index to get our next animation.
        _currInfoIdx++;
        if (_currInfoIdx < _tutorialInfo.Count)
        {
            // If we still have more animations to render, render them.
            StartCoroutine(WaitForContinue());
        }
        else
        {
            // Or else, hide this screen.
            StartCoroutine(HideIntroScreenCoroutine());
        }
    }

    private IEnumerator ShowIntroScreenCoroutine()
    {
        _introScreen.SetActive(true);
        _introLogoAnimator.gameObject.SetActive(false);
        // Wait until the fade transition is not animating anymore.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !FadeTransitionController.Instance.IsScreenTransitioning());
        // Animate the intro logo in.
        _introLogoAnimator.gameObject.SetActive(true);
        _introLogoAnimator.Play("Show");
        // Wait until the animation is finished.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introLogoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Then, play the animation for the entire screen. Disable the logo animator.
        _introLogoAnimator.enabled = false;
        _introAnimator.Play("Show");
        // Wait until the animation is finished.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Wait for some additional time.
        yield return new WaitForSeconds(2);
    }

    private IEnumerator HideIntroScreenCoroutine()
    {
        // Wait for the animation to finish (if any is playing).
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        _introAnimator.Play("Hide");
        // Wait until we're done playing the hide animation.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        _introScreen.SetActive(false);
    }

}
