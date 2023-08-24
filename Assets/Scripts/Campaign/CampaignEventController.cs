using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CampaignEventController : MonoBehaviour
{

    public static CampaignEventController Instance;
    [Header("Dummy Object Assignments")]
    [SerializeField] private SpriteRenderer _dummySpriteRenderer;
    [SerializeField] private Animator _dummyAnimator;
    [SerializeField] private ParticleSystem _dummyParticleSystem;
    [SerializeField] private Sprite _intactDummy;
    [SerializeField] private Sprite _destroyedDummy;
    [SerializeField] private AudioClip _dummyDestroyedSFX;

    public Queue<UnityAction> QueuedEvents = new Queue<UnityAction>();
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
    public void RenderAllQueuedEvents()
    {
        StartCoroutine(RenderAllQueuedEventsCoroutine());
    }

    private IEnumerator RenderAllQueuedEventsCoroutine()
    {
        AreAllEventsComplete = false;
        yield return new WaitUntil(() => !TransitionManager.Instance.IsScreenTransitioning());
        while (QueuedEvents.Count > 0)
        {
            QueuedEvents.Dequeue().Invoke();
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !IsPlayingEvent);
        }
        AreAllEventsComplete = true;
    }

    // Plays the animation of the dummy getting destroyed.
    public void DestroyDummy()
    {
        QueuedEvents.Enqueue(() =>
        {
            IsPlayingEvent = true;
            _dummyParticleSystem.Play();
            _dummyAnimator.Play("Destroy");
            SoundManager.Instance.PlayOneShot(_dummyDestroyedSFX, 1.2f);
            GameManager.CompleteEvent(EventType.DEFEATED_DUMMY);
            StartCoroutine(StopEventWhenAnimationIsFinished(_dummyAnimator));
        });
    }

    // Sets the IsPlayingEvent parameter to false after a specific animator
    // is no longer animating.

    private IEnumerator StopEventWhenAnimationIsFinished(Animator anim)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying(anim));
        yield return new WaitForSeconds(1);
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
