using UnityEngine;


[CreateAssetMenu(fileName = "AbilityDef", menuName = "Abilities/Ability")]
public class AbilityDef : ScriptableObject
{
    public AbilityTags Tag;
    public AbilityTags[] GrantTags;
    public AbilityTags[] BlockedByTags;
    public AbilityTags[] RequiredTags;
    public float CooldownSeconds;
    public ResourceCosts[] Costs;
    public TargetingDef Targeting;
    public EffectType[] Effects;
    public PoolIdSO StartPhasePool, ImpactPhasePool, EndPhasePool;
    public CueDef Start, Impact, End;
    public bool IsChanneled;
    public bool UsesFixedUpdate;
    public float ChannelTick;
}

[System.Serializable] public struct ResourceCosts
{
    public StatType ResourceType;
    public float Amount;
}
