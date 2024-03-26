using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public abstract class CampaignEventController : MonoBehaviour
{

    public static CampaignEventController Instance;
    [Header("Banner Object Assignments")]
    [SerializeField] private Animator _bannerAnimator;
    [SerializeField] private TextMeshPro _bannerText;

    public Queue<UnityAction> QueuedEvents = new();
    public bool HasEventsQueued => QueuedEvents.Count > 0;
    public bool IsPlayingSingularEvent = false;
    private bool _areAllEventsComplete = true;  // Initially true, false when events are rendered

    public bool IsPlayingAnyEvent => IsPlayingSingularEvent || !_areAllEventsComplete;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    public void Start()
    {
        InitializeMapState();
    }

    // Initializes the states of all map objects from the save file.
    public abstract void InitializeMapState();

    // Renders all events currently in the queue.
    public void RenderAllQueuedEvents()
    {
        StartCoroutine(RenderAllQueuedEventsCoroutine());
    }

    private IEnumerator RenderAllQueuedEventsCoroutine()
    {
        _areAllEventsComplete = false;
        while (HasEventsQueued)
        {
            QueuedEvents.Dequeue().Invoke();
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !IsPlayingSingularEvent);
        }
        _areAllEventsComplete = true;
        if (TransitionManager.Instance.IsScreenDarkened)
        {
            TransitionManager.Instance.ShowScreen(1.25f);
        }
    }

    #region Fade Transition

    public void QueueScreenShow()
    {
        QueuedEvents.Enqueue(() =>
        {
            StartCoroutine(QueueScreenShowCoroutine(true));
        });
    }

    public void QueueScreenShowWithNext()
    {
        QueuedEvents.Enqueue(() =>
        {
            StartCoroutine(QueueScreenShowCoroutine(false));
        });
    }

    private IEnumerator QueueScreenShowCoroutine(bool isStandaloneEvent)
    {
        if (isStandaloneEvent) IsPlayingSingularEvent = true;
        TransitionManager.Instance.ShowScreen(1.25f);
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning);
        if (isStandaloneEvent) IsPlayingSingularEvent = false;
    }

    #endregion

    #region Zoom In On Object Event

    // Plays animation where camera zooms in an object.
    public void QueueZoomOnObject(GameObject obj)
    {
        QueuedEvents.Enqueue(() =>
        {
            ZoomOnObject(obj, 0.6f);
        });
    }

    // Plays animation where camera zooms in an object slowly.
    public void QueueSlowZoomOnObject(GameObject obj)
    {
        QueuedEvents.Enqueue(() =>
        {
            ZoomOnObject(obj, 0.9f);
        });
    }

    // Plays animation where camera zooms in an object without waiting to complete.
    public void QueueZoomOnObjectWithNext(GameObject obj)
    {
        QueuedEvents.Enqueue(() =>
        {
            ZoomOnObject(obj, 0.6f, false);
        });
    }

    // Makes the camera object zoom in on an object.
    private void ZoomOnObject(GameObject obj, float animationTime, bool isStandaloneEvent = true)
    {
        if (isStandaloneEvent) IsPlayingSingularEvent = true;
        CampaignCameraController.Instance.ZoomCameraOnObject(obj, animationTime, 3.25f, 1, () =>
        {
            if (isStandaloneEvent) IsPlayingSingularEvent = false;
        });
    }

    #endregion

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
        IsPlayingSingularEvent = true;
        switch (GameManager.GetGameScene())
        {
            case GameScene.FOREST:
                _bannerText.text = "<color=\"black\"><size=13>Old Woods</size></color>\n<color=#282E27><i><size=5>Chapter 1</size></i></color>";
                break;
            case GameScene.AERICHO:
                _bannerText.text = "<color=\"black\"><size=13>Aericho City</size></color>\n<color=#282E27><i><size=5>Chapter 2</size></i></color>";
                break;
            case GameScene.SECRET:
                _bannerText.text = "<color=\"black\"><size=13>The Secret</size></color>\n<color=#282E27><i><size=5>Hello from Selenium :)</size></i></color>";
                break;
        }
        _bannerAnimator.enabled = true;
        _bannerAnimator.Play("FadeIn");
        StartCoroutine(StopEventWhenAnimationIsFinished(_bannerAnimator, 0, () =>
        {
            _bannerAnimator.enabled = false;
        }));
    }

    #endregion

    #region Restore Camera Event

    // Plays the animation of the dummy getting destroyed.
    public void QueueRestoreCamera()
    {
        QueuedEvents.Enqueue(() =>
        {
            CampaignCameraController.Instance.RestoreCamera();
        });
    }

    #endregion

    // Sets the IsPlayingEvent parameter to false after a specific animator
    // is no longer animating.
    public IEnumerator StopEventWhenAnimationIsFinished(Animator anim, float delayAfter, Action codeToRunAfter = null)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying(anim));
        yield return new WaitForSeconds(delayAfter);
        IsPlayingSingularEvent = false;
        codeToRunAfter?.Invoke();
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
