using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHeroController : BattleCharacterController
{

    public void Start()
    {
        // When the hero dies, set the state to be lost.
        OnDeath.AddListener(() =>
        {
            BattleController.Instance.SetState(new Lost(BattleController.Instance));
        });
        // Update the actual stored health (affects UI) after the health of the hero updates.
        OnUpdateHealthText.AddListener(() => GameManager.SetHeroHealth(GetHealth()));
    }

}
