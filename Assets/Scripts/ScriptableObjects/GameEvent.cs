using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    VISITED_SHOP_BEFORE = 0, VISITED_UPGRADES_BEFORE = 1, DEFEATED_DUMMY = 2
}

[CreateAssetMenu(fileName = "GameEvent", menuName = "ScriptableObjects/GameEvent")]
public class GameEvent : ScriptableObject
{

    [Header("Base Information")]
    public EventType EventType;
    public int RequiredAmountToComplete = 1;

    private int _currCompletionAmount = 0;
    public bool IsCompleted() => _currCompletionAmount == RequiredAmountToComplete;
    public void Increment() => _currCompletionAmount = Mathf.Min(_currCompletionAmount + 1, RequiredAmountToComplete);
    public void SetCompleted() => RequiredAmountToComplete = _currCompletionAmount;

}