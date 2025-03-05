using UnityEngine;

public class TestBulletFire : MonoBehaviour
{
    public BulletManager bulletManager;
    public float bulletSpeed = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Replace with actual shoot trigger
        {
            /*FindFirstObjectByType<BulletManager>().ShootBullet(
            transform.position,
            transform.forward,
            bulletSpeed
            );*/
        }
    }
}
