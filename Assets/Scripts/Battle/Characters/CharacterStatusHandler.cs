using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class CharacterStatusHandler : MonoBehaviour
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
        bool foundStatusEffect = false;
        // If we can find the status, increment it.
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].statusInfo.type == e.statusInfo.type)
            {
                foundStatusEffect = true;
                statusEffects[i].shouldActivate = true;
                statusEffects[i].ChangeCount(e.amplifier);
                break;
            }
        }
        // If the status wasn't found, add it.
        if (!foundStatusEffect)
        {
            e.shouldActivate = true;
            statusEffects.Add(e);
            InitializeNewStatusObject(e);
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
        // If we can find the status, return its corresponding object to the pool.
        for (int i = 0; i < statusIconObjects.Count; i++)
        {
            if (statusIconObjects[i].GetComponent<StatusController>().effectType == e)
            {
                BattlePooler.Instance.ReturnStatusObjectToPool(statusIconObjects[i]);
                statusIconObjects.RemoveAt(i);
                return;
            }
        }
        UpdateStatusIcons();
    }

    // Removes all status effects that have reached a count of 0.
    public void RemoveEmptyStatusEffects()
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

    // Updates all status effect icons shown under health bar.
    public void UpdateStatusIcons()
    {
        RemoveEmptyStatusEffects();
        for (int i = 0; i < statusIconObjects.Count; i++)
        {
            GameObject iconObject = statusIconObjects[i];
            iconObject.transform.SetParent(statusParentTransform);
            iconObject.transform.position = statusParentTransform.position + new Vector3(0.6f * i, 0);
            iconObject.GetComponent<StatusController>().UpdateStatus(statusEffects[i]);
        }
    }

    private void InitializeNewStatusObject(StatusEffect e)
    {
        GameObject statusObject = BattlePooler.Instance.GetStatusObjectFromPool();
        statusObject.GetComponent<StatusController>().UpdateStatus(e);
        statusIconObjects.Add(statusObject);
    }

}
