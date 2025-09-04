using UnityEngine;

public readonly struct AbilityContext
{
    public readonly IAbilityOwner Owner;
    public readonly Transform Origin;
    public readonly Collider[] Targets;
    public readonly int TargetCount;
    public readonly float Now;

    public AbilityContext(IAbilityOwner owner, Transform origin, Collider[] targets, int count, float now)
    {
        Owner = owner;
        Origin = origin;
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
