using System;
using UnityEngine;


public class AgentRangedWeapon : RangedWeaponBase
{
    private EnemyEventManager _eventManager;
    
    protected Transform _target;
   

    public AgentRangedWeapon(PoolIdSO poolId, Transform bulletSpawnPoint, Transform target, EnemyEventManager eventManager, GameObject gunOwner, int clipCapacity, Action _cb)
    {
        _eventManager = eventManager;
        _normalBulletPoolId = poolId;
        _bulletSpawnPoint = bulletSpawnPoint;
        _target = target;
        _gunOwner = gunOwner;
        _clipCapacity = clipCapacity;
        _clipCount = _clipCapacity;
        Callback = _cb;
        PoolRequestCallback = SetBulletPool;
    }

  
    public override void Reload()
    {
        _clipCount = _clipCapacity;
    }

   

    public override void UpdateWeapon()
    {
        // For updating coroutine conditions
    }

    protected override void NotifyReload()
    {
        //Debug.LogError("Reloading weapon");
        _eventManager.NotifyReload();
       // _eventManager.AnimationTriggered(AnimationAction.Reload);
    }

    public override void OnInstanceDestroyed()
    {
        _eventManager = null;
        PoolRequestCallback = null;
        _target = null;
        base.OnInstanceDestroyed();
    }
}
