using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{

    public static BackgroundController Instance { get; private set; }
    [Header("Object Assignments")]
    public GameObject forestBGObject;
    public GameObject warzoneBGObject;
    public GameObject cloudPrefab;
    public Transform cloudParentTransform;
    public List<Sprite> cloudSprites = new List<Sprite>();

    private void Awake()
    {
        Instance = GetComponent<BackgroundController>();
    }

    // Initializes any type of background sprites.
    public void InitializeBG()
    {
        // Set all battle backgrounds to invisible.
        GameObject[] battleBgs = GameObject.FindGameObjectsWithTag("BattleBG");
        foreach (GameObject obj in battleBgs)
        {
            obj.SetActive(false);
        }
        // Turn the correct background visible based on map scene.
        switch (GameController.GetMapObject().currScene)
        {
            case MapScene.FOREST:
                forestBGObject.SetActive(true);
                StartCoroutine(CloudMoveCoroutine(-9));
                StartCoroutine(CloudMoveCoroutine(-4));
                StartCoroutine(CloudMoveCoroutine(0));
                StartCoroutine(CloudMoveCoroutine(4));
                StartCoroutine(CloudMoveCoroutine(9));
                break;
            case MapScene.AERICHO:
                warzoneBGObject.SetActive(true);
                break;
        }
    }

    private IEnumerator CloudMoveCoroutine(float startingX)
    {
        GameObject cloud = Instantiate(cloudPrefab);
        cloud.transform.SetParent(cloudParentTransform);
        cloud.transform.position = new Vector3(startingX, Random.Range(1.7f, 3.8f), 0);
        SpriteRenderer cloudRenderer = cloud.GetComponent<SpriteRenderer>();
        cloudRenderer.GetComponent<SpriteRenderer>().sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];
        while (true)
        {
            while (cloud.transform.position.x < 12)
            {
                cloud.transform.position += new Vector3(0.01f, 0, 0);
                yield return new WaitForSeconds(0.03f);
            }
            cloudRenderer.GetComponent<SpriteRenderer>().sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];
            cloud.transform.position = new Vector3(-12, Random.Range(1.7f, 3.8f), 0);
        }
    }

}
