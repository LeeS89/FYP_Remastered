using UnityEngine;

public interface ITargetable
{
    (Vector3, Vector3?) GetTargetablePositionAndForward();

    Vector3 GetPosition();

    Quaternion GetRotation();

    Transform Transform { get; }

    Collider TargetableCollider { get; }

    bool IsMoving { get; }

    LayerMask LayerMask { get; }
}
