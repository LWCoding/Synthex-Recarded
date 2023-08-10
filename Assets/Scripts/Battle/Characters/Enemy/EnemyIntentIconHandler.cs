using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyIntentIconHandler : MonoBehaviour
{

    [Header("Object Assignments")]
    public Transform intentTooltipParentTransform;
    public TextMeshPro intentTooltipText;
    private BoxCollider2D _boxCollider2D;

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        intentTooltipParentTransform.gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
        intentTooltipText.text = text;
    }

    public void OnMouseEnter()
    {
        intentTooltipParentTransform.gameObject.SetActive(true);
    }

    public void OnMouseExit()
    {
        intentTooltipParentTransform.gameObject.SetActive(false);
    }

}
