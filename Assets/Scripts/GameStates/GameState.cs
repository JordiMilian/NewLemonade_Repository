using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public Canvas stateCanvas;
    public bool isGamePausedInState;

    public virtual void OnStateEnter()
    {
        if(stateCanvas != null )
        {
            stateCanvas.enabled = true;
        }
    }
    public virtual void OnStateStay()
    {

    }
    public virtual void OnStateExit()
    {
        if (stateCanvas != null)
        {
            stateCanvas.enabled = false;
        }
    }
}
