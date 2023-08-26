using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{

    public static TransitionManager Instance;
    [Header("Object Assignments")]
    public Image overlayFadeImage;

    public bool IsScreenTransitioning = false;
    public bool IsScreenDarkened = true;

    private void Awake()
    {
        // Set this to the Instance if it is the first one.
        // Or else, destroy this.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        overlayFadeImage.gameObject.SetActive(false);
    }

    public void ShowScreen(float time)
    {
        StartCoroutine(ShowScreenCoroutine(time));
    }

    public void HideScreen(string desiredScene, float time)
    {
        StartCoroutine(HideScreenCoroutine(desiredScene, time));
    }

    public void BackToMapOrCampaign(float time)
    {
        if (GameManager.IsInCampaign)
        {
            StartCoroutine(HideScreenCoroutine("Campaign", time));
        }
        else
        {
            StartCoroutine(HideScreenCoroutine("Map", time));
        }
    }

    // This coroutine is called when the screen should fade
    // to black to clear.
    private IEnumerator ShowScreenCoroutine(float time)
    {
        IsScreenTransitioning = true;
        IsScreenDarkened = true;
        overlayFadeImage.gameObject.SetActive(true);
        overlayFadeImage.color = new Color(0, 0, 0, 1);
        Color initialColor = overlayFadeImage.color;
        Color targetColor = new Color(0, 0, 0, 0);
        float currTime = 0;
        float timeToWait = time;
        yield return new WaitForEndOfFrame(); // Wait to see if the volume is set for the AudioSource!
        float targetVolume = SoundManager.Instance.GetDesiredVolume();
        while (currTime < timeToWait)
        {
            if (!SettingsManager.Instance.IsGamePaused())
            {
                SoundManager.Instance.SetVolume(Mathf.Lerp(0, targetVolume, currTime / timeToWait));
            }
            currTime += Time.deltaTime;
            overlayFadeImage.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        // In case the desired volume was changed, let's set it again here.
        SoundManager.Instance.SetVolume(SoundManager.Instance.GetDesiredVolume());
        overlayFadeImage.gameObject.SetActive(false);
        IsScreenTransitioning = false;
        IsScreenDarkened = false;
    }

    // This coroutine is called when the screen should fade
    // to black and go to a certain screen. This should be
    // used when transitioning to a scene.
    private IEnumerator HideScreenCoroutine(string desiredSceneName, float time)
    {
        IsScreenTransitioning = true;
        IsScreenDarkened = false;
        overlayFadeImage.gameObject.SetActive(true);
        overlayFadeImage.color = new Color(0, 0, 0, 0);
        Color initialColor = overlayFadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1);
        float currTime = 0;
        float timeToWait = time;
        yield return new WaitForEndOfFrame(); // Wait to see if the volume is set for the AudioSource!
        float initialVolume = SoundManager.Instance.GetDesiredVolume();
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            if (!SettingsManager.Instance.IsGamePaused())
            {
                SoundManager.Instance.SetVolume(Mathf.Lerp(initialVolume, 0, currTime / timeToWait));
            }
            overlayFadeImage.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        IsScreenTransitioning = false;
        IsScreenDarkened = true;
        // If the deck is showing, hide it.
        if (TopBarController.Instance.IsCardPreviewShowing())
        {
            TopBarController.Instance.HideDeckOverlay();
        }
        // If the journal is showing, hide it.
        if (JournalManager.Instance.IsJournalShowing())
        {
            JournalManager.Instance.HidePopup();
        }
        // Load the next scene.
        SceneManager.LoadScene(desiredSceneName);
    }

}
