using UnityEngine;

public class FreezeAndTrackExecutor : IEffectExecutor
{
    private readonly TrackedFreezeables _tracker;

    public FreezeAndTrackExecutor(TrackedFreezeables tracker) => _tracker = tracker;

    public void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (phase != CuePhase.Impact) return;
        for(int i = 0; i < context.TargetCount; i++)
        {
            var go = context.Targets[i].gameObject;
            if (ComponentRegistry.TryGet(go, out IDeflectable freezeable))
            {
                if(_tracker.Add(freezeable)) freezeable.Freeze();
            }
        }
    }
}

public sealed class ReturnTrackedOnEndExecutor : IEffectExecutor
{
    private readonly TrackedFreezeables _tracker;

    public ReturnTrackedOnEndExecutor(TrackedFreezeables tracker) => _tracker = tracker;


    public void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (phase != CuePhase.End) return;
        _tracker?.ClearFreezeables();
    }
}
