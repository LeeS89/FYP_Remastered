using System;
using UnityEngine;

public interface IFsmStateEvents : ITargetContext, IInstanceIdentifiable
{
    void Test();

    bool TryGetTargetPosition(out Vector3? targetPos);
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
    void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete);
}

public interface IContext { }

public interface ITargetContext : IContext
{

}

public interface IInstanceIdentifiable
{

    int EntityId { get; }
}


public interface ITargetProvider
{
    bool TryGetTargetPosition(out Vector3? position);
}