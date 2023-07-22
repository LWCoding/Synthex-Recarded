using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy")]
public class Enemy : Character
{

    [Header("Battle Information")]
    public int enemyHealthMin;
    public int enemyHealthMax;
    public int enemyRewardMin;
    public int enemyRewardMax;
    public int enemyXPReward;
    [Header("Journal Information")]
    public Sprite enemyIcon;
    public string locationFound;
    [Header("AI")]
    public EnemyAI enemyAI;
    [SerializeField] private List<string> _possibleEncounterDialogues = new List<string>();
    public string GetRandomEncounterDialogue() => _possibleEncounterDialogues[Random.Range(0, _possibleEncounterDialogues.Count)];
    public bool HasEncounterDialogues() => _possibleEncounterDialogues.Count > 0;

}
