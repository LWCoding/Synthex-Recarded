using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{

    public UnityEvent<GameObject> OnCardClick;

    private void OnEnable()
    {
        OnCardClick = new UnityEvent<GameObject>(); // Make card initially do nothing on click when enabled.
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClick.Invoke(gameObject);
    }

}
