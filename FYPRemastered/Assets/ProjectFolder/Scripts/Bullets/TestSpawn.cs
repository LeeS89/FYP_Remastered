using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    public GameObject _bullet;
    public Transform _player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("FireBullet", 3f, 3f);
    }

    private void FireBullet()
    {
        Vector3 _directionToPlayer = TargetingUtility.GetDirectionToTarget(_player, transform);
        Quaternion bulletRotation = Quaternion.LookRotation(_directionToPlayer);

        GameObject obj = Instantiate(_bullet, transform.position, bulletRotation);

        Bullet bullet = obj.GetComponentInChildren<Bullet>();
        bullet.Owner = gameObject;
        bullet.Initializebullet();
    }
}
