using Unity.VisualScripting;
using UnityEngine;

public abstract class EMouseBaseState
{
    public abstract void EnterState(EMouseStateManager eMouse);

    public abstract void UpdateState(EMouseStateManager eMouse);
}
