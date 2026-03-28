using System;
using UnityEngine;

public enum EffectType {None, Damage, Knockback, FreezeAndTrack, ReturnTrackedOnEnd, SpawnAndFire }

[Obsolete("",true)]
[CreateAssetMenu(fileName = "EffectDef", menuName = "Abilities/EffectDef")]
public class EffectKind : ScriptableObject
{
    public EffectKind Kind;
    public PoolIdSO PoolId;
    public float Magnitude;
    public string DamageType;
}
