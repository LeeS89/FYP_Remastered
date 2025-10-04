using System;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Weapon, IRanged
{
    [SerializeField] protected PoolIdSO poolId;
    protected IPoolManager _pool;
    [SerializeField] protected Transform _spawnPoint;
    [SerializeField] protected FireRate _fireRate = FireRate.Single;
    protected Action<string, IPoolManager> PoolRequestCallback;

    [SerializeField] protected int _clipCapacity;
    [SerializeField] protected int _clipCount;
    protected int _leftInClip;
    protected Transform _target; // Used by NPC's to fire in direction of player

    [SerializeField] protected List<FireRateParams> _params;
    protected Dictionary<FireRate, FireRateParams> _fireStates = new(3);
    protected FireRateParams _currentFireRate;

    private void Start()
    {
        if(poolId != null && !string.IsNullOrEmpty(poolId.Id))
        {
            PoolRequestCallback = OnPoolReceived;
            this.RequestPool(poolId.Id, PoolRequestCallback);
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

    protected void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || poolId != this.poolId.Id || pool == null) return;
        Debug.LogError("Pool Request Completed");
        _pool = pool;
    }

    public override void Equip(EventManager eventManager, GameObject owner)
    {
        base.Equip(eventManager, owner);
    }

    public virtual void Fire(Transform target = null)
    {
        if(_leftInClip > 0)
        {
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
        }
        else
        {
            NotifyReload();
        }
    }

    public virtual void NotifyReload()
    {
        // For Player => Maybe some text, SFX, voice over etc
        // For NPC => Trigger Reload animation in derived class
    }

    public virtual void OnFireInterupted()
    {
        
    }

    public virtual void Reload()
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
       base.UnEquip();
    }

    public virtual void StartAutoFire() { }
    

    private void Update()
    {
        if (!Equipped || _fireRate == FireRate.Single) return;
    }

  
    /*public enum FireRate
    {
        Single,
        SingleAutomatic,
        Burst,
        FullAutomatic
    }*/
}
