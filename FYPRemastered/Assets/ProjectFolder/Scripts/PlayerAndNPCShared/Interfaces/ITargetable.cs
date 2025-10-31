using UnityEngine;

public interface ITargetable
{
    (Vector3, Vector3?) GetTargetablePositionAndForward();

    Vector3 GetPosition();

    Collider GetTargetableCollider();

    bool IsMoving { get; }
}
