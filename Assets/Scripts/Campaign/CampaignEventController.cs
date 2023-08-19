using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignEventController : MonoBehaviour
{

    public static CampaignEventController Instance;
    [Header("Dummy Object Assignments")]
    [SerializeField] private SpriteRenderer _dummySpriteRenderer;
    [SerializeField] private Animator _dummyAnimator;
    [SerializeField] private ParticleSystem _dummyParticleSystem;
    [SerializeField] private Sprite _intactDummy;
    [SerializeField] private Sprite _destroyedDummy;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    ///<summary>
    /// Initializes the states of all map objects from the save file.
    ///</summary>
    public void InitializeMapState(GameScene area)
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

    // Plays the animation of the dummy getting destroyed.
    public void DestroyDummy()
    {
        _dummyParticleSystem.Play();
        _dummyAnimator.Play("Destroy");
        GameManager.CompleteEvent(EventType.DEFEATED_DUMMY);
    }

}
