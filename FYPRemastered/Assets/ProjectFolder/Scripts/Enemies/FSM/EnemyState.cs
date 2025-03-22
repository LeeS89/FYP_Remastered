using Unity.VisualScripting;
using UnityEngine;

public abstract class EnemyState
{
    protected EnemyFSMController _fsm;

    public EnemyState(EnemyFSMController fsm)
    {
        _fsm = fsm;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void OnStateDestroyed();
}
