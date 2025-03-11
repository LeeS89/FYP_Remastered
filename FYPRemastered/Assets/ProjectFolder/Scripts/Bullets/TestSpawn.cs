using Meta.XR.MRUtilityKit.SceneDecorator;
using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    public GameObject _bullet;
    public Transform _player;
    //public BaseSceneManager _sceneManager;
    private PoolManager _poolManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _poolManager = new PoolManager(_bullet,1,2);
        _poolManager.PrewarmPool(2);
        InvokeRepeating(nameof(FireBullet), 3f, 0.5f);
    }

    private void FireBullet()
    {
        Vector3 _directionToPlayer = TargetingUtility.GetDirectionToTarget(_player, transform, true);
        Quaternion bulletRotation = Quaternion.LookRotation(_directionToPlayer);

        GameObject obj = _poolManager.GetGameObject(transform.position, bulletRotation);
       
        BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
        bullet.Owner = gameObject;
        obj.SetActive(true);
        bullet.Initializebullet();
        
    }
}
