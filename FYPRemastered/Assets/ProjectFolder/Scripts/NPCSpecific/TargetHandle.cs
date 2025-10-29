using System.Collections;
using UnityEngine;

public class TargetHandle
{
    private ITargetable _primaryTarget;
    private ITargetable _secondaryTarget;

    private ITargetable _followTarget;
    public ITargetable _attackTarget;


    private BlockData _wayPointSet;

    // Plan for Destination providers
    //













    public Transform FollowTarget { get; private set; } = null;
    public Vector3 LastKnownTargetPos { get; private set; }

    public Collider GetAttackTarget(AttackTarget target)
    {
        if (target == AttackTarget.Primary) return _primaryTarget?.GetTargetableCollider();
        else return _secondaryTarget?.GetTargetableCollider();
    }
    public Vector3? GetFollowTarget(MovementIntent intent)
    {
        Vector3? target;
        switch (intent)
        {
            case MovementIntent.FollowPrimary:
                target = _primaryTarget?.GetTargetablePosition();
                break;
            case MovementIntent.FollowSecondary:
                target = _secondaryTarget?.GetTargetablePosition();
                break;
            default:
                target = null;
                break;
        }
        return target;
    }


    public void GetPrimaryTarget()
    {
        if(!GameManager.Instance.TryGetPlayer(out _primaryTarget))
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
