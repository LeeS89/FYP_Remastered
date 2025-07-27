using UnityEngine;

public class AgentWeaponHandler : WeaponHandlerBase
{
    protected EnemyEventManager _eventManager;
    protected Transform _target;
    protected float[] _fireRateOffsets;

    //private Dictionary<WeaponType, IWeapon> _weapons; => Possible future use, leave in for now


    public virtual bool IsAimReady { get; protected set; }
    public virtual bool IsTargetInView { get; protected set; }
    public virtual bool IsFacingTarget { get; protected set; }
    public virtual bool IsTargetDead { get; protected set; }
    public virtual bool IsMeleeTriggered { get; protected set; }
  


    public AgentWeaponHandler(EnemyEventManager eventManager, GameObject owner, Transform bulletSpawnPoint, Transform target, int clipCapacity) : base(owner)
    {
        _eventManager = eventManager;
        _bulletSpawnPoint = bulletSpawnPoint;
        _target = target;
        _clipCapacity = clipCapacity;

        _rangedWeapon = new AgentRangedWeapon(_bulletSpawnPoint, _target, _eventManager, _owner, _clipCapacity);

        _eventManager.OnFireRangedWeapon += FireRangedweapon;
        _eventManager.OnAimingLayerReady += AimStateChanged;
        _eventManager.OnFacingTarget += FacingTargetStatusChanged;
        _eventManager.OnMelee += MeleeTriggered;
        _eventManager.OnTargetSeen += TargetInViewStatusChanged;
        _eventManager.OnOutOfAmmo += OutOfAmmo;
        _eventManager.OnReload += ReloadStateChanged;

        GameManager.OnPlayerDied += TargetDead;
        GameManager.OnPlayerRespawn += TargetRespawned;

        EquipWeapon(WeaponType.Ranged);
        //_equippedWeapon = _rangedWeapon;
    }

    #region Firing condition Setters

    protected void TargetDead() => IsTargetDead = true;
    protected void TargetRespawned() => IsTargetDead = false;

    protected void MeleeTriggered(bool isMelee) => IsMeleeTriggered = isMelee;
  
    protected void FacingTargetStatusChanged(bool isFacing) => IsFacingTarget = isFacing;

    protected void TargetInViewStatusChanged(bool inView) => IsTargetInView = inView;

    protected void AimStateChanged(bool isReady) => IsAimReady = isReady;

    protected override FireConditions GetFireState()
    {
        if (IsTargetDead) return FireConditions.TargetDied;
        if (!IsTargetInView) return FireConditions.TargetNotInView;
        if (IsMeleeTriggered) return FireConditions.Meleeing;
        if (!IsAimReady || !IsFacingTarget) return FireConditions.NotAiming;
        return base.GetFireState();
    }

    #endregion



    public override void FireRangedweapon()
    {
        if(_equippedWeapon is IRangedWeapon rw)
        {
            Vector3 directionToTarget = TargetingUtility.GetDirectionToTarget(_target, _bulletSpawnPoint, true);
           
            rw.Fire(directionToTarget);
        }
    }


    public override void TryFireRangedWeapon()
    {
        if (CanFire())
        {
            _eventManager.AnimationTriggered(AnimationAction.Shoot);
        }
    }

  

    protected override void OutOfAmmo()
    {
        _eventManager.AnimationTriggered(AnimationAction.Reload);
    }

    protected override void ReloadStateChanged(bool isReloading)
    {
        if (_equippedWeapon is IRangedWeapon rw)
        {
            base.ReloadStateChanged(isReloading);
            if (IsReloading)
            {
                rw.Reload();
            }
        }
    }

    public virtual void InjectFireRateOffsets(float[] offsets) { }

    public override void OnInstanceDestroyed()
    {
        _eventManager.OnFireRangedWeapon -= FireRangedweapon;
        _eventManager.OnAimingLayerReady -= AimStateChanged;
        _eventManager.OnFacingTarget -= FacingTargetStatusChanged;
        _eventManager.OnMelee -= MeleeTriggered;
        _eventManager.OnTargetSeen -= TargetInViewStatusChanged;
        _eventManager.OnOutOfAmmo -= OutOfAmmo;
        _eventManager.OnReload -= ReloadStateChanged;

        GameManager.OnPlayerDied -= TargetDead;
        GameManager.OnPlayerRespawn -= TargetRespawned;
        base.OnInstanceDestroyed();
    }

    #region Future Implementations/ fixes
    public override void EquipWeapon(GameObject owner, EnemyEventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target = null)
    {
        // if (_gun != null) { return; }

        _eventManager = eventManager;

        //_eventManager.OnTryShoot += TryShootgun;
        _eventManager.OnFireRangedWeapon += FireRangedweapon;
        _eventManager.OnAimingLayerReady += AimStateChanged;
        _eventManager.OnFacingTarget += FacingTargetStatusChanged;
        _eventManager.OnMelee += MeleeTriggered;
        _eventManager.OnTargetSeen += TargetInViewStatusChanged;
        _eventManager.OnOutOfAmmo += OutOfAmmo;
        _eventManager.OnReload += ReloadStateChanged;

        GameManager.OnPlayerDied += TargetDead;
        GameManager.OnPlayerRespawn += TargetRespawned;

        // _request = new ResourceRequest();
        _owner = owner;

        _bulletSpawnPoint = bulletSpawnLocaiton;
        _clipCapacity = clipCapacity;
        _target = target;

        // _request.ResourceType = PoolResourceType.NormalBulletPool;
        //_request.poolRequestCallback = SetBulletPool;

        // SceneEventAggregator.Instance.RequestResource(_request);

        // _gun = new GunBase(_bulletSpawnPoint, _target, _eventManager, _owner, _bulletPoolManager, _clipCapacity);
    }
    #endregion

}
