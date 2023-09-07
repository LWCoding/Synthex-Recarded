using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneInteractable : MonoBehaviour, IInteractable
{

    [Header("Object Assignments")]
    public GameObject popupObject;
    [Header("Area Properties")]
    public string sceneNameWhenInteracted;

    public void Awake() {
        OnLocationExit();
    }

    public void OnInteract() {
        TransitionManager.Instance.HideScreen(sceneNameWhenInteracted, 1.25f);
    }

    public void OnLocationEnter() {
        popupObject.SetActive(true);
        StartCoroutine(CheckForInteractCoroutine());
    }

    public void OnLocationExit() {
        popupObject.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator CheckForInteractCoroutine() {
        while (true) {
            if (Input.GetKeyDown(KeyCode.E)) {
                OnInteract();
            }
            yield return null;
        }
    }

}
