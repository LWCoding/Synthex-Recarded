using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{

    public State State { get; private set; }

    ///<summary>
    /// Set the current state in the state machine.
    /// Will run any logic that should run when the state is switched.
    ///</summary>
    public void SetState(State state)
    {
        State = state;
        StartCoroutine(state.Start());
    }

}
