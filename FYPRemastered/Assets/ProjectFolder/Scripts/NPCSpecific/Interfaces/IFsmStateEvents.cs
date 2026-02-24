using UnityEngine;

public interface IFsmStateEvents
{
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
}
