using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum DialogueDirection
{
    LEFT, RIGHT
}

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    public string speakerText;
    public string leftAnimationName;
    public string rightAnimationName;
    public CharacterBlipName characterBlipName;
    public DialogueDirection focusDirection;
}

[System.Serializable]
public enum DialogueAction
{
    NONE = 0, WON_GAME_SEND_TO_TITLE = 1, SECRET_WIN_SEND_TO_TITLE = 2, HEAL_TO_FULL_HP = 3
}

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue")]
public class Dialogue : ScriptableObject
{

    [Header("Base Information")]
    public string dialogueName;
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    public DialogueAction actionToPlayAfterDialogue;

}