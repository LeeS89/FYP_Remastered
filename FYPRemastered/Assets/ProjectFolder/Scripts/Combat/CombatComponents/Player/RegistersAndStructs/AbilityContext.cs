using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public readonly CuePhase Phase; 
    

    public AbilityContext(IAbilityOwner owner, CuePhase phase, Transform fireOrigin, Collider[] targets, int count, float now, float directionOffset = 0f, Transform directionOrigin = null)
    {
        Owner = owner;
        Phase = phase;
        AbilityFireOrigin = fireOrigin;
        AbilityDirectionOrigin = directionOrigin;
        Targets = targets;
        TargetCount = count;
        Now = now;
        DirectionOffset = directionOffset;
        if (AbilityDirectionOrigin == null) AbilityDirectionOrigin = fireOrigin;
    }
}

public readonly struct AbilityResources
{
    public readonly AbilityTags AbilityTag;
    public readonly float Now;
    public readonly Dictionary<CuePhase, IPoolManager> AbilityPools;
  /*  public readonly IPoolManager _startPhasePool;
    public readonly IPoolManager _impactPhasePool;
    public readonly IPoolManager _endPhasePool;*/

    public AbilityResources(AbilityTags tag, float now, Dictionary<CuePhase, IPoolManager> pools)
    {
        AbilityTag = tag;
        Now = now;
        AbilityPools = pools;
       /* _startPhasePool = startPhasePool;
        _impactPhasePool = impactPhasePool;
        _endPhasePool = endPhasePool;*/
    }

    /*   public static AbilityResources SetStartPhasePool(AbilityTags tag, float now, IPoolManager pool)
           => new(tag, now, pool);

       public static AbilityResources SetImpactPhasePool(AbilityTags tag, float now, IPoolManager pool)
           => new(tag, now, null, pool);

       public static AbilityResources SetEndPhasePool(AbilityTags tag, float now, IPoolManager pool)
           => new(tag, now, null, null, pool);

       public static AbilityResources SetAllPhasePools(AbilityTags tag, float now, IPoolManager startPool, IPoolManager impactPool, IPoolManager endPool)
           => new(tag, now, startPool, impactPool, endPool);*/

   /* public static AbilityResources SetAbilityPools(AbilityTags tag, float now, Dictionary<CuePhase, IPoolManager> pools)
        => new(tag, now, pools);*/
}



public enum CuePhase { Start, Impact, End };

public interface IEffectExecutor
{
    void Execute(in AbilityContext context, EffectType def, IPoolManager pool = null);

  //  void UpdatePool(PoolIdSO poolId, Action<bool> actionCompleteCallback = null);

  

    bool IsReady { get; } 

}
