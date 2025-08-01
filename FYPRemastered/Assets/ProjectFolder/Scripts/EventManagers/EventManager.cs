using System;
using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    //protected PoolManager _poolManager;

    public abstract void BindComponentsToEvents();

    public abstract void UnbindComponentsToEvents();

    public event Action<bool> OnOwnerDeathStatusUpdated;

    public void OwnerDeathStatusUpdated(bool isDead)
    {
        OnOwnerDeathStatusUpdated?.Invoke(isDead);
    }

    public event Action<float, DamageType, float, float, float> OnNotifyDamage;

    public void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        OnNotifyDamage?.Invoke(baseDamage, dType, statusEffectChancePercentage, damageOverTime, duration);
    }

    //GunSetup(GameObject gunOwner, EventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target = null)
    /*public event Action<GameObject, EventManager,Transform, int, Transform> OnSetupGun;

    public void SetupGun(GameObject gunOwner, EventManager eventManager, Transform bulletSpawnLocation, int clipCapacity, Transform target = null)
    {
        OnSetupGun?.Invoke(gunOwner, eventManager, bulletSpawnLocation, clipCapacity, target);
    }*/

    public event Action<FireConditions> OnTryShoot;

    public void TryShoot(FireConditions conditions)
    {
        OnTryShoot?.Invoke(conditions);
    }

    public event Action OnOutOfAmmo;

    public void OutOfAmmo()
    {
        OnOutOfAmmo?.Invoke();
    }

    public event Action<bool> OnReload;

    public void Reload(bool isReloading)
    {
        OnReload?.Invoke(isReloading);
    }

    public event Action<Vector3, float, float> OnKnockbackTriggered;

    public void Knockbacktriggered(Vector3 direction, float force, float duration)
    {
        OnKnockbackTriggered?.Invoke(direction, force, duration);
    }

    // Gun Events
    public event Action OnFireRangedWeapon;

    public void FireRangedWeapon()
    {
        OnFireRangedWeapon?.Invoke();
    }
}
