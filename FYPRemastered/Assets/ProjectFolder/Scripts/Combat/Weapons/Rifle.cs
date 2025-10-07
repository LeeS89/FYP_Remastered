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

    public void Equip(EventManager eventManager, GameObject owner, FireRate fRate)
    {
        base.Equip(eventManager, owner);
        SetFireRate(fRate);
    }

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || poolId != this.poolId.Id || pool == null) return;
        Debug.LogError("Pool Request Completed");
        _pool = pool;
    }

    public override void Equip(EventManager eventManager, GameObject owner)
    {
        base.Equip(eventManager, owner);
    }

    public void TryFire(Transform target = null)
    {
        if (_leftInClip > 0) Fire(target);
        else NotifyReload();
    }

    public void Fire(Transform target = null)
    {
        //  if(_leftInClip > 0)
        //  {
        _leftInClip--;
        Vector3 directionToTarget = target != null ? TargetingUtility.GetDirectionToTarget(_target, _spawnPoint, true) :
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

        if (ComponentRegistry.TryGet<IPoolable>(obj, out var bullet)) bullet.LaunchPoolable(_owner);
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve IPoolable component on bullet");
#endif
        }
        // }
        /* else
         {
             NotifyReload();
         }*/
    }

    public  void NotifyReload()
    {
        // For Player => Maybe some text, SFX, voice over etc
        // For NPC => Trigger Reload animation in derived class
    }

    public void OnFireInterupted()
    {
        
    }

    public void Reload()
    {
        if (_clipCount <= 0) return; // Notify Player via event
        _clipCount--;
        _leftInClip = _clipCapacity;
        // Reload Audio
    }

    public void SetFireRate(FireRate rate)
    {
        if (_currentFireRate != null && _currentFireRate._fireRate == rate) return;

        if(_fireStates != null && _fireStates.TryGetValue(rate, out var frp))
        {
            _currentFireRate = frp;
        }
    }


    public override void UnEquip()
    {
        EndAutoFire();
        base.UnEquip();
    }

    public void StartAutoFire()
    {
        if (!CanAutoFire()) return;
        NextTick = Time.time + _currentFireRate.GetNextInterval();
        AutoFiring = true;
    }

    public void EndAutoFire()
    {
        if (!AutoFiring) return;
        AutoFiring = false;
    }


    private void Update()
    {
        if (!AutoFiring) return;

       
        if (Time.time < NextTick) return;


        NextTick += _currentFireRate.GetNextInterval();
    }

    private bool CanAutoFire()
    {
        if (!Equipped || _currentFireRate == null 
            || _currentFireRate._fireRate == FireRate.Single) return false;

        return true;
    }

    
}
