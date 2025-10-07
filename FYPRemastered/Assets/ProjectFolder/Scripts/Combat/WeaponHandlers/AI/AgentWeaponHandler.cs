using System;
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

    protected Coroutine _firingCoroutine;
    protected WaitUntil _waitUntilShotReady;
    protected WaitForSeconds _shotInterval;
    protected bool _shotIsReady = false;

    protected Action _fireSequenceCallback;

    protected Action PoolIsready;
    public bool PoolReady { get; protected set; } = false;

    public AgentWeaponHandler(EnemyEventManager eventManager, PoolIdSO poolId, GameObject owner, Transform bulletSpawnPoint, Transform target, int clipCapacity) : base(owner, poolId)
    {
        _eventManager = eventManager;
        _bulletSpawnPoint = bulletSpawnPoint;
        _target = target;
        _clipCapacity = clipCapacity;

        _fireSequenceCallback = TryFireRangedWeapon;
        PoolIsready = SetPoolReady;
        _rangedWeapon = new AgentRangedWeapon(poolId, _bulletSpawnPoint, _target, _eventManager, _owner, _clipCapacity, PoolIsready);
      //  _waitUntilShotReady = new WaitUntil(() => _shotIsReady);
        _shotInterval = new WaitForSeconds(0.25f);

        _eventManager.OnFireRangedWeapon += FireRangedweapon;
        _eventManager.OnAimingLayerReady += AimStateChanged;
        _eventManager.OnFacingTarget += FacingTargetStatusChanged;
        _eventManager.OnMelee += MeleeTriggered;
        _eventManager.OnTargetSeen += TargetInViewStatusChanged;
        _eventManager.OnOutOfAmmo += OutOfAmmo;
        _eventManager.OnReload += ReloadStateChanged;

        GameManager.OnPlayerDeathStatusChanged += TargetDead;
       
        

        EquipWeapon(WeaponType.Ranged);
        //_equippedWeapon = _rangedWeapon;
    }

    protected void SetPoolReady()
    {
        Debug.LogError("Pool Is Ready");
        PoolReady = true;
    }

    public override void EquipWeapon(WeaponType type)
    {
        base.EquipWeapon(type);
       // Debug.LogError("Weapon Equipped");

        if(_equippedWeapon is IRangedWeapon)
        {
            BeginFiringSequence();
        }
    }

    public override void UnEquipWeapon()
    {
        Debug.LogError("Weapon UnEquipped now");
        if (_firingCoroutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_firingCoroutine);
            _firingCoroutine = null;
        }
        base.UnEquipWeapon();
    }

    /// <summary>
    /// FBF Update relay
    /// </summary>
    public override void UpdateEquippedWeapon()
    {
        if(_equippedWeapon == null) { return; }

        if(_equippedWeapon is not IRangedWeapon || !IsTargetInView) 
        {
            if(_firingCoroutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(_firingCoroutine);
                _firingCoroutine = null;
            }
            return;
        }
        else
        {
            if(IsTargetInView && _firingCoroutine == null)
            {
                _firingCoroutine = this.StartSingleFireRoutine(_shotInterval/*, _fireSequenceCallback*/);
            }
        }
    }

    #region Firing condition Setters

    protected void TargetDead(bool isDead) => IsTargetDead = isDead;
    //protected void TargetRespawned() => IsTargetDead = false;

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
        if (!PoolReady) return;
        if (CanFire())
        {
            _eventManager.AnimationTriggered(AnimationAction.Shoot);
        }
        _shotIsReady = false;
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
            if (isReloading)
            {
                rw.Reload();

                /*if(_firingCoroutine != null)
                {
                    CoroutineRunner.Instance.StopCoroutine(_firingCoroutine);
                    _firingCoroutine = null;
                }*/
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

        GameManager.OnPlayerDeathStatusChanged -= TargetDead;
        
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

        GameManager.OnPlayerDeathStatusChanged += TargetDead;
       

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
