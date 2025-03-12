using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletEventManager : EventManager, IEventManager
{
    private List<IBulletEvents> _cachedListeners;
    public event Action OnExpired;
    public event Action OnFired;
    public event Action OnCollision;
    public event Action<Vector3, Quaternion, float> OnDeflected;
    public event Action<BulletBase, BulletType> OnBulletParticlePlay;
    public event Action<BulletBase, BulletType> OnBulletParticleStop;
    public event Action<Vector3, Quaternion> OnSpawnHitParticle;
    public event Action<PoolManager> OnInjectPoolManager;
    //public event Action OnStartMovement;


    private void Awake()
    {
        _cachedListeners = new List<IBulletEvents>();
        //BindComponentsToEvents();
    }

    public override void BindComponentsToEvents()
    {
 
        var childListeners = GetComponentsInChildren<IBulletEvents>();
        _cachedListeners.AddRange(childListeners);

        foreach(var listener in _cachedListeners)
        {
            //Debug.LogError($"Registered listener: {listener.GetType().Name} on {((MonoBehaviour)listener).gameObject.name}");
            listener.RegisterEvents(this);
        }
        InjectPoolManager(_poolManager);
    }

    public void InjectPoolManager(PoolManager manager)
    {
        OnInjectPoolManager?.Invoke(manager);
    }
    public override void ParentPoolInjection(PoolManager poolManager)
    {

        _poolManager = poolManager;
    }

    public void ParticlePlay(BulletBase bullet, BulletType bulletType)
    {
        OnBulletParticlePlay?.Invoke(bullet, bulletType);
    }

    public void ParticleStop(BulletBase bullet, BulletType bulletType)
    {
        OnBulletParticleStop?.Invoke(bullet, bulletType);
    }

    public void SpawnHitParticle(Vector3 position, Quaternion rotation)
    {
        OnSpawnHitParticle?.Invoke(position, rotation);
    }

    public void Fired()
    {
        if (OnFired != null)
        {
            OnFired?.Invoke();
        }
        else
        {
            Debug.LogError("Event Is Null");
        }

    }

    public void Deflected(Vector3 direction, Quaternion rotation, float speed)
    {
        OnDeflected?.Invoke(direction, rotation, speed);
    }

    public void Collision()
    {
        OnCollision?.Invoke();
    }

    public void Expired()
    {
        OnExpired?.Invoke();
    }
}
