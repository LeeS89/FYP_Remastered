using System;
using System.Collections;
using UnityEngine;

public class GunBase
{
    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////
    /// New GunBase Class
    /// </summary>
    ///  [Header("Bullet Params")]
    private Transform _bulletSpawnPoint;
    private PoolManager _bulletPool;
    private Transform _target;
    private int _clipCapacity;
    private int _clipCount;
    private EventManager _eventManager;

    private GameObject _owner;

    ///////////////////////////////////////////////////////// END




    //[Header("Firing Sequence Conditions")]
  /*  private WaitUntil _waitUntilCanFire;
    private WaitUntil _waitUntilAimIsReady;
    private Func<bool> _hasAimAnimationCompleted;
    private WaitUntil _waitUntilFinishedReloading;*/


   /* private WaitUntil _testWaitUntil;
    private bool _testBool = false;*/
  
    #region Constructors
  

    public GunBase(Transform bulletSpawnPoint, Transform target, EventManager eventManager, GameObject gunOwner, PoolManager bulletPool, int clipCapacity)
    {
        _bulletSpawnPoint = bulletSpawnPoint;
        _target = target;
        _eventManager = eventManager;
        _owner = gunOwner;
        _bulletPool = bulletPool;
        _clipCapacity = clipCapacity;
        _clipCount = clipCapacity;
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
  

    private void NotifyReload()
    {
        _eventManager.OutOfAmmo();
        //_isReloading = isReloading;
    }

    public void Reload()
    {
        _clipCount = _clipCapacity;
    }


    #endregion


    #region Shooting Region


    /*private IEnumerator FiringSequence()
    {
        if (_testBool)
        {
            Debug.LogWarning("GunCoroutine started again");
        }

        while (_isShooting)
        {

            // yield return _testWaitUntil;
            // yield return _waitUntilAimIsReady;

            yield return _waitUntilCanFire;
            // yield return _waitUntilFinishedReloading;

            if (!_playerHasDied)
            {
                Shoots();
            }

            yield return new WaitForSeconds(2f);
        }
    }*/


  

    /// <summary>
    /// Called From Animation Event
    /// </summary>
    public void Shoot()
    {
        if (_clipCount > 0)
        {
         
            _clipCount--;
            Vector3 _directionToTarget = TargetingUtility.GetDirectionToTarget(_target, _bulletSpawnPoint, true);
            Quaternion bulletRotation = Quaternion.LookRotation(_directionToTarget);

            GameObject obj = _bulletPool.GetFromPool(_bulletSpawnPoint.position, bulletRotation);

            BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
            //bullet.Owner = _owner;
            //obj.SetActive(true);
            bullet.InitializeBullet(_owner);
        }

        if(_clipCount == 0)
        {
            NotifyReload();
        }
    }

    #endregion

    public void OnInstanceDestroyed()
    {

        //CoroutineRunner.Instance.StopCoroutine(FiringSequence());
        _owner = null;
        _target = null;
        _bulletSpawnPoint = null;
        _bulletPool = null;
        _eventManager = null;
        /* _hasAimAnimationCompleted = null;
         _waitUntilAimIsReady = null;*/
        //_reloadComplete = null;


    }
}
