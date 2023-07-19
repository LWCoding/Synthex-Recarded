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
    public string locationFound;
    [Header("AI")]
    public EnemyAI enemyAI;

}
