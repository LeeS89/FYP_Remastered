using UnityEngine;

public class TestSpawn : EventManager
{
    //public GameObject _bullet;
    public Transform _player;
    //public BaseSceneManager _sceneManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //_poolManager = new PoolManager(_bullet,1,2);
        //_poolManager.PrewarmPool(2);
        //BaseSceneManager._instance.GetBulletPool(ref _poolManager);
        InvokeRepeating(nameof(FireBullet), 3f, 0.5f);
    }

    private void FireBullet()
    {
        if(_poolManager == null)
        {
            BaseSceneManager._instance.GetBulletPool(ref _poolManager);
        }

        Vector3 _directionToPlayer = TargetingUtility.GetDirectionToTarget(_player, transform, true);
        Quaternion bulletRotation = Quaternion.LookRotation(_directionToPlayer);

        GameObject obj = _poolManager.GetGameObject(transform.position, bulletRotation);
       
        BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
        bullet.Owner = gameObject;
        obj.SetActive(true);
        bullet.Initializebullet();
        
    }

  /*  public void HitParticlePoolInjection(PoolManager poolManager)
    {
        
        _poolManager = poolManager;
    }*/

    public override void BindComponentsToEvents()
    {
        return;
    }

    public override void UnbindComponentsToEvents()
    {
        return;
    }
}
