using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyIntentHandler : MonoBehaviour
{

    [Header("Object Assignments")]
    public Transform intentParentTransform;
    public SpriteRenderer intentIconRenderer;
    public TextMeshPro intentValueText;
    public EnemyIntentIconHandler enemyIntentIconHandler;
    [Header("Sprite Assignments")]
    public Sprite attackerImage;
    public Sprite blockerImage;
    public Sprite specialAttackerImage;
    public Sprite specialBlockerImage;
    public Sprite specialMiscImage;
    private CardType _futureBehaviorType;
    private CardStats _futureBehavior;
    private int _strengthBuff = 0;

    public void HideIntentIcon()
    {
        intentParentTransform.gameObject.SetActive(false);
    }

    public void ShowIntentIcon()
    {
        intentParentTransform.gameObject.SetActive(true);
    }

    // Sets the intent icon and text based on what card is going to
    // be played in the future. Specifically for enemy.
    public void SetIntentType(Card futureBehavior, int strengthBuff = 0)
    {
        _futureBehavior = futureBehavior.GetCardStats();
        _futureBehaviorType = futureBehavior.cardData.cardType;
        _strengthBuff = strengthBuff;
        UpdateIntentType();
    }

    public void UpdateStrengthValue(int strengthBuff)
    {
        _strengthBuff = strengthBuff;
        UpdateIntentType();
    }

    private void UpdateIntentType()
    {
        int repeatTimes = _futureBehavior.attackRepeatCount;
        // Set the correct information based on the card's type.
        switch (_futureBehaviorType)
        {
            case CardType.ATTACKER:
                intentIconRenderer.sprite = attackerImage;
                SetIntentTextValue(_futureBehavior.damageValue, repeatTimes, true);
                enemyIntentIconHandler.SetText("This enemy intends to attack for <b>" + (_futureBehavior.damageValue + _strengthBuff) + "</b> damage" + ((repeatTimes != 1) ? " " + repeatTimes + " times " : "") + ".");
                break;
            case CardType.BLOCKER:
                intentIconRenderer.sprite = blockerImage;
                SetIntentTextValue(0);
                enemyIntentIconHandler.SetText("This enemy intends to protect itself for the next turn.");
                break;
            case CardType.SPECIAL_ATTACKER:
                intentIconRenderer.sprite = specialAttackerImage;
                SetIntentTextValue(_futureBehavior.damageValue, repeatTimes, true);
                enemyIntentIconHandler.SetText("This enemy intends to perform a special attack for <b>" + (_futureBehavior.damageValue + _strengthBuff) + "</b> damage.");
                break;
            case CardType.SPECIAL_BLOCKER:
                intentIconRenderer.sprite = specialBlockerImage;
                enemyIntentIconHandler.SetText("This enemy intends to protect itself for the next turn with special effects.");
                SetIntentTextValue(0);
                break;
            case CardType.SPECIAL_MISC:
                intentIconRenderer.sprite = specialMiscImage;
                enemyIntentIconHandler.SetText("This enemy's next move is unknown, but non-damaging.");
                SetIntentTextValue(0);
                break;
        }
    }

    // If trying to set `value` to 0, hides the value.
    // Or else, sets the text to show the value.
    private void SetIntentTextValue(int value, int repeatTimes = 1, bool increaseByStrength = false)
    {
        if (value == 0)
        {
            intentValueText.enabled = false;
            return;
        }
        if (increaseByStrength) { value += _strengthBuff; }
        intentValueText.enabled = true;
        intentValueText.text = value.ToString() + ((repeatTimes == 1) ? "" : "x" + repeatTimes);
    }

}
