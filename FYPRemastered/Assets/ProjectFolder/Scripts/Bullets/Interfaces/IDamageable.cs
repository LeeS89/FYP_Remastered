using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0f, float damageOverTime = 0f, float duration = 0f);
}
