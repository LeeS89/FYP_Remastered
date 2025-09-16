using System;
using UnityEngine;

public readonly struct AbilityContext
{
    public readonly IAbilityOwner Owner;
    public readonly Transform AbilityFireOrigin;
    /// <summary>
    /// Optional: If using a different direction origin from the fire origin
    /// </summary>
    public readonly Transform AbilityDirectionOrigin;
    /// <summary>
    /// Optional: Direction offset in degrees to apply to the direction origin's forward vector
    /// </summary>
    public readonly float DirectionOffset;
    public readonly Collider[] Targets;
    public readonly int TargetCount;
    public readonly float Now;
    

    public AbilityContext(IAbilityOwner owner, Transform fireOrigin, Collider[] targets, int count, float now, float directionOffset = 0f, Transform directionOrigin = null)
    {
        Owner = owner;
        AbilityFireOrigin = fireOrigin;
        AbilityDirectionOrigin = directionOrigin;
        Targets = targets;
        TargetCount = count;
        Now = now;
        DirectionOffset = directionOffset;
        if (AbilityDirectionOrigin == null) AbilityDirectionOrigin = fireOrigin;
    }
}

public enum CuePhase { Start, Impact, End };

public interface IEffectExecutor
{
    void Execute(in AbilityContext context, EffectDef def, CuePhase phase);

    void UpdatePool(PoolIdSO poolId, Action<bool> actionCompleteCallback = null);
   

    bool IsReady { get; } 

}
