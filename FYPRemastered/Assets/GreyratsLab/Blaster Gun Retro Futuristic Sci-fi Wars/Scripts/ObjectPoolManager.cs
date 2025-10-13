using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
// using System;

namespace Greyrat.StarBlaster
{
    [System.Serializable]
    public class PooledObjectTimer
    {
        public PooledRigidbodyObject obj;
        public float timeRemaining;
        
        public PooledObjectTimer(PooledRigidbodyObject pooledObject, float lifetime)
        {
            obj = pooledObject;
            timeRemaining = lifetime;
        }
    }

    [System.Serializable]
    public class ParticlePooledObjectTimer
    {
        public ParticlePooledObject obj;
        public float timeRemaining;
        public Transform originalParent;
        
        public ParticlePooledObjectTimer(ParticlePooledObject pooledObject, float lifetime, Transform originalParent)
        {
            obj = pooledObject;
            timeRemaining = lifetime;
            this.originalParent = originalParent;
        }
    }

    public class ObjectPoolManager
    {
        public System.Action<Collision, PooledRigidbodyObject> OnBulletCollisionEvent;
        // Pool settings
        private readonly bool collectionChecks;
        private readonly int maxPoolSize;
        private readonly int defaultCapacity;
        private readonly Transform poolParent;

        // Object pools
        private IObjectPool<PooledRigidbodyObject> bulletPool;
        private IObjectPool<ParticlePooledObject> muzzleFlashPool;
        private IObjectPool<ParticlePooledObject> hitContactParticlePool;

        // Prefab references
        private readonly PooledRigidbodyObject bulletPrefab;
        private readonly ParticlePooledObject muzzleFlashPrefab;
        private readonly ParticlePooledObject hitContactParticlePrefab;

        // Settings
        private readonly float bulletMass;

        public ObjectPoolManager(
            PooledRigidbodyObject bulletPrefab,
            ParticlePooledObject muzzleFlashPrefab,
            ParticlePooledObject hitContactParticlePrefab,
            Transform poolParent,
            float bulletMass = 1f,
            bool collectionChecks = true,
            int maxPoolSize = 100,
            int defaultCapacity = 10)
        {
            this.bulletPrefab = bulletPrefab;
            this.muzzleFlashPrefab = muzzleFlashPrefab;
            this.hitContactParticlePrefab = hitContactParticlePrefab;
            this.poolParent = poolParent;
            this.bulletMass = bulletMass;
            this.collectionChecks = collectionChecks;
            this.maxPoolSize = maxPoolSize;
            this.defaultCapacity = defaultCapacity;
        }

        // Pool properties
        public IObjectPool<PooledRigidbodyObject> BulletPool
        {
            get
            {
                if (bulletPool == null && bulletPrefab != null)
                    bulletPool = new ObjectPool<PooledRigidbodyObject>(
                        CreateBullet, 
                        OnTakeFromBulletPool, 
                        OnReturnToPool, 
                        OnDestroyPoolObject, 
                        collectionChecks, 
                        defaultCapacity, 
                        maxPoolSize);
                return bulletPool;
            }
        }

        public IObjectPool<ParticlePooledObject> MuzzleFlashPool
        {
            get
            {
                if (muzzleFlashPool == null && muzzleFlashPrefab != null)
                    muzzleFlashPool = new ObjectPool<ParticlePooledObject>(
                        CreateMuzzleFlash, 
                        OnTakeFromPoolParticle, 
                        OnReturnToPoolParticle, 
                        OnDestroyPoolObjectParticle, 
                        collectionChecks, 
                        defaultCapacity, 
                        maxPoolSize);
                return muzzleFlashPool;
            }
        }

        public IObjectPool<ParticlePooledObject> HitContactParticlePool
        {
            get
            {
                if (hitContactParticlePool == null && hitContactParticlePrefab != null)
                    hitContactParticlePool = new ObjectPool<ParticlePooledObject>(
                        CreateHitContactParticle, 
                        OnTakeFromPoolParticle, 
                        OnReturnToPoolParticle, 
                        OnDestroyPoolObjectParticle, 
                        collectionChecks, 
                        defaultCapacity, 
                        maxPoolSize);
                return hitContactParticlePool;
            }
        }

        // Pool object creation methods
        private PooledRigidbodyObject CreateBullet()
        {
            PooledRigidbodyObject bullet = Object.Instantiate(bulletPrefab, poolParent);
            bullet.OnCollisionEnterEvent += (collision, bullet) => OnBulletCollisionEvent?.Invoke(collision, bullet);
            bullet.pool = BulletPool;
            
            // Disable gravity on bullet creation so it flies straight
            if (bullet.rb != null)
            {
                bullet.rb.useGravity = false;
            }
            
            return bullet;
        }

        private ParticlePooledObject CreateMuzzleFlash()
        {
            return Object.Instantiate(muzzleFlashPrefab, poolParent);
        }

        private ParticlePooledObject CreateHitContactParticle()
        {
            return Object.Instantiate(hitContactParticlePrefab, poolParent);
        }

        // Pool callback methods
        private void OnTakeFromPool(PooledRigidbodyObject obj)
        {
            obj.gameObject.SetActive(true);
            // Reset rigidbody if it exists
            if (obj.rb != null)
            {
                obj.rb.linearVelocity = Vector3.zero;
                obj.rb.angularVelocity = Vector3.zero;
            }
        }

        private void OnTakeFromBulletPool(PooledRigidbodyObject obj)
        {
            obj.gameObject.SetActive(true);
            // Reset rigidbody if it exists
            if (obj.rb != null)
            {
                obj.rb.linearVelocity = Vector3.zero;
                obj.rb.angularVelocity = Vector3.zero;
                obj.rb.mass = bulletMass;
            }
        }

        private void OnReturnToPool(PooledRigidbodyObject obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(PooledRigidbodyObject obj)
        {
            Object.Destroy(obj.gameObject);
        }

        private void OnTakeFromPoolParticle(ParticlePooledObject obj)
        {
            obj.gameObject.SetActive(true);
        }

        private void OnReturnToPoolParticle(ParticlePooledObject obj)
        {
            // Restore original parent before deactivating
            obj.transform.SetParent(poolParent);
            obj.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObjectParticle(ParticlePooledObject obj)
        {
            Object.Destroy(obj.gameObject);
        }

        // Utility methods for managing active object timers
        public static void UpdateObjectTimers<T>(LinkedList<T> activeObjects,
        System.Func<T, bool> shouldRemove, System.Action<T> onRemove)
        {
            var objectsToRemove = new List<T>();
            
            foreach (var obj in activeObjects)
            {
                if (shouldRemove(obj))
                {
                    onRemove?.Invoke(obj);
                    objectsToRemove.Add(obj);
                }
            }
            
            foreach (var obj in objectsToRemove)
            {
                activeObjects.Remove(obj);
            }
        }
    }
} 