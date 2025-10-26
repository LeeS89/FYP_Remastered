using System.Collections;
using UnityEngine;

public class TargetHandle
{
    private ITargetable _followTarget;
    public ITargetable _attackTarget;

   

    public Transform FollowTarget { get; private set; } = null;
    public Vector3 LastKnownTargetPos { get; private set; }

    public Collider GetAttackTarget() => _attackTarget?.GetTargetableCollider();
    public Vector3? GetFollowTarget() => _followTarget?.GetTargetablePosition();


    public void SetAttackTarget()
    {
        if(!GameManager.Instance.TryGetPlayer(out _attackTarget))
        {
#if UNITY_EDITOR
            Debug.LogError("Player ITargetable not found");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Player ITargetable found");
#endif
        }
    }
}
