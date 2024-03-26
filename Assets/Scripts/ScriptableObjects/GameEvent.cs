using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EventType
{
    VISITED_SHOP_BEFORE = 0, VISITED_UPGRADES_BEFORE = 1, DEFEATED_DUMMY = 2, FOREST_CHEST_001 = 3,
    FOREST_GATE_001 = 4
}

[System.Serializable]
public class GameEvent
{

    [Header("Base Information")]
    public EventType EventType;
    public int RequiredAmountToComplete = 1;

    private int _currCompletionAmount = 0;
    public bool IsCompleted() => _currCompletionAmount == RequiredAmountToComplete;
    public void Increment() => _currCompletionAmount = Mathf.Min(_currCompletionAmount + 1, RequiredAmountToComplete);
    public void SetCompleted() => RequiredAmountToComplete = _currCompletionAmount;

}