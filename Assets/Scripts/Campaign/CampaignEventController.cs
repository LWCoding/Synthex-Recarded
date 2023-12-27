using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Playables;

public class CampaignEventController : MonoBehaviour
{

    public static CampaignEventController Instance;
    [Header("Banner Object Assignments")]
    [SerializeField] private PlayableDirector _cameraDirector;
    [SerializeField] private TextMeshPro _bannerText;
    [Header("Dummy Object Assignments")]
    [SerializeField] private SpriteRenderer _dummySpriteRenderer;
    [SerializeField] private Animator _dummyAnimator;
    [SerializeField] private ParticleSystem _dummyParticleSystem;
    [SerializeField] private Sprite _intactDummy;
    [SerializeField] private Sprite _destroyedDummy;
    [SerializeField] private AudioClip _dummyDestroyedSFX;
    [Header("Ryan Object Assignments")]
    [SerializeField] private Transform _ryanTransform;
    [SerializeField] private ParticleSystem _ryanParticleSystem;
    [SerializeField] private AudioClip _footstepsSFX;

    public Queue<UnityAction> QueuedEvents = new Queue<UnityAction>();
    public bool HasEventsQueued => QueuedEvents.Count > 0;
    public bool IsPlayingEvent = false;

    private bool _areAllEventsComplete = false; 

    /// <summary>
    /// If no events are currently playing, this returns True. Else False.
    /// Should be called by functions that plan to cause a screen refresh. 
    /// (Functions that should not play during cutscenes.)
    /// </summary>
    /// <returns>A boolean representing if events are all complete. (Good to go!)</returns>
    public bool AreEventsComplete() => !IsPlayingEvent && _areAllEventsComplete; 

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
                bool defeatedDummy = EventManager.IsEventComplete(EventType.DEFEATED_DUMMY);
                _dummySpriteRenderer.sprite = defeatedDummy ? _destroyedDummy : _intactDummy;
                break;
        }
    }

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
            yield return new WaitUntil(() => !IsPlayingEvent);
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
        if (isStandaloneEvent) IsPlayingEvent = true;
        TransitionManager.Instance.ShowScreen(1.25f);
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning);
        if (isStandaloneEvent) IsPlayingEvent = false;
    }

    #endregion

    // Adds a follower transform. These follow the player when they move.
    public void AddTransformToFollower(Transform transform)
    {
        CampaignController.Instance.HeroFollowerTransforms.Add(transform);
    }

    #region Move Ryan To Position

    // Plays the animation of the dummy getting destroyed.
    public void QueueMoveRyanToPosition(Transform positionToMoveTo)
    {
        QueuedEvents.Enqueue(() =>
        {
            StartCoroutine(MoveRyanToPositionCoroutine(positionToMoveTo.position));
        });
    }

    // Instantly sends Ryan's icon to a certain position.
    public void InstantMoveRyanToPosition(Transform positionToMoveTo)
    {
        _ryanTransform.position = positionToMoveTo.position;
    }

    private IEnumerator MoveRyanToPositionCoroutine(Vector3 targetPosition)
    {
        IsPlayingEvent = true;
        float currTime = 0;
        float timeToWait = 0.7f;
        float timeSinceLastParticle = 0;
        float particleCooldown = 0.15f;
        SoundManager.Instance.PlayOneShot(_footstepsSFX, 0.22f);
        Vector3 initialPosition = _ryanTransform.position;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            timeSinceLastParticle += Time.deltaTime;
            _ryanTransform.position = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            if (timeSinceLastParticle > particleCooldown)
            {
                _ryanParticleSystem.Emit(1);
                timeSinceLastParticle = 0;
            }
            yield return null;
        }
        IsPlayingEvent = false;
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
        _cameraDirector.Play();
        StartCoroutine(StopEventWhenDirectorIsFinished());
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
        EventManager.CompleteEvent(EventType.DEFEATED_DUMMY);
        StartCoroutine(StopEventWhenAnimationIsFinished(_dummyAnimator, 1));
    }

    #endregion

    // Sets the IsPlayingEvent parameter to false after a specific animator
    // is no longer animating.
    private IEnumerator StopEventWhenAnimationIsFinished(Animator anim, float delayAfter = 0, Action codeToRunAfter = null)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying(anim));
        yield return new WaitForSeconds(delayAfter);
        IsPlayingEvent = false;
        codeToRunAfter?.Invoke();
    }

    // Sets the IsPlayingEvent parameter to false after a specific animator
    // is no longer animating.
    private IEnumerator StopEventWhenDirectorIsFinished(float delayAfter = 0, Action codeToRunAfter = null)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _cameraDirector.state != PlayState.Playing);
        yield return new WaitForSeconds(delayAfter);
        IsPlayingEvent = false;
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
