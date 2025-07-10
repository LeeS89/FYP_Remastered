using System;
using System.Collections;
using TMPro;
using UnityEngine;


public class Gun
{
    [Header("Bullet Params")]
    private Transform _bulletSpawnPoint;
    private PoolManager _poolManager;
    private Transform _target;
    
    private EnemyEventManager _enemyEventManager;
    

    [Header("Firing Sequence Conditions")]
    private WaitUntil _waitUntilAimIsReady;
    private Func<bool> _hasAimAnimationCompleted;
    private WaitUntil _waitUntilFinishedReloading;
    private Func<bool> _reloadComplete;
    private bool _targetSeen = false;
    private bool _isShooting = false;
    private bool _isAimReady = false;
    private bool _playerHasDied = false;
    private bool _reloading = false;
    private int _ammo = 5;
    private GameObject _owner;
    private bool _isFacingTarget = false;

    private float _shootInterval = 2f;
    private float _nextShootTime = 0f;

    private ResourceRequest _request;

    #region Constructors
    public Gun(EventManager eventManager, GameObject gunOwner)
    {

        if(eventManager is EnemyEventManager enemyEventManager)
        {
            _enemyEventManager = enemyEventManager;
            _owner = gunOwner;
            _hasAimAnimationCompleted = IsAimReady;
            _waitUntilAimIsReady = new WaitUntil(_hasAimAnimationCompleted);
            _reloadComplete = IsReloading;
            _waitUntilFinishedReloading = new WaitUntil(_reloadComplete);

            _enemyEventManager.OnShoot += SpawnBullet;
            _enemyEventManager.OnPlayerSeen += UpdateTargetVisibility;
            _enemyEventManager.OnAimingLayerReady += SetAimReady;
            _enemyEventManager.OnFacingTarget += SetIsFacingTarget;


            _request = new ResourceRequest();
            _enemyEventManager.OnReload += Reload;
            GameManager.OnPlayerDied += PlayerHasDied;
            GameManager.OnPlayerRespawn += PlayerHasRespawned;
           
        }
    }

    public Gun(Transform bulletSpawnPoint, EventManager eventManager, GameObject gunOwner) : this(eventManager, gunOwner)
    {
        _bulletSpawnPoint = bulletSpawnPoint;

        _request.ResourceType = PoolResourceType.NormalBulletPool;
        _request.poolRequestCallback = (pool) =>
        {
            _poolManager = pool;
            _request.ResourceType = PoolResourceType.None; // Reset resource type after assignment
        };
        SceneEventAggregator.Instance.RequestResource(_request);
        //BaseSceneManager._instance.GetBulletPool(ref _poolManager);
    }

    public Gun(Transform bulletSpawnPoint, Transform target, EventManager eventManager, GameObject gunOwner) : this(bulletSpawnPoint, eventManager, gunOwner)
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

    #endregion

    #region Bool Setters And Getters
    private void PlayerHasDied()
    {
        _playerHasDied = true;
    }

    private void PlayerHasRespawned()
    {
        _playerHasDied = false;
    }

    private void SetAimReady(bool isReady)
    {
        _isAimReady = isReady;
    }

    private bool IsAimReady()
    {
        return _isAimReady;
    }

    private void Reload(bool isReloading)
    {
        _reloading = isReloading;
    }

   

    private bool IsReloading()
    {
        return !_reloading;
    }

    private void SetIsFacingTarget(bool isFacingTarget)
    {
        _isFacingTarget = isFacingTarget;
    }
    #endregion


    #region Shooting Region
    public void UpdateTargetVisibility(bool targetSeen)
    {
        if(_targetSeen == targetSeen) { return; }

        _targetSeen = targetSeen;

        /*if (_targetSeen && !_isShooting)
        {
            _isShooting = true;
            CoroutineRunner.Instance.StartCoroutine(FiringSequence());
        }
        else
        {
            _isShooting = false;
            CoroutineRunner.Instance.StopCoroutine(FiringSequence());
        }*/
    }

    private IEnumerator FiringSequence()
    {
        while (_isShooting)
        {
            //if (!IsAimReady())
            //{
            yield return _waitUntilAimIsReady;
            //}

            // if (IsReloading())
            // {
            yield return _waitUntilFinishedReloading;
            // }

            if (!_playerHasDied)
            {
                Shoot();
            }

            yield return new WaitForSeconds(2f);
        }
    }

    public void UpdateGun()
    {
        if (!_targetSeen) { return; }

        if (Time.time >= _nextShootTime)
        {
            if (!_playerHasDied && _isAimReady && !_reloading && _isFacingTarget)
            {
                Shoot();
            }
            _nextShootTime = Time.time + _shootInterval;
        }
    }

    private void Shoot()
    {
        if (!_isFacingTarget) { return; }

        if (_ammo-- > 0)
        {
            _enemyEventManager.AnimationTriggered(AnimationAction.Shoot);
        }
        else
        {

            _ammo = 5;

            _enemyEventManager.AnimationTriggered(AnimationAction.Reload);
        }
    }

    /// <summary>
    /// Called From Animation Event
    /// </summary>
    private void SpawnBullet()
    {
       
        Vector3 _directionToTarget = TargetingUtility.GetDirectionToTarget(_target, _bulletSpawnPoint, true);
        Quaternion bulletRotation = Quaternion.LookRotation(_directionToTarget);

        GameObject obj = _poolManager.GetFromPool(_bulletSpawnPoint.position, bulletRotation);

        BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
        //bullet.Owner = _owner;
        //obj.SetActive(true);
        bullet.InitializeBullet(_owner);

    }

    #endregion

    public void OnInstanceDestroyed()
    {
        _isAimReady = false;
        _isShooting = false;
        CoroutineRunner.Instance.StopCoroutine(FiringSequence());
        _enemyEventManager.OnShoot -= SpawnBullet;
        _enemyEventManager.OnPlayerSeen -= UpdateTargetVisibility;
        _enemyEventManager.OnAimingLayerReady -= SetAimReady;
        _enemyEventManager.OnFacingTarget -= SetIsFacingTarget;

        _enemyEventManager.OnReload -= Reload;
        GameManager.OnPlayerDied -= PlayerHasDied;
        GameManager.OnPlayerRespawn -= PlayerHasRespawned;
        _poolManager = null;
        _enemyEventManager = null;
        _hasAimAnimationCompleted = null;
        _waitUntilAimIsReady = null;
        _reloadComplete = null;
        _waitUntilFinishedReloading = null;

    }
}
