using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestEventController : CampaignEventController
{

    [Header("Dummy Object Assignments")]
    [SerializeField] private SpriteRenderer _dummySpriteRenderer;
    [SerializeField] private Animator _dummyAnimator;
    [SerializeField] private ParticleSystem _dummyParticleSystem;
    [SerializeField] private Sprite _intactDummy;
    [SerializeField] private Sprite _destroyedDummy;
    [SerializeField] private AudioClip _dummyDestroyedSFX;
    [Header("Gate Object Assignments")]
    [SerializeField] private SpriteRenderer _firstGateObject;
    [SerializeField] private Sprite _closedGate;
    [SerializeField] private Sprite _openGate;
    [Header("Ryan Object Assignments")]
    [SerializeField] private Transform _ryanTransform;
    [SerializeField] private ParticleSystem _ryanParticleSystem;
    [SerializeField] private AudioClip _footstepsSFX;

    // Initializes the states of all map objects from the save file.
    public override void InitializeMapState()
    {
        // Set dummy to either be intact or destroyed.
        bool defeatedDummy = EventManager.IsEventComplete(EventType.DEFEATED_DUMMY);
        _dummySpriteRenderer.sprite = defeatedDummy ? _destroyedDummy : _intactDummy;
        // Set first gate to either be open or closed
        bool gateOpen = EventManager.IsEventComplete(EventType.FOREST_GATE_001);
        _firstGateObject.sprite = gateOpen ? _openGate : _closedGate;
    }

    #region Add Follower Transform

    // Adds a follower transform. These follow the player when they move.
    public void AddTransformToFollower(Transform transform)
    {
        CampaignController.Instance.HeroFollowerTransforms.Add(transform);
    }

    #endregion

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
        IsPlayingSingularEvent = true;
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
        IsPlayingSingularEvent = false;
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
        IsPlayingSingularEvent = true;
        _dummyParticleSystem.Play();
        _dummyAnimator.Play("Destroy");
        SoundManager.Instance.PlayOneShot(_dummyDestroyedSFX, 1.2f);
        EventManager.CompleteEvent(EventType.DEFEATED_DUMMY);
        StartCoroutine(StopEventWhenAnimationIsFinished(_dummyAnimator, 1));
    }

    #endregion

    #region Open First Gate Event

    public void QueueOpenFirstGate()
    {
        QueuedEvents.Enqueue(() =>
        {
            EventManager.CompleteEvent(EventType.FOREST_GATE_001);
            _firstGateObject.sprite = _openGate;
        });
    }

    #endregion

}
