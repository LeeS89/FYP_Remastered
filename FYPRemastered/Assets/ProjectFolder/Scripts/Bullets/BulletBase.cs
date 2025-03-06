using System.Collections;
using UnityEngine;

public enum BulletType
{
    Normal,
    Fire
}


public abstract class BulletBase : MonoBehaviour, IBulletEvents
{
    protected GameObject _cachedRoot;
    protected BulletEventManager _eventManager;
    [SerializeField] protected float _deflectSpeed;
    [SerializeField] protected BulletType _bulletType;
    [SerializeField]
    protected GameObject _owner;
    public GameObject Owner
    {
        get => _owner;
        set
        {
            if (value != null) 
            {
                _owner = value;
            }
        }
    }

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if (eventManager == null) 
        {

            Debug.LogError("No event manager");
            return;
        }
        else
        {
            Debug.LogError("We have an event manager");
        }

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
        _eventManager.ParticlePlay(this, _bulletType);
        _eventManager.Fired();
    }
    public bool testBool = false;
    private void Update()
    {
        if (testBool)
        {
            Deflect();
            testBool = false;
        }
    }

    public virtual void Deflect()
    {
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(_owner, _cachedRoot);
        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);
        _eventManager.Deflected(newRotation, _deflectSpeed);
    }

    protected abstract void OnExpired();

    public abstract void Freeze();
    public abstract void UnFreeze();

    
}
