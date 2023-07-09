using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PoolableType
{
    CARD, RELIC, CARD_EFFECT, TEXT_POPUP, UI_TEXT_POPUP
}

public class ObjectPooler : MonoBehaviour
{

    public static ObjectPooler Instance { get; private set; }
    [Header("Object Assignments")]
    public GameObject cardPrefab;
    public GameObject relicPrefab;
    public GameObject textPopupPrefab;
    public GameObject uiTextPopupPrefab;
    public Dictionary<PoolableType, Stack<GameObject>> inactiveObjects = new Dictionary<PoolableType, Stack<GameObject>>();

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
    }

    public void AddObjectToPool(PoolableType objectType)
    {
        GameObject obj = null;
        // Instantiate an object depending on the type of object we want created.
        switch (objectType)
        {
            case PoolableType.CARD:
                obj = Instantiate(cardPrefab);
                break;
            case PoolableType.RELIC:
                obj = Instantiate(relicPrefab);
                break;
            case PoolableType.CARD_EFFECT:
                obj = new GameObject("CardEffect");
                obj.AddComponent<Image>();
                obj.GetComponent<RectTransform>().sizeDelta = new Vector3(403, 529);
                break;
            case PoolableType.TEXT_POPUP:
                obj = Instantiate(textPopupPrefab);
                break;
            case PoolableType.UI_TEXT_POPUP:
                obj = Instantiate(uiTextPopupPrefab);
                break;
        }
        // Set default properties for the GameObject.
        obj.tag = "Pooled";
        obj.transform.SetParent(gameObject.transform);
        obj.SetActive(false);
        // If the dictionary doesn't conatin the object, create an empty list.
        if (!inactiveObjects.ContainsKey(objectType))
        {
            inactiveObjects.Add(objectType, new Stack<GameObject>());
        }
        inactiveObjects[objectType].Push(obj);
    }

    public GameObject GetObjectFromPool(PoolableType objectType)
    {
        // If the object doesn't exist, create it.
        if (!inactiveObjects.ContainsKey(objectType) || inactiveObjects[objectType].Count == 0)
        {
            AddObjectToPool(objectType);
        }
        // Get the already created object.
        GameObject obj = inactiveObjects[objectType].Pop();
        obj.SetActive(true);
        // Return the created object.
        return obj;
    }

    // Spawns popup objects that go away after a duration.
    public void SpawnPopup(string text, float fontSize, Vector3 position, Color fontColor, float speed = 1, float maxTime = 1.4f, bool shouldXMove = true)
    {
        // Return an already created popup object.
        GameObject popupObject = GetObjectFromPool(PoolableType.TEXT_POPUP);
        TextMeshPro popupText = popupObject.GetComponent<TextMeshPro>();
        popupObject.transform.position = position;
        popupText.text = text;
        popupText.fontSize = fontSize;
        popupText.color = fontColor;
        popupObject.GetComponent<PopupFade>().Initialize(speed, maxTime, shouldXMove);
        // Remove the popup text after a certain duration.
        StartCoroutine(ReturnPopupObjectToPool(popupObject, maxTime));
    }

    public IEnumerator ReturnPopupObjectToPool(GameObject objectToDeactivate, float maxTime)
    {
        yield return new WaitForSeconds(maxTime);
        objectToDeactivate.SetActive(false);
        ReturnObjectToPool(PoolableType.TEXT_POPUP, objectToDeactivate);
    }

    // Spawns UI popup objects that go away after a duration.
    public void SpawnUIPopup(string text, float fontSize, Vector3 position, Color fontColor, Transform canvasTransform, float speed = 1, float maxTime = 1.4f, bool shouldXMove = true)
    {
        // Return an already created popup object.
        GameObject popupObject = GetObjectFromPool(PoolableType.UI_TEXT_POPUP);
        TextMeshProUGUI popupText = popupObject.GetComponent<TextMeshProUGUI>();
        popupObject.transform.SetParent(canvasTransform);
        popupObject.transform.position = position;
        popupText.text = text;
        popupText.fontSize = fontSize;
        popupText.color = fontColor;
        popupObject.GetComponent<UIPopupFade>().Initialize(speed, maxTime, shouldXMove);
        // Remove the popup text after a certain duration.
        StartCoroutine(ReturnUIPopupObjectToPool(popupObject, maxTime));
    }

    public IEnumerator ReturnUIPopupObjectToPool(GameObject objectToDeactivate, float maxTime)
    {
        yield return new WaitForSeconds(maxTime);
        objectToDeactivate.SetActive(false);
        ReturnObjectToPool(PoolableType.UI_TEXT_POPUP, objectToDeactivate);
    }

    public void ReturnObjectToPool(PoolableType objectType, GameObject objectToDeactivate)
    {
        objectToDeactivate.transform.SetParent(gameObject.transform, false);
        objectToDeactivate.SetActive(false);
        inactiveObjects[objectType].Push(objectToDeactivate);
    }

}
