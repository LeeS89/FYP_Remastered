using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletEventManager : MonoBehaviour, IEventManager
{
    private List<IBulletEvents> _cachedListeners;
    public event Action OnExpired;
    public event Action OnFired;
    public event Action OnCollision;
    public event Action<Quaternion, float> OnDeflected;
    public event Action<BulletBase, BulletType> OnParticlePlay;
    //public event Action OnStartMovement;


    private void Awake()
    {
        _cachedListeners = new List<IBulletEvents>();
        BindComponentsToEvents();
    }

    public void BindComponentsToEvents()
    {
 
        var childListeners = GetComponentsInChildren<IBulletEvents>();
        _cachedListeners.AddRange(childListeners);

        foreach(var listener in _cachedListeners)
        {
            //Debug.LogError($"Registered listener: {listener.GetType().Name} on {((MonoBehaviour)listener).gameObject.name}");
            listener.RegisterEvents(this);
        }
    }

    public void ParticlePlay(BulletBase bullet, BulletType bulletType)
    {
        OnParticlePlay?.Invoke(bullet, bulletType);
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

    public void Deflected(Quaternion rotation, float speed)
    {
        OnDeflected?.Invoke(rotation, speed);
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
