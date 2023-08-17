using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkipCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerClickHandler
{

    public float _initialScale;
    public float _desiredScale;

    private void Start()
    {
        _initialScale = transform.localScale.x;
        _desiredScale = _initialScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _desiredScale = _initialScale + 0.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _desiredScale = _initialScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CardChoiceController.Instance.HideUnselectedCards(false, () =>
        {
            TransitionManager.Instance.BackToMapOrCampaign(0.75f);
        });
    }

    public void FixedUpdate()
    {
        float difference = Mathf.Abs(transform.localScale.x - _desiredScale);
        if (difference > 0.011f)
        {
            if (transform.transform.localScale.x > _desiredScale)
            {
                if (difference < 0.04f)
                {
                    transform.transform.localScale -= new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    transform.transform.localScale -= new Vector3(0.03f, 0.03f, 0);
                }
            }
            else
            {
                if (difference < 0.04f)
                {
                    transform.transform.localScale += new Vector3(0.01f, 0.01f, 0);
                }
                else
                {
                    transform.transform.localScale += new Vector3(0.03f, 0.03f, 0);
                }
            }
        }
    }

}
