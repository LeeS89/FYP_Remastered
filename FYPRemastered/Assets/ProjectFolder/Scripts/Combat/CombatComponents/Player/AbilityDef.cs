using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDef", menuName = "Abilities/Ability")]
public class AbilityDef : ScriptableObject
{
    public string Id;
    public string[] GrantTags;
    public string[] BlockedByTags;
    public string[] RequiredTags;
    public float CooldownSeconds;
    //public ResourceCost[] Costs;
    public TargetingDef Targeting;
    public EffectDef[] Effects;
    //public CueDef[] Cues;
    public bool IsChanneled;
    public float ChannelTick;
}

[System.Serializable] public struct ResourceCost
{
    public string Resource;
    public float Amount;
}
