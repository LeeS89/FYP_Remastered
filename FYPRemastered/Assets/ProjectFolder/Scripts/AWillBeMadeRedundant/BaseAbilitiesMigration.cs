using UnityEngine;

public class BaseAbilitiesMigration : ComponentEvents
{
    // protected GunBase _gun;
    protected Transform _bulletSpawnLocation;
    protected Transform _targetTransform;
    protected GameObject _owner;
    protected IPoolManager _bulletPoolManager;
    //  protected ResourceRequest _request;
    protected bool _ownerHasDied = false;
    protected bool _isReloading = false;
    [SerializeField] protected int _clipCapacity = 5;

    public bool IsReloading
    {
        get => _isReloading;
        set => _isReloading = value;
    }

    public Transform BulletSpawnLocation
    {
        get => _bulletSpawnLocation;
        set => _bulletSpawnLocation = value;
    }

    public Transform TargetTransform
    {
        get => _targetTransform;
        set => _targetTransform = value;
    }

    public override void RegisterLocalEvents(EventManagerObsolete eventManager)
    {
        base.RegisterLocalEvents(eventManager);
     
    }



    protected virtual void ShootGun()
    {
        /* if(_gun == null || !CanShoot()) { return; }

         _gun.Shoot();*/
    }

    protected virtual FireConditions GetFireState()
    {
        if (_ownerHasDied) return FireConditions.OwnerDied;
        if (_isReloading) return FireConditions.Reloading;
        return FireConditions.Ready;
    }

    public bool _testShootStatus = false;

    protected virtual bool CanShoot()
    {
        FireConditions fireState = GetFireState();
        // Debug.LogError("Fire State: " + fireState);
        return fireState == FireConditions.Ready;
        //return GetFireState() == FireConditions.Ready;
    }

    protected virtual void OutOfAmmo() { }

  

    protected override void OnSceneComplete()
    {
        //if (_gun != null)
        //{
        //  _gun.OnInstanceDestroyed();
        _owner = null;
        _bulletSpawnLocation = null;
        _targetTransform = null;
        //_gun = null;
        //}

    }
}
