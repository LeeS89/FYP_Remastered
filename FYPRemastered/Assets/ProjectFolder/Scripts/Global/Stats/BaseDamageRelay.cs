using UnityEngine;

public class BaseDamageRelay : ComponentEvents, IDamageable
{

    public void NotifyDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        if(_eventManager == null)
        {
            Debug.LogError("Event manager not found, please ensure valid event manager");
            return;
        }

        _eventManager.TakeDamage(
            baseDamage,
            dType,
            statusEffectChancePercentage,
            damageOverTime,
            duration
        );
    }
}
