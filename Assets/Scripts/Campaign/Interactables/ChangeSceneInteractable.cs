using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneInteractable : MonoBehaviour, IInteractable
{

    [Header("Object Assignments")]
    public MouseHoverScaler spriteObjectScaler;
    public GameObject popupObject;
    [Header("Area Properties")]
    public string sceneNameWhenInteracted;

    private bool _isInteractable = false;

    public void Awake() {
        spriteObjectScaler.Initialize(spriteObjectScaler.transform);
        spriteObjectScaler.SetIsInteractable(true);
        OnLocationExit();
    }

    public void OnInteract() {
        if (!_isInteractable) { return; }
        _isInteractable = false;
        TransitionManager.Instance.HideScreen(sceneNameWhenInteracted, 1.25f);
    }

    public void OnLocationEnter() {
        popupObject.SetActive(true);
        _isInteractable = true;
        StartCoroutine(CheckForInteractCoroutine());
    }

    public void OnLocationExit() {
        popupObject.SetActive(false);
        _isInteractable = false;
        spriteObjectScaler.ResetScale();
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

    public void OnMouseDown() {
        if (_isInteractable) {
            OnInteract();
        }
    }

    public void OnMouseOver() {
        if (_isInteractable) {
            spriteObjectScaler.OnMouseEnter();
        }
    }

    public void OnMouseExit() {
        if (_isInteractable) {
            spriteObjectScaler.OnMouseExit();
        }
    }

}
