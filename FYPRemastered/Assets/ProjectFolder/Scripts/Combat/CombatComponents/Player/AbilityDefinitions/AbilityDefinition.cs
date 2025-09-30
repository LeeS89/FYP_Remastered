using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Abilities/AbilityDefinition")]
public class AbilityDefinition : ScriptableObject
{
    public AbilityTags Tag;
    public AbilityTags[] GrantTags;
    public AbilityTags[] BlockedByTags;
    public AbilityTags[] RequiredTags;
    public float CooldownSeconds;
    public ResourceCost Cost;
    public TargetingDef Targeting;
    public EffectType EffectKind;
    // public EffectDef[] Effects;
    public PoolIdSO StartPhasePool, ImpactPhasePool, EndPhasePool;
    public CueDef Start, Impact, End;
    public bool IsChanneled;
    public bool UsesFixedUpdate;
    public float ChannelTick;

    public bool UsesPools()
    {
        return StartPhasePool != null || ImpactPhasePool != null || EndPhasePool != null;
    }
}

[System.Serializable]
public struct ResourceCost
{
    public StatType ResourceType;
    public float Amount;
}
