using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lost : State
{

    public Lost() : base() { }

    public override IEnumerator Start()
    {
        // Play Jack's death animation.
        BattleController.playerBCC.SetCharacterSprite(CharacterState.DEATH, true);
        // Play the death sound effect.
        SoundManager.Instance.PlaySFX(SoundEffect.GAME_OVER);
        // Transition back to the title.
        TransitionManager.Instance.HideScreen("Title", 2.5f);
        yield break;
    }

}
