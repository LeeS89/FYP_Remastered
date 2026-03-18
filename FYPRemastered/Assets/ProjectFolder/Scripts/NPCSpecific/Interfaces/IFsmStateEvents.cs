using UnityEngine;

public interface IFsmStateEvents : IInstanceIdentifiable
{
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
}

public interface IInstanceIdentifiable
{
    int EntityId { get; }
}
