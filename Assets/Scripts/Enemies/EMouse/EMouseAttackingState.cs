using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EMouseAttackingState : EMouseBaseState
{
    public override void EnterState(EMouseStateManager eMouse)
    {
        Debug.Log("Start Attack");
    }

    public override void UpdateState(EMouseStateManager eMouse)
    {
        // Attacking logic - move towards player
        GameObject em = eMouse.GetEMouseGO();
        Vector3 dir = new Vector3(eMouse.GetPlayerDir().x, 0.0f, eMouse.GetPlayerDir().z);
        float speed = 3.0f;
        em.transform.localPosition += dir.normalized * speed * Time.deltaTime;

        // Transition logic
        if (eMouse.GetPlayerDist() > 10.0f)
        {
            eMouse.ChangeState(eMouse.idleState);
        }
    }
}
