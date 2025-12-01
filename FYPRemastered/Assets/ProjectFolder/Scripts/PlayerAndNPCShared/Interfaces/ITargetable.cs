using UnityEngine;

public interface ITargetable
{
    Vector3 Forward { get; }

    Vector3 Position();

    Quaternion Rotation();

    Transform Transform { get; }

    Collider TargetableCollider { get; }

    bool IsStationary { get; }

    LayerMask LayerMask { get; }
}
