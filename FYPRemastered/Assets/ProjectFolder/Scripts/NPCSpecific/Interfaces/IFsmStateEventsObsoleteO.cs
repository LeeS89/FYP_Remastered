using System;
using UnityEngine;
using UnityEngine.AI;

[Obsolete("", true)]
public interface IFsmStateEventsObsoleteO : ITargetContext, IInstanceIdentifiable
{
    void Test();

    bool TryGetTargetPosition(out Vector3? targetPos);
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
    void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete);
}


[Obsolete("", true)]
public interface IContext { }

[Obsolete("", true)]
public interface ITargetContext : IContext
{

}

