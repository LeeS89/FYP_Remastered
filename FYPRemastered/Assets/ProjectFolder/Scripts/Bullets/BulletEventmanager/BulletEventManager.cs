using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletEventManager : EventManager
{
    private List<ComponentEvents> _cachedListeners;
    public event Action OnExpired;
    public event Action OnFired;
    public event Action<Collision> OnCollision;
    public event Action OnDeflected;
    public event Func<Vector3> OnGetDirectionToTarget;
    public event Action OnFreeze;
    public event Action OnReverseDirection;
    public event Action<BulletBase/*, BulletType*/> OnBulletParticlePlay;
    public event Action<BulletBase/*, BulletType*/> OnBulletParticleStop;
    
    public event Action<bool> OnCull;
   


    /*private bool _isAlreadyInitialized = false;

    public bool IsAlreadyInitialized
    {
        get => _isAlreadyInitialized;
        private set
        {
            _isAlreadyInitialized = value;
        }
     
    }*/

    private void Awake()
    {
        BindComponentsToEvents();
    }

    public override void BindComponentsToEvents()
    {
       
        _cachedListeners = new List<ComponentEvents>();

        var childListeners = GetComponentsInChildren<ComponentEvents>();
        _cachedListeners.AddRange(childListeners);

        foreach(var listener in _cachedListeners)
        {
          
            listener.RegisterLocalEvents(this);
        }
       
    }

    public override void UnbindComponentsToEvents()
    {
        foreach(var listener in _cachedListeners)
        {
            listener.UnRegisterLocalEvents(this);
        }
        _cachedListeners?.Clear();
        _cachedListeners = null;
        
    }

    

    public void ParticlePlay(BulletBase bullet/*, BulletType bulletType*/)
    {
        OnBulletParticlePlay?.Invoke(bullet/*, bulletType*/);
    }

    public void ParticleStop(BulletBase bullet/*, BulletType bulletType*/)
    {
        OnBulletParticleStop?.Invoke(bullet/*, bulletType*/);
    }


    public void Cull(bool cull = false)
    {
        OnCull?.Invoke(cull);
    }

    public void Fired()
    {
        OnFired?.Invoke();
    }

    public void Deflected()
    {
        OnDeflected?.Invoke();
    }

    public Vector3 GetDirectionToTarget()
    {
        return OnGetDirectionToTarget?.Invoke() ?? Vector3.zero;
    }

    public void Freeze()
    {
        OnFreeze?.Invoke();
    }

    public void ReverseDirection()
    {
        OnReverseDirection?.Invoke();
    }

    public void Collision(Collision collision)
    {
        OnCollision?.Invoke(collision);
    }

    public void Expired()
    {
        OnExpired?.Invoke();
    }

    
}
