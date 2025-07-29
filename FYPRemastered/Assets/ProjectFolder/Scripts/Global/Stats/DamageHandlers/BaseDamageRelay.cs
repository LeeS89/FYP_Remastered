using UnityEngine;

public class BaseDamageRelay : ComponentEvents, IDamageable
{
    public void Knockback(float damage, Vector3 direction, float force, float duration)
    {
        _eventManager.Knockbacktriggered(direction, force, duration);
        NotifyDamage(damage);
    }

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

    public bool _testDeath = false;
    private void Update()
    {
        if (_testDeath)
        {
            NotifyDamage(1000, DamageType.Normal); // Simulate a death by applying a large amount of damage
            _testDeath = false;
        }
    }
}
