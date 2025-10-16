using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectRemaster.Combat;

public sealed class Rifle : Weapon, IRanged
{
    [Header("Pool Params")]
    [SerializeField] private PoolIdSO bulletPoolId;
    [SerializeField] private PoolIdSO muzzleFlashPoolId;
    private IPoolManager _muzzleFlashPool;
    private IPoolManager _bulletPool;
    private Action<string, IPoolManager> PoolRequestCallback;

    [Header("Bullet Spawn Point & ammo params")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _muzzleFlashSpawnPoint;
    [SerializeField] private int _clipCapacity;
    [SerializeField] private int _clipCount;
    private int _leftInClip;

    [Header("Rates at which this weaon can fire, i.e. Single, Burst, Fully Automatic")]
    [SerializeField] private List<FireRateParams> _params;
    private Dictionary<FireRate, FireRateParams> _fireStates = new(3);
    private FireRateParams _currentFireRate;
    [SerializeField] private FireRate _defaultFireRate = FireRate.SingleAutomatic;

    [Header("Firing sequence params")]
   // public float NextTick { get; private set; }
    private float _fireCooldown;
    public bool AutoFiring { get; private set; } = false;
    private bool _lockedAndLoaded = true;
    public bool WeaponReady { get; private set; } = false;

    private Transform _target; // Used by NPC's to fire in direction of player



    #region Initialization

    private void Start()
    {
        if(bulletPoolId != null && !string.IsNullOrEmpty(bulletPoolId.Id))
        {
            PoolRequestCallback = OnPoolReceived;
            this.RequestPool(bulletPoolId, PoolRequestCallback);
            if(muzzleFlashPoolId != null) this.RequestPool(muzzleFlashPoolId, PoolRequestCallback);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new NullReferenceException("Must provide a valid Pool Id");
#else
            return;
#endif

        }

        _leftInClip = _clipCapacity;

        foreach(var rate in _params)
        {
            FireRate fr = rate._fireRate;
            if (!_fireStates.TryAdd(fr, rate))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Cannot add duplicate FireRate");
#endif
                return;

            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Successfully added FireRate");
#endif
            }

        }
    }

    public override void Equip(EventManager eventManager, IWeaponOwner owner = null)
    {
        EnsureBulletPoolExists();
        base.Equip(eventManager, owner); 
    }

    private void EnsureBulletPoolExists()
    {
        if (_bulletPool != null) return;
      
        if (bulletPoolId != null && !string.IsNullOrEmpty(bulletPoolId.Id))
        {
            PoolRequestCallback = OnPoolReceived;
            this.RequestPool(bulletPoolId, PoolRequestCallback);
            if (muzzleFlashPoolId != null) this.RequestPool(muzzleFlashPoolId, PoolRequestCallback);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new NullReferenceException("Must provide a valid Pool Id");
#else
            return;
#endif

        }
    }

    public void TriggerPressed() => EventManager.TriggerPressed();
    public void TriggerReleased() => EventManager.TriggerReleased();

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        Debug.LogError("Pool Id: "+poolId);
        if (string.IsNullOrEmpty(poolId) || pool == null) return;
        if(poolId == bulletPoolId.Id)
        {
            _bulletPool = pool;
            _leftInClip = _clipCapacity;
            WeaponReady = true;
        }
        else if(poolId == muzzleFlashPoolId.Id) _muzzleFlashPool = pool;

    }

    public void SetFireRate(FireRate rate)
    {
        if (_currentFireRate != null && _currentFireRate._fireRate == rate) return;
        if (_fireStates == null) return;
       
        if (_fireStates.TryGetValue(rate, out var frp))
        {
            _currentFireRate = frp;

        }
        else
        {
            FireRateParams fr = new FireRateParams();
            fr._fireRate = rate;
            _fireStates.Add(rate, fr);
        }
       
    }
    #endregion

    #region Firing Region
    public void TryFire() => TryFire(_defaultFireRate);

    public void TryFire(FireRate rate = FireRate.SingleAutomatic, Transform target = null)
    {
        if (!Equipped) return;
        Target = target;
        SetFireRate(rate);

        if (rate == FireRate.SingleAutomatic || rate == FireRate.Burst || rate == FireRate.FullAutomatic) StartAutoFire(rate);
        else EnsureAmmoAndFire();
    }

    private void EnsureAmmoAndFire()
    {
        if (_leftInClip > 0)
        {
            if (_owner != null &&_owner.IsNPC) EventManager.ReadyToFire(this);
            else Fire();
        }
        else ClipEmpty();

    }

    public void Fire()
    {
        _leftInClip--;
        Vector3 directionToTarget = Target != null ? TargetingUtility.GetDirectionToTarget(_target, _spawnPoint, true) :
            _spawnPoint.forward;
        Quaternion rotation = Quaternion.LookRotation(directionToTarget);

        GameObject obj = _bulletPool?.GetFromPool(_spawnPoint.position, rotation) as GameObject;
        ParticleSystem ps = _muzzleFlashPool?.GetFromPool(_muzzleFlashSpawnPoint.position, rotation) as ParticleSystem;

        if (obj == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve bullet from pool");
#endif
            return;

        }

        if (ComponentRegistry.TryGet<IPoolable>(obj, out var bullet))
        {
            if (ps != null)
            {
                ps.transform.SetParent(_muzzleFlashSpawnPoint);//.parent = _muzzleFlashSpawnPoint;//.transform;
                ps.Play();
            }
            bullet.LaunchPoolable(/*gameObject*/_owner.GameObject);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve IPoolable component on bullet");
#endif
        }

    }
    #endregion

    #region Reload/ Out of ammo notifications
    public void ClipEmpty()
    {
        if (EventManager == null) return;

        _lockedAndLoaded = false;
      //  SetWeaponReady(false);
        // EndAutoFire();
        if (_clipCount > 0) EventManager.NotifyReload();
        else
        {
            EndAutoFire();
            EventManager.OutOfAmmo();
        }

        // For Player => Maybe some text, SFX, voice over etc, and weapon UI update in derived class
        // For NPC => Trigger Reload animation in derived class
    }

    public void Reload()
    {
        // For now, NPC's have infinite ammo, so no need to check for clip count
        if (!_owner.IsNPC) _clipCount--;
        _leftInClip = _clipCapacity;
        _lockedAndLoaded = true;
        // Reload Audio
    }
    #endregion


    #region End Fire, unEquip & Interupt
    public void OnInterupted() => EndAutoFire();


    public override void UnEquip()
    {
        EndAutoFire();
        _currentFireRate = null;
        base.UnEquip();
    }
    #endregion


    #region Auto Fire Start/End
    private void StartAutoFire(FireRate rate)
    {
        EnsureAmmoAndFire(); // Initial fire on trigger press
        EnsureAutorFireRateExists(rate);
        _fireCooldown = _currentFireRate.GetNextInterval();
       // NextTick = Time.time + _currentFireRate.GetNextInterval();
        AutoFiring = true;
    }

    private void EndAutoFire()
    {
        if (!AutoFiring) return;
        AutoFiring = false;
    }

    private void EnsureAutorFireRateExists(FireRate rate)
    {
        if (!Equipped) return;
        if (_currentFireRate == null || _currentFireRate._fireRate != rate)
        {
            SetFireRate(rate);
        }
    }
    #endregion

    #region Boolean Sets/ Checks
    private bool CanFire()
    {
        if (!Equipped || _currentFireRate == null) return false;
        if (!_lockedAndLoaded) return false;

        return true;
    }

    #endregion


    private void Update()
    {
        if (!AutoFiring || !CanFire()) return;
       
        if(_fireCooldown > 0)
        {
            _fireCooldown -= Time.deltaTime;
            return;
        }
       // if (Time.time < NextTick) return;
        EnsureAmmoAndFire();

        _fireCooldown = _currentFireRate.GetNextInterval();
       // NextTick += _currentFireRate.GetNextInterval();
    }

   
}
