using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Gun
{
    private Transform _bulletSpawnPoint;
    private PoolManager _poolManager;
    private Transform _target;
    private bool _targetSeen = false;
    private EnemyEventManager _enemyEventManager;
    private bool _isShooting = false;

    public Gun(EventManager eventManager)
    {
        //_enemyEventManager = eventManager;

        if(eventManager is EnemyEventManager enemyEventManager)
        {
            _enemyEventManager = enemyEventManager;
            _enemyEventManager.OnPlayerSeen += UpdateTargetVisibility;
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

    public void UpdateTargetVisibility(bool targetSeen)
    {
        if(_targetSeen == targetSeen) { return; }

        _targetSeen = targetSeen;

        if (_targetSeen)
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

            Vector3 _directionToPlayer = TargetingUtility.GetDirectionToTarget(_target, _bulletSpawnPoint, true);
            Quaternion bulletRotation = Quaternion.LookRotation(_directionToPlayer);

            GameObject obj = _poolManager.GetGameObject(_bulletSpawnPoint.position, bulletRotation);

            BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
            //bullet.Owner = transform.parent.gameObject;
            obj.SetActive(true);
            bullet.Initializebullet();
            _enemyEventManager.AnimationTriggered(AnimationAction.Shoot);

            yield return new WaitForSeconds(2f);
        }
    }
}
