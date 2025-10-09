using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Rifle : Weapon, IRanged
{
    [SerializeField] private PoolIdSO poolId;
    private IPoolManager _pool;
    [SerializeField] private Transform _spawnPoint;
   // [SerializeField] protected FireRate _fireRate = FireRate.Single;
    private Action<string, IPoolManager> PoolRequestCallback;

    [SerializeField] private int _clipCapacity;
    [SerializeField] private int _clipCount;
    private int _leftInClip;
    private Transform _target; // Used by NPC's to fire in direction of player

    [SerializeField] private List<FireRateParams> _params;
    private Dictionary<FireRate, FireRateParams> _fireStates = new(3);
    private FireRateParams _currentFireRate;
    public float NextTick { get; private set; }
    public bool AutoFiring { get; private set; } = false;

    public bool LockedAndLoaded { get; private set; } = true;

    #region Initialization

    private void Start()
    {
        if(poolId != null && !string.IsNullOrEmpty(poolId.Id))
        {
            PoolRequestCallback = OnPoolReceived;
            this.RequestPool(poolId, PoolRequestCallback);
        }
        else
        {
#if UNITY_EDITOR
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
#if UNITY_EDITOR
                Debug.LogError("Cannot add duplicate FireRate");
                return;
#endif
            }
            else
            {
                Debug.LogError("Successfully added FireRate");
            }

        }
    }

  
    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || poolId != this.poolId.Id || pool == null) return;
        Debug.LogError("Pool Request Completed");
        _pool = pool;

       // Equip(new EnemyEventManager());
       // TryFire(FireRate.FullAutomatic);
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
        SetWeaponReady(true);
    }
    #endregion

    #region Firing Region
    public void TryFire(FireRate rate, Transform target = null)
    {
        Target = target;
        SetFireRate(rate);

        if (rate == FireRate.SingleAutomatic || rate == FireRate.Burst || rate == FireRate.FullAutomatic) StartAutoFire(rate);
        else TryFire();
    }

    private void TryFire()
    {
        if (_leftInClip > 0)
        {
            if (_owner != null &&_owner.IsNPC) _eventManager.ReadyToFire(this);
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

        GameObject obj = _pool?.GetFromPool(_spawnPoint.position, rotation) as GameObject;

        if (obj == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve bullet from pool");
#endif
            return;

        }

        if (ComponentRegistry.TryGet<IPoolable>(obj, out var bullet)) bullet.LaunchPoolable(gameObject/*_owner.GameObject*/);
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
        if (_eventManager == null) return;
        SetWeaponReady(false);
        // EndAutoFire();
        if (_clipCount > 0) _eventManager.NotifyReload();
        else
        {
            EndAutoFire();
            _eventManager.OutOfAmmo();
        }

        // For Player => Maybe some text, SFX, voice over etc
        // For NPC => Trigger Reload animation in derived class
    }

    public void Reload()
    {
        _clipCount--;
        _leftInClip = _clipCapacity;
        SetWeaponReady(true);
        // Reload Audio
    }
    #endregion


    #region End Fire, unEquip & Interupt
    public void OnInterupted() => EndAutoFire();


    public override void UnEquip()
    {
        // Reset weapon ready to true on UnEquip so it is ready when re-equipped
        SetWeaponReady(true);
        EndAutoFire();
        _currentFireRate = null;
        base.UnEquip();
    }
    #endregion


    #region Auto Fire Start/End
    private void StartAutoFire(FireRate rate)
    {
        EnsureAutorFireRateExists(rate);
        NextTick = Time.time + _currentFireRate.GetNextInterval();
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
            SetWeaponReady(false);
            SetFireRate(rate);
        }
    }
    #endregion

    #region Boolean Sets/ Checks
    private bool CanFire()
    {
        if (!Equipped || _currentFireRate == null) return false;
        if (!LockedAndLoaded) return false;

        return true;
    }

    private void SetWeaponReady(bool ready)
    {
        _owner?.NotifyWeaponReady(ready);
        LockedAndLoaded = ready;
    }
    #endregion


    private void Update()
    {
        if (!AutoFiring || !CanFire()) return;
       
        if (Time.time < NextTick) return;
        TryFire();

        NextTick += _currentFireRate.GetNextInterval();
    }

   


    

   
}
