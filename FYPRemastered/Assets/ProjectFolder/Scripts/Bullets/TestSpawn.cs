using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    public GameObject _bullet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //InvokeRepeating("FireBullet", 3f, 3f);
    }

    private void FireBullet()
    {
        GameObject obj = Instantiate(_bullet, transform.position, transform.rotation);

        //Bullet bullet = obj.GetComponentInChildren<Bullet>();
        //bullet.Initializebullet();
    }
}
