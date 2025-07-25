using UnityEngine;

public class WeaponHandlerBase
{
    protected GunBase _gun;
    protected GameObject _owner;
    protected EnemyEventManager _eventManager;
    protected Transform _bulletSpawnPoint;
    protected int _clipCapacity;
    protected Transform _target;
    protected PoolManager _bulletPoolManager;
    protected ResourceRequest _request;

    protected bool _meleeTriggered = false;
    protected bool _isAimReady = false;
    protected bool _isfacingTarget = false;
    protected bool _targetInView = false;
    protected bool _targetDead = false;
    protected bool _ownerHasDied = false;
    protected bool _isReloading = false;


    public virtual void EquipGun(GameObject gunOwner, EnemyEventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target = null)
    {
        if(_gun != null) { return; }

        _eventManager = eventManager;

        //_eventManager.OnTryShoot += TryShootgun;
        _eventManager.OnShoot += ShootGun;
        _eventManager.OnAimingLayerReady += SetAimReady;
        _eventManager.OnFacingTarget += SetFacingtarget;
        _eventManager.OnMelee += SetMeleeTriggered;
        _eventManager.OnTargetSeen += UpdateTargetVisibility;
        _eventManager.OnOutOfAmmo += OutOfAmmo;
        _eventManager.OnReload += ReloadingGun;

        GameManager.OnPlayerDied += OnTargetDied;
        GameManager.OnPlayerRespawn += OnTargetRespawned;

        _request = new ResourceRequest();
        _owner = gunOwner;
        
        _bulletSpawnPoint = bulletSpawnLocaiton;
        _clipCapacity = clipCapacity;
        _target = target;

        _request.ResourceType = PoolResourceType.NormalBulletPool;
        _request.poolRequestCallback = SetBulletPool;

        SceneEventAggregator.Instance.RequestResource(_request);

        _gun = new GunBase(_bulletSpawnPoint, _target, _eventManager, _owner, _bulletPoolManager, _clipCapacity);
    }



    protected void SetMeleeTriggered(bool isMelee)
    {
        _meleeTriggered = isMelee;
    }

    protected void SetAimReady(bool isReady)
    {
        _isAimReady = isReady;
    }

    protected void UpdateTargetVisibility(bool targetInView)
    {
        _targetInView = targetInView;
    }

    protected void SetFacingtarget(bool isFacingTarget)
    {
        _isfacingTarget = isFacingTarget;
    }


    protected virtual void OnTargetDied()
    {
        _targetDead = true;
    }

    protected virtual void OnTargetRespawned()
    {
        _targetDead = false;
    }









    protected virtual void SetBulletPool(PoolManager pool)
    {
        _bulletPoolManager = pool;
    }

    public virtual void ShootGun()
    {
        _gun.Shoot();
    }

    public virtual void TryShootGun()
    {
        if (CanShoot() && _gun != null)
        {
            _eventManager.AnimationTriggered(AnimationAction.Shoot);
        }
    }

    /*protected virtual void Reload()
    {
        _gun.Reload();
    }*/

    protected virtual void OutOfAmmo()
    {
        _eventManager.AnimationTriggered(AnimationAction.Reload);
    }

    protected virtual void ReloadingGun(bool isReloading)
    {
        _isReloading = isReloading;
        if (_isReloading)
        {
            _gun.Reload();
        }
    }

    protected virtual FireConditions GetFireState()
    {
        if (_ownerHasDied) return FireConditions.OwnerDied;
        if (_isReloading) return FireConditions.Reloading;
        if (_targetDead) return FireConditions.TargetDied;
        if (!_targetInView) return FireConditions.TargetNotInView;
        if (_meleeTriggered) return FireConditions.Meleeing;
        if (!_isAimReady || !_isfacingTarget) return FireConditions.NotAiming;
        return FireConditions.Ready;
        //return base.GetFireState();
    }

    protected virtual bool CanShoot()
    {
        FireConditions fireState = GetFireState();
        // Debug.LogError("Fire State: " + fireState);
        return fireState == FireConditions.Ready;
        //return GetFireState() == FireConditions.Ready;
    }
}
