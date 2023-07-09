using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy")]
public class Enemy : Character
{

    [Header("Base Information")]
    public int enemyHealthMin;
    public int enemyHealthMax;
    public int enemyRewardMin;
    public int enemyRewardMax;
    public int enemyXPReward;
    [Header("AI")]
    public EnemyAI enemyAI;

}
