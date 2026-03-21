using System;
using UnityEngine;

public interface IFsmStateEvents : IInstanceIdentifiable
{
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
    void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete);
}

public interface IInstanceIdentifiable
{
    int EntityId { get; }
}
