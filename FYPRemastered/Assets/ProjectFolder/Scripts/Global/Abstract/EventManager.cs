using System;
using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    protected PoolManager _poolManager;

    public abstract void BindComponentsToEvents();

    public abstract void UnbindComponentsToEvents();

    public event Action OnOwnerDied;

    public void OwnerDied()
    {
        OnOwnerDied?.Invoke();
    }

    public event Action<float, DamageType, float, float, float> OnNotifyDamage;

    public void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        OnNotifyDamage?.Invoke(baseDamage, dType, statusEffectChancePercentage, damageOverTime, duration);
    }
}
