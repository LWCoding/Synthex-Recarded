using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TutorialDialogue
{
    public string animationName = "";
    public string dialogueText = "";
    public Sprite dialogueSprite;
}

public class TutorialController : MonoBehaviour
{

    public static TutorialController Instance;
    [Header("Object Assignments")]
    public GameObject tutorialCanvasObject;
    public Image tutorialCharacterSprite;
    public TextMeshProUGUI dialogueText;
    public List<TutorialDialogue> animationsToPlayInOrder;

    private Animator _tutorialCanvasAnimator;
    private int _currAnimationStep; // Current place in the animation list that we're at.

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        _tutorialCanvasAnimator = tutorialCanvasObject.GetComponent<Animator>();
    }

    public void StartTutorial()
    {
        _currAnimationStep = 0;
        tutorialCanvasObject.SetActive(true);
        _tutorialCanvasAnimator.Play("TutorialCanvasShow", -1);
        StartCoroutine(RenderNextAnimationCoroutine());
    }

    private IEnumerator RenderNextAnimationCoroutine()
    {
        // If we're playing the show animation, wait until that's done.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() =>
        {
            return !IsPlaying("TutorialCanvasShow", 1);
        });
        // Find the animation by name. If not found, error!
        TutorialDialogue dialogue = animationsToPlayInOrder[_currAnimationStep];
        _tutorialCanvasAnimator.Play(dialogue.animationName);
        dialogueText.text = dialogue.dialogueText;
        tutorialCharacterSprite.sprite = dialogue.dialogueSprite;
        // Make the speaker animate upwards IF it's not the same animation.
        if (_currAnimationStep == 0 || dialogue.animationName != animationsToPlayInOrder[_currAnimationStep - 1].animationName)
        {
            _tutorialCanvasAnimator.Play("SpeakerShow");
        }
        // Wait until the animation is finished playing.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() =>
        {
            return !IsPlaying(dialogue.animationName);
        });
        // Then, wait for the player to click.
        StartCoroutine(WaitForClickCoroutine());
    }

    private IEnumerator WaitForClickCoroutine()
    {
        // Wait until the left click button isn't being held down.
        yield return new WaitUntil(() => { return !Input.GetMouseButton(0); });
        // Wait until the left click button is triggered.
        yield return new WaitUntil(() => { return Input.GetMouseButton(0); });
        // Make the speaker hide to transition to the next dialogue if the
        // next animation is not the same.
        if (_currAnimationStep == animationsToPlayInOrder.Count - 1 || animationsToPlayInOrder[_currAnimationStep].animationName != animationsToPlayInOrder[_currAnimationStep + 1].animationName)
        {
            _tutorialCanvasAnimator.Play("SpeakerHide");
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() =>
        {
            return !IsPlaying("SpeakerHide", 2);
        });
        // If there are still animations we haven't played in the list, play them.
        // Or else, hide the box. We're done!
        if (_currAnimationStep < animationsToPlayInOrder.Count - 1)
        {
            _currAnimationStep++;
            StartCoroutine(RenderNextAnimationCoroutine());
        }
        else
        {
            _tutorialCanvasAnimator.Play("TutorialCanvasHide");
        }
    }

    public bool IsPlaying(string stateName, int layerNumber = 0)
    {
        return _tutorialCanvasAnimator.GetCurrentAnimatorStateInfo(layerNumber).IsName(stateName) && _tutorialCanvasAnimator.GetCurrentAnimatorStateInfo(layerNumber).normalizedTime < 1.0f;
    }

}
