using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureController : MonoBehaviour
{

    public static TreasureController Instance;
    [Header("Object Assignments")]
    public GameObject chestObject;
    public Image chestOverlayImage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        chestObject.SetActive(false);
    }

    private void Start()
    {
        chestObject.SetActive(false);
    }

    public void ShowChest()
    {
        StartCoroutine(ShowChestCoroutine());
    }

    private IEnumerator ShowChestCoroutine()
    {
        yield return ShowBGOverlayCoroutine();
        yield return new WaitForSeconds(0.3f);
        chestObject.SetActive(true);
        chestObject.GetComponent<ChestHandler>().ShowChest();
    }

    private IEnumerator ShowBGOverlayCoroutine()
    {
        float currTime = 0;
        float timeToWait = 0.4f;
        Color initialColor = new Color(0, 0, 0, 0);
        Color targetColor = new Color(0, 0, 0, 0.8f);
        chestOverlayImage.gameObject.SetActive(true);
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            chestOverlayImage.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
    }

}
