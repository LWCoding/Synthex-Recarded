using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterStatusHandler : MonoBehaviour
{

    [Header("Object Assignments")]
    public Transform statusParentTransform;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    [HideInInspector] public List<GameObject> statusIconObjects = new List<GameObject>();
    [HideInInspector] public UnityEvent<StatusEffect> OnGetStatusEffect = new UnityEvent<StatusEffect>();

    public StatusEffect GetStatusEffect(Effect e)
    {
        // If we can find the status, return true.
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].statusInfo.type == e)
            {
                return statusEffects[i];
            }
        }
        // Or else, return null.
        return null;
    }

    // Adds a status effect to the current character. If they
    // already have the effect, increments it instead.
    public void AddStatusEffect(StatusEffect e)
    {
        // Try to find the status effect.
        StatusEffect currEffect = GetStatusEffect(e.statusInfo.type);
        // If the status wasn't found, add it.
        // Or else, increment it.
        if (currEffect == null)
        {
            e.shouldActivate = true;
            statusEffects.Add(e);
            InitializeNewStatusObject(e);
        }
        else
        {
            currEffect.shouldActivate = true;
            currEffect.ChangeCount(e.amplifier);
        }
        // Play the corresponding sound if this is a buff or debuff.
        switch (e.statusInfo.effectFaction)
        {
            case EffectFaction.BUFF:
                SoundManager.Instance.PlaySFX(SoundEffect.GAIN_BUFF);
                break;
            case EffectFaction.DEBUFF:
                SoundManager.Instance.PlaySFX(SoundEffect.GAIN_DEBUFF);
                break;
            case EffectFaction.CHARGE:
                SoundManager.Instance.PlaySFX(SoundEffect.GAIN_CHARGE);
                break;
        }
        UpdateStatusIcons();
        // Run the OnGetStatusEffect action. This updates enemy intents if necessary.
        OnGetStatusEffect.Invoke(e);
    }

    // Decrements a status effect from the current character.
    // A positive amp will reduce the status effect's count by that much.
    // If they do not have it, does nothing.
    public void DecrementStatusEffect(Effect e, int amp)
    {
        // Try to find the status effect.
        StatusEffect currEffect = GetStatusEffect(e);
        // If the status wasn't found, stop here.
        if (currEffect == null) { return; }
        // Or else, decrement it.
        currEffect.shouldActivate = true;
        currEffect.ChangeCount(-amp);
        // Update all status effect icons afterwards.
        UpdateStatusIcons();
    }

    // Removes a status effect from the current character.
    public void RemoveStatusEffect(Effect e)
    {
        // If we can find the status, remove it from the statusEffects list.
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].statusInfo.type == e)
            {
                statusEffects.RemoveAt(i);
                break;
            }
        }
        // If we can find the status icon, return its corresponding object to the pool.
        for (int i = 0; i < statusIconObjects.Count; i++)
        {
            if (statusIconObjects[i].GetComponent<StatusEffectHandler>().effectType == e)
            {
                BattlePooler.Instance.ReturnStatusObjectToPool(statusIconObjects[i]);
                statusIconObjects.RemoveAt(i);
                break;
            }
        }
        UpdateStatusIcons();
    }

    // Updates all status effect icons shown under health bar.
    private void UpdateStatusIcons()
    {
        RemoveEmptyStatusEffects();
        for (int i = 0; i < statusIconObjects.Count; i++)
        {
            GameObject iconObject = statusIconObjects[i];
            iconObject.transform.SetParent(statusParentTransform);
            iconObject.transform.position = statusParentTransform.position + new Vector3(0.6f * i, 0);
            iconObject.GetComponent<StatusEffectHandler>().UpdateStatus(statusEffects[i]);
        }
    }

    // Removes all status effects that have reached a count of 0.
    private void RemoveEmptyStatusEffects()
    {
        // Remove all status effects that have worn off.
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (!statusEffects[i].IsActive())
            {
                RemoveStatusEffect(statusEffects[i].statusInfo.type);
            }
        }
    }

    private void InitializeNewStatusObject(StatusEffect e)
    {
        GameObject statusObject = BattlePooler.Instance.GetStatusObjectFromPool();
        statusObject.GetComponent<StatusEffectHandler>().UpdateStatus(e);
        statusIconObjects.Add(statusObject);
    }

}
