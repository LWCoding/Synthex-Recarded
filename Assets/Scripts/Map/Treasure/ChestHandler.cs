using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChestHandler : MonoBehaviour, IPointerClickHandler
{

    [Header("Object Assignments")]
    public GameObject flashObject;
    public Vector3 flashObjectFinalSize;
    private Animator _chestAnimator;
    private bool _isOpened;
    [Header("Audio Assignments")]
    public AudioClip boxDropSFX;
    public AudioClip boxOpeningSFX;
    public AudioClip boxShakingSFX;

    private void Awake()
    {
        _isOpened = false;
        _chestAnimator = GetComponent<Animator>();
    }

    public void OnPointerClick(PointerEventData ped)
    {
        if (_isOpened) { return; }
        _isOpened = true;
        StartCoroutine(BoxOpenCoroutine());
    }

    public void ShowChest()
    {
        StartCoroutine(ShowChestCoroutine());
    }

    private IEnumerator ShowChestCoroutine()
    {
        _chestAnimator.Play("BoxShow");
        yield return new WaitUntil(() =>
        {
            return !IsAnimatorPlaying();
        });
        _chestAnimator.Play("BoxLocked");
    }

    // Plays the sound effect when the box drops.
    // This should be called from an animation event.
    public void PlayBoxDropSFX()
    {
        SoundManager.Instance.PlayOneShot(boxDropSFX);
    }

    // Plays the sound effect when the box opens.
    // This should be called from an animation event.
    public void PlayBoxOpeningSFX()
    {
        SoundManager.Instance.PlayOneShot(boxOpeningSFX);
    }

    // Plays the sound effect when the box opens.
    // This should be called from an animation event.
    public void PlayBoxShakingSFX()
    {
        SoundManager.Instance.PlayOneShot(boxShakingSFX, 1.8f);
    }

    private IEnumerator BoxOpenCoroutine()
    {
        // Start the box shaking animation.
        _chestAnimator.Play("BoxShake");
        yield return new WaitForEndOfFrame();
        // Wait until the box is finished animating the shake sequence.
        yield return new WaitUntil(() =>
        {
            return !IsAnimatorPlaying();
        });
        // Make a circle grow from the center of the screen to flash the player.
        StartCoroutine(FlashScreenCoroutine(0.2f));
        // Start the box opening animation.
        _chestAnimator.Play("BoxOpen");
        yield return new WaitForEndOfFrame();
        // Wait until the box is finished animating the open sequence.
        yield return new WaitUntil(() =>
        {
            return !IsAnimatorPlaying();
        });
        // While the flash screen is overlayed, show the relic the player unlocked.
        GameObject relicObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.RELIC);
        Relic randomRelic = GameController.GetRandomUnownedRelic(new List<Relic>());
        relicObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
        relicObject.transform.position = transform.position + new Vector3(0, 40, 0);
        relicObject.GetComponent<RelicHandler>().Initialize(randomRelic, true);
        relicObject.GetComponent<RelicHandler>().EnableTreasureFunctionality();
        // Make the screen flash slowly go away again, revealing the object.
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DimFlashScreenCoroutine(1.5f));
    }

    private IEnumerator FlashScreenCoroutine(float timeInSeconds)
    {
        int frames = 0;
        int maxFrames = (int)(60 * timeInSeconds);
        Transform flashObjectTransform = flashObject.GetComponent<Transform>();
        Image flashObjectImage = flashObject.GetComponent<Image>();
        Color initialColor = new Color(1, 1, 1, 0);
        Color targetColor = new Color(1, 1, 1, 1);
        flashObjectTransform.gameObject.SetActive(true);
        while (frames < maxFrames)
        {
            frames++;
            flashObjectTransform.transform.localScale = Vector3.Lerp(Vector3.zero, flashObjectFinalSize, (float)frames / maxFrames);
            flashObjectImage.color = Color.Lerp(initialColor, targetColor, (float)frames / maxFrames);
            yield return null;
        }
    }

    private IEnumerator DimFlashScreenCoroutine(float timeInSeconds)
    {
        int frames = 0;
        int maxFrames = (int)(60 * timeInSeconds);
        Image flashObjectImage = flashObject.GetComponent<Image>();
        Color initialColor = new Color(1, 1, 1, 1);
        Color targetColor = new Color(1, 1, 1, 0);
        while (frames < maxFrames)
        {
            frames++;
            flashObjectImage.color = Color.Lerp(initialColor, targetColor, (float)frames / maxFrames);
            yield return null;
        }
        flashObjectImage.gameObject.SetActive(false);
    }

    private bool IsAnimatorPlaying()
    {
        return _chestAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

}
