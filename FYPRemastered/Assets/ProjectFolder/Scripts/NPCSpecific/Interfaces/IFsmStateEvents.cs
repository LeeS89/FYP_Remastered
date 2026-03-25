using System;
using UnityEngine;
using UnityEngine.AI;

[Obsolete]
public interface IFsmStateEvents : ITargetContext, IInstanceIdentifiable
{
    void Test();

    bool TryGetTargetPosition(out Vector3? targetPos);
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
    void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete);
}

public interface IFsmStateContext : IInstanceIdentifiable
{
    bool TryGetPath(out NavMeshPath path);
    bool TryGetCurrentPosition(out Vector3? currentPos);
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
    void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete);
}

public interface IContext { }

[Obsolete]
public interface ITargetContext : IContext
{

}

public interface IInstanceIdentifiable
{

    int EntityId { get; }
}


public interface ITargetProvider
{
    bool TryGetTarget(out ITargetable target);
    bool TryGetTargetPosition(out Vector3? position);
}