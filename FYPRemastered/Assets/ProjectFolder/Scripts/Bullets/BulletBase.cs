using System.Collections;
using UnityEngine;

public abstract class BulletBase : MonoBehaviour, IBulletEvents
{
    protected GameObject _cachedRoot;
    protected BulletEventManager _eventManager;

    

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if (eventManager == null) { return; }

        _eventManager = eventManager;
        _eventManager.OnExpired += OnExpired;
        _cachedRoot = transform.root.gameObject;
        StartCoroutine(FireDelay());

    }

    IEnumerator FireDelay()
    {
        yield return new WaitForEndOfFrame();
        Initializebullet();
    }

    public virtual void Initializebullet()
    {
        _eventManager.Fired();
    }

    protected abstract void OnExpired();

    public abstract void Freeze();
    public abstract void UnFreeze();

    
}
