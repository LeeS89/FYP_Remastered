using System;
using System.Collections;
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
    private bool _targetSeen = false;
    private bool _isShooting = false;
    private bool _isAimReady = false;
    private bool _playerHasDied = false;

    #region Constructors
    public Gun(EventManager eventManager)
    {

        if(eventManager is EnemyEventManager enemyEventManager)
        {
            _enemyEventManager = enemyEventManager;
            _hasAimAnimationCompleted = IsAimReady;
            _waitUntilAimIsReady = new WaitUntil(_hasAimAnimationCompleted);

            _enemyEventManager.OnShoot += Shoot;
            _enemyEventManager.OnPlayerSeen += UpdateTargetVisibility;
            _enemyEventManager.OnAimingLayerReady += SetAimReady;
            GameManager.OnPlayerDied += PlayerHasDied;
            GameManager.OnPlayerRespawn += PlayerHasRespawned;
           
        }
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
    #endregion


    #region Shooting Region
    public void UpdateTargetVisibility(bool targetSeen)
    {
        if(_targetSeen == targetSeen) { return; }

        _targetSeen = targetSeen;

        if (_targetSeen && !_isShooting)
        {
            _isShooting = true;
            CoroutineRunner.Instance.StartCoroutine(FiringSequence());
        }
        else
        {
            _isShooting = false;
            CoroutineRunner.Instance.StopCoroutine(FiringSequence());
        }
    }

    private IEnumerator FiringSequence()
    {
        while (_isShooting)
        {
            if (!IsAimReady())
            {
                yield return _waitUntilAimIsReady;
            }

            _enemyEventManager.AnimationTriggered(AnimationAction.Shoot);

            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Called From Animation Event
    /// </summary>
    private void Shoot()
    {
        if (_playerHasDied) { return; }

        Vector3 _directionToTarget = TargetingUtility.GetDirectionToTarget(_target, _bulletSpawnPoint, true);
        Quaternion bulletRotation = Quaternion.LookRotation(_directionToTarget);

        GameObject obj = _poolManager.GetGameObject(_bulletSpawnPoint.position, bulletRotation);

        BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
        //bullet.Owner = transform.parent.gameObject;
        obj.SetActive(true);
        bullet.Initializebullet();
    }

    #endregion

    public void OnInstanceDestroyed()
    {
        _isAimReady = false;
        _isShooting = false;
        CoroutineRunner.Instance.StopCoroutine(FiringSequence());
        _enemyEventManager.OnShoot -= Shoot;
        _enemyEventManager.OnPlayerSeen -= UpdateTargetVisibility;
        _enemyEventManager.OnAimingLayerReady -= SetAimReady;
        GameManager.OnPlayerDied -= PlayerHasDied;
        GameManager.OnPlayerRespawn -= PlayerHasRespawned;
        _poolManager = null;
        _enemyEventManager = null;
        _hasAimAnimationCompleted = null;
        _waitUntilAimIsReady = null;
        
    }
}
