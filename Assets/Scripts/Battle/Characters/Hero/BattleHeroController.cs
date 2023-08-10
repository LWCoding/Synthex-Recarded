using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHeroController : BattleCharacterController
{

    public override void Awake()
    {
        base.Awake();
        _characterAlignment = Alignment.HERO;
    }

    public void Start()
    {
        // When the hero dies, set the state to be lost.
        OnDeath.AddListener(() =>
        {
            BattleController.Instance.SetState(new Lost());
        });
        // Update the actual stored health (affects UI) after the health of the hero updates.
        OnUpdateHealthText.AddListener(() => GameManager.SetHeroHealth(GetHealth()));
    }

}
