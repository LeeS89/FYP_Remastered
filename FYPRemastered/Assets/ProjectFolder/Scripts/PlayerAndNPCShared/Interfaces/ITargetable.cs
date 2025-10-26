using UnityEngine;

public interface ITargetable
{
    Vector3 GetTargetablePosition();

    Collider GetTargetableCollider();

    bool IsMoving { get; }
}
