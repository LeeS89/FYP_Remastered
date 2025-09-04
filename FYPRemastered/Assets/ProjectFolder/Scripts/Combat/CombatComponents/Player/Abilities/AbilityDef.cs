using UnityEngine;


[CreateAssetMenu(fileName = "AbilityDef", menuName = "Abilities/Ability")]
public class AbilityDef : ScriptableObject
{
    public AbilityTags Id;
    public AbilityTags[] GrantTags;
    public AbilityTags[] BlockedByTags;
    public AbilityTags[] RequiredTags;
    public float CooldownSeconds;
    public ResourceCost[] Costs;
    public TargetingDef Targeting;
    public EffectDef[] Effects;
    public CueDef Start, Impact, End;
    public bool IsChanneled;
    public float ChannelTick;
}

[System.Serializable] public struct ResourceCost
{
    public StatType ResourceType;
    public float Amount;
}
