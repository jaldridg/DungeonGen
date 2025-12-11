using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMouseStateManager : MonoBehaviour
{
    EMouseBaseState currentState;
    public EMouseIdleState idleState = new EMouseIdleState();
    public EMouseAttackingState attackingState = new EMouseAttackingState();

    GameObject eMouse;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").transform.GetChild(0).gameObject;
        eMouse = gameObject;
        currentState = idleState;
        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);
    }

    public void ChangeState(EMouseBaseState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }

    public float GetPlayerDist()
    {
        return GetPlayerDir().magnitude;
    }

    public Vector3 GetPlayerDir()
    {
        return player.transform.position - GetEMouseGO().transform.position;
    }

    public GameObject GetEMouseGO()
    {
        return eMouse;
    }
}
