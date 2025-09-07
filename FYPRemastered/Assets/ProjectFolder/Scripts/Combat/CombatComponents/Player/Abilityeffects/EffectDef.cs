using UnityEngine;

public enum EffectKind { Damage, Knockback, FreezeAndTrack, ReturnTrackedOnEnd, SpawnAndFire }

[CreateAssetMenu(fileName = "EffectDef", menuName = "Abilities/EffectDef")]
public class EffectDef : ScriptableObject
{
    public EffectKind Kind;
    public float Magnitude;
    public string DamageType;
}
