using UnityEngine;
using System.Collections.Generic;

namespace Greyrat.StarBlaster
{
    public class BlasterController : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private PooledRigidbodyObject bulletPrefab;
        [SerializeField] private ParticlePooledObject muzzleFlashPrefab;
        [SerializeField] private ParticlePooledObject hitContactParticlePrefab;

        [Header("Location References")]
        [SerializeField] private Animator blasterAnimator;
        [SerializeField] private Transform barrelBulletSpawnLocation;
        [SerializeField] private Transform barrelMuzzleFlashSpawnLocation;

        [Header("Shooting Settings")]
        [Tooltip("Bullet Speed")][SerializeField] private float shotPower = 500f;
        [Tooltip("Bullet Mass")][SerializeField] private float bulletMass = 1f;
        [SerializeField] private bool destroyBulletOnCollision = true;
        [SerializeField] private bool enableMouseInput = false;
        [Tooltip("Direction to launch the bullet")][SerializeField] private LaunchDirection launchDirection = LaunchDirection.Forward;

        [Header("Lifetime Settings")]
        [Tooltip("Muzzle flash lifetime before returning to pool")]
        [SerializeField] private float flashLifetime = 2f;
        [Tooltip("Bullet lifetime before returning to pool")]
        [SerializeField] private float bulletLifetime = 10f;
        [Tooltip("Hit contact particle lifetime before returning to pool")]
        [SerializeField] private float hitContactParticleLifetime = 3f;

        [Header("Pool Settings")]
        [SerializeField] private bool collectionChecks = true;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private int defaultCapacity = 10;

        // Pool manager and active object tracking
        private ObjectPoolManager poolManager;
        private LinkedList<PooledObjectTimer> activeBullets = new LinkedList<PooledObjectTimer>();
        private LinkedList<ParticlePooledObjectTimer> activeMuzzleFlashes = new LinkedList<ParticlePooledObjectTimer>();
        private LinkedList<ParticlePooledObjectTimer> activeHitContactParticles = new LinkedList<ParticlePooledObjectTimer>();

        private Transform poolParent;

        void Start()
        {
            InitializeComponents();
            InitializePoolManager();
        }

        void Update()
        {
            HandleInput();
            UpdateActiveObject();
        }

        private void InitializeComponents()
        {
            // Create pool parent
            poolParent = new GameObject("RUNTIME_BlasterPool").transform;

            // Set default references if not assigned
            if (barrelBulletSpawnLocation == null)
                barrelBulletSpawnLocation = transform;

            if (blasterAnimator == null)
                blasterAnimator = GetComponent<Animator>();
        }

        private void InitializePoolManager()
        {
            poolManager = new ObjectPoolManager(
                bulletPrefab,
                muzzleFlashPrefab,
                hitContactParticlePrefab,
                poolParent,
                bulletMass,
                collectionChecks,
                maxPoolSize,
                defaultCapacity
            );
            poolManager.OnBulletCollisionEvent += OnBulletCollision;
        }

        private void HandleInput()
        {
            if (enableMouseInput && Input.GetMouseButtonDown(0))
            {
                Fire();
            }
        }

        private void UpdateActiveObject()
        {
            // Update bullet timers
            ObjectPoolManager.UpdateObjectTimers(
                activeBullets,
                timer =>
                {
                    timer.timeRemaining -= Time.deltaTime;
                    return timer.timeRemaining <= 0 || timer.obj == null || !timer.obj.gameObject.activeInHierarchy;
                },
                timer =>
                {
                    if (timer.obj != null && timer.obj.gameObject.activeInHierarchy)
                    {
                        poolManager.BulletPool.Release(timer.obj);
                    }
                }
            );

            // Update muzzle flash timers
            ObjectPoolManager.UpdateObjectTimers(
                activeMuzzleFlashes,
                timer =>
                {
                    timer.timeRemaining -= Time.deltaTime;
                    return timer.timeRemaining <= 0 || timer.obj == null || !timer.obj.gameObject.activeInHierarchy;
                },
                timer =>
                {
                    if (timer.obj != null && timer.obj.gameObject.activeInHierarchy)
                    {
                        timer.obj.transform.SetParent(timer.originalParent);
                        poolManager.MuzzleFlashPool.Release(timer.obj);
                    }
                }
            );

            // Update hit contact particle timers
            ObjectPoolManager.UpdateObjectTimers(
                activeHitContactParticles,
                timer =>
                {
                    timer.timeRemaining -= Time.deltaTime;
                    return timer.timeRemaining <= 0 || timer.obj == null || !timer.obj.gameObject.activeInHierarchy;
                },
                timer =>
                {
                    if (timer.obj != null && timer.obj.gameObject.activeInHierarchy)
                    {
                        timer.obj.transform.SetParent(timer.originalParent);
                        poolManager.HitContactParticlePool.Release(timer.obj);
                    }
                }
            );
        }

        public void Fire()
        {
            if (blasterAnimator != null)
            {
                blasterAnimator.SetTrigger("Fire");
            }

            Shoot();
        }

        private void Shoot()
        {
            SpawnMuzzleFlash();
            SpawnBullet();
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzleFlashPrefab == null) return;

            ParticlePooledObject flash = poolManager.MuzzleFlashPool.Get();
            flash.transform.position = barrelMuzzleFlashSpawnLocation.position;
            flash.transform.rotation = barrelMuzzleFlashSpawnLocation.rotation;

            flash.GetComponent<ParticleSystem>().Play();

            activeMuzzleFlashes.AddLast(new ParticlePooledObjectTimer(flash, flashLifetime, this.transform));
        }

        private void SpawnBullet()
        {
            if (bulletPrefab == null) return;

            PooledRigidbodyObject bullet = poolManager.BulletPool.Get();
            bullet.transform.position = barrelBulletSpawnLocation.position;
            bullet.transform.rotation = barrelBulletSpawnLocation.rotation;

            Vector3 launchDir = GetLaunchDirection();
            bullet.rb.AddForce(launchDir * shotPower, ForceMode.Impulse);
            bullet.rb.useGravity = false;

            activeBullets.AddLast(new PooledObjectTimer(bullet, bulletLifetime));
        }

        private Vector3 GetLaunchDirection()
        {
            switch (launchDirection)
            {
                case LaunchDirection.Forward:
                    return barrelBulletSpawnLocation.forward;
                case LaunchDirection.Backward:
                    return -barrelBulletSpawnLocation.forward;
                case LaunchDirection.Up:
                    return barrelBulletSpawnLocation.up;
                case LaunchDirection.Down:
                    return -barrelBulletSpawnLocation.up;
                case LaunchDirection.Left:
                    return -barrelBulletSpawnLocation.right;
                case LaunchDirection.Right:
                    return barrelBulletSpawnLocation.right;
                default:
                    return barrelBulletSpawnLocation.forward;
            }
        }

        private void OnBulletCollision(Collision collision, PooledRigidbodyObject bullet)
        {
            if(collision.gameObject.tag == "Bullet")
                return;

            // Enable gravity after bullet hits something
            if (bullet.rb != null)
            {
                bullet.rb.useGravity = true;
            }

            // Spawn hit contact particle
            SpawnHitContactParticle(collision);

            // Handle bullet destruction based on settings
            if (destroyBulletOnCollision)
            {
                // Remove from active tracking
                RemoveBulletFromActiveList(bullet);

                // Return bullet to pool
                if (bullet.pool != null)
                {
                    bullet.pool.Release(bullet);
                }
                else
                {
                    bullet.ReturnToPool();
                }
            }
        }

        private void SpawnHitContactParticle(Collision collision)
        {
            if (hitContactParticlePrefab == null) return;

            ParticlePooledObject hitParticle = poolManager.HitContactParticlePool.Get();
            hitParticle.transform.position = collision.contacts[0].point;
            hitParticle.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);

            // Set the hit object as parent so particle follows its transform
            hitParticle.transform.SetParent(collision.transform);

            // Play the particle system
            if (hitParticle.GetComponent<ParticleSystem>() != null)
            {
                hitParticle.GetComponent<ParticleSystem>().Play();
            }

            // Add to active tracking
            activeHitContactParticles.AddLast(new ParticlePooledObjectTimer(hitParticle, hitContactParticleLifetime, this.transform));
        }

        private void RemoveBulletFromActiveList(PooledRigidbodyObject bullet)
        {
            var node = activeBullets.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.obj == bullet)
                {
                    activeBullets.Remove(node);
                    break;
                }
                node = next;
            }
        }

        // Public methods that can be called from animation events or external scripts
        public void OnFireAnimationEvent()
        {
            // Called from animation event during firing animation
            Shoot();
        }

        private void OnDestroy()
        {
            // Clean up pool parent
            if (poolParent != null)
            {
                Destroy(poolParent.gameObject);
            }
        }
    }

    public enum LaunchDirection
    {
        Forward,
        Backward,
        Up,
        Down,
        Left,
        Right,
    }
}