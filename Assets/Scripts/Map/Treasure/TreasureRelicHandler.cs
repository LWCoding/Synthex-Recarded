using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TreasureRelicHandler : MonoBehaviour, IPointerClickHandler
{

    [Header("Object Assignments")]
    private RelicHandler _parentRelicHandler;

    private void Awake()
    {
        _parentRelicHandler = GetComponent<RelicHandler>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Play the relic chosen SFX.
        SoundManager.Instance.PlaySFX(SoundEffect.RELIC_OBTAIN);
        // Add the relic to the deck.
        GameController.AddRelicToInventory(_parentRelicHandler.relicInfo);
        TopBarController.Instance.RenderRelics();
        // Go back to the map screen.
        FadeTransitionController.Instance.HideScreen("Map", 1.2f);
        // Hide this relic object by returning it to the pool.
        _parentRelicHandler.StopSpinShinyObject();
        ObjectPooler.Instance.ReturnObjectToPool(PoolableType.RELIC, gameObject);
    }

}
