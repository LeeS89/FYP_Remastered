using UnityEngine;

public readonly struct AbilityContext
{
    public readonly IAbilityOwner Owner;
    public readonly Transform CurrentOrigin;
    public readonly Transform GazeOrigin;
    public readonly Collider[] Targets;
    public readonly int TargetCount;
    public readonly float Now;

    public AbilityContext(IAbilityOwner owner, Transform origin, Transform gazeOrigin, Collider[] targets, int count, float now)
    {
        Owner = owner;
        CurrentOrigin = origin;
        GazeOrigin = gazeOrigin;
        Targets = targets;
        TargetCount = count;
        Now = now;
    }
}

public enum CuePhase { Start, Impact, End };

public interface IEffectExecutor
{
    void Execute(in AbilityContext context, EffectDef def, CuePhase phase);
}
