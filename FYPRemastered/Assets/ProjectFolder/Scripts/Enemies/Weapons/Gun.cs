using UnityEngine;

public class Gun
{
    private Transform _bulletSpawnPoint;
    private PoolManager _poolManager;
    private Transform _target;
    private bool _targetSeen = false;
    private EventManager _eventManager;

    public Gun(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public Gun(Transform bulletSpawnPoint, EventManager eventManager) : this(eventManager)
    {
        _bulletSpawnPoint = bulletSpawnPoint;
        BaseSceneManager._instance.GetBulletPool(ref _poolManager);
    }

    public Gun(Transform bulletSpawnPoint, Transform target, EventManager eventManager) : this(bulletSpawnPoint, eventManager)
    {
        _target = target;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetBulletSpawnPoint(Transform spawnPoint)
    {
        _bulletSpawnPoint = spawnPoint;
    }

    public void UpdateTargetVisibility(bool targetSeen)
    {
        if(_targetSeen ==  targetSeen) { return; }
    }
}
