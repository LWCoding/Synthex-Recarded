using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CampaignEventController : MonoBehaviour
{

    public static CampaignEventController Instance;
    [Header("Banner Object Assignments")]
    [SerializeField] private Animator _bannerAnimator;
    [SerializeField] private TextMeshPro _bannerText;
    [Header("Dummy Object Assignments")]
    [SerializeField] private SpriteRenderer _dummySpriteRenderer;
    [SerializeField] private Animator _dummyAnimator;
    [SerializeField] private ParticleSystem _dummyParticleSystem;
    [SerializeField] private Sprite _intactDummy;
    [SerializeField] private Sprite _destroyedDummy;
    [SerializeField] private AudioClip _dummyDestroyedSFX;

    public Queue<UnityAction> QueuedEvents = new Queue<UnityAction>();
    public bool HasEventsQueued => QueuedEvents.Count > 0;
    public bool IsPlayingEvent = false;
    public bool AreAllEventsComplete = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeMapState(GameManager.GetGameScene());
    }

    // Initializes the states of all map objects from the save file.
    private void InitializeMapState(GameScene area)
    {
        switch (area)
        {
            case GameScene.FOREST:
                // Set dummy to either be intact or destroyed.
                bool defeatedDummy = GameManager.IsEventComplete(EventType.DEFEATED_DUMMY);
                _dummySpriteRenderer.sprite = (defeatedDummy) ? _destroyedDummy : _intactDummy;
                break;
        }
    }

    // Renders all events currently in the queue.
    public void RenderAllQueuedEvents(bool shouldTransitionScreen)
    {
        StartCoroutine(RenderAllQueuedEventsCoroutine(shouldTransitionScreen));
    }

    private IEnumerator RenderAllQueuedEventsCoroutine(bool shouldTransitionScreen)
    {
        AreAllEventsComplete = false;
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning);
        if (QueuedEvents.Count > 0 && shouldTransitionScreen)
        {
            TransitionManager.Instance.ShowScreen(1.25f);
        }
        while (QueuedEvents.Count > 0)
        {
            QueuedEvents.Dequeue().Invoke();
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !IsPlayingEvent);
        }
        AreAllEventsComplete = true;
    }

    #region Show Banner Event

    // Plays the animation of the dummy getting destroyed.
    public void QueuePlayBanner()
    {
        QueuedEvents.Enqueue(() =>
        {
            PlayBanner();
        });
    }

    // Plays the banner animation.
    private void PlayBanner()
    {
        IsPlayingEvent = true;
        switch (GameManager.GetGameScene())
        {
            case GameScene.FOREST:
                _bannerText.text = "<color=\"black\"><size=13>The Forest</size></color>\n<color=#282E27><i><size=5>Chapter 1</size></i></color>";
                break;
            case GameScene.AERICHO:
                _bannerText.text = "<color=\"black\"><size=13>Aericho City</size></color>\n<color=#282E27><i><size=5>Chapter 2</size></i></color>";
                break;
            case GameScene.SECRET:
                _bannerText.text = "<color=\"black\"><size=13>The Secret</size></color>\n<color=#282E27><i><size=5>Hello from Selenium :)</size></i></color>";
                break;
        }
        _bannerAnimator.Play("FadeIn");
        StartCoroutine(StopEventWhenAnimationIsFinished(_bannerAnimator, 0));
    }

    #endregion

    #region Destroy Dummy Event

    // Plays the animation of the dummy getting destroyed.
    public void QueueDestroyDummy()
    {
        QueuedEvents.Enqueue(() =>
        {
            DestroyDummy();
        });
    }

    // Plays the animation of the dummy getting destroyed.
    private void DestroyDummy()
    {
        IsPlayingEvent = true;
        _dummyParticleSystem.Play();
        _dummyAnimator.Play("Destroy");
        SoundManager.Instance.PlayOneShot(_dummyDestroyedSFX, 1.2f);
        GameManager.CompleteEvent(EventType.DEFEATED_DUMMY);
        StartCoroutine(StopEventWhenAnimationIsFinished(_dummyAnimator, 1));
    }

    #endregion

    // Sets the IsPlayingEvent parameter to false after a specific animator
    // is no longer animating.

    private IEnumerator StopEventWhenAnimationIsFinished(Animator anim, float delayAfter)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying(anim));
        yield return new WaitForSeconds(delayAfter);
        IsPlayingEvent = false;
    }

    /*
        Returns a boolean representing whether or not the specified animator is
        playing an animation clip with the specified name.
    */
    private bool IsPlaying(Animator anim)
    {
        return anim.GetCurrentAnimatorStateInfo(0).length > anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

}
