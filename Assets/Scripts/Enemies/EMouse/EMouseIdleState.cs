using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMouseIdleState : EMouseBaseState
{
    Vector3 playerLoc;
    public override void EnterState(EMouseStateManager eMouse)
    {
        Debug.Log("Start Idle");
    }

    public override void UpdateState(EMouseStateManager eMouse)
    {
        // Transition logic
        if (eMouse.GetPlayerDist() < 10.0f)
        {
            eMouse.ChangeState(eMouse.attackingState);
        }
    }
}
