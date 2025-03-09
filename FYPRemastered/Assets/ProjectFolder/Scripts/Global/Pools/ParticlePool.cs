using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public static class ParticlePool
{
    private static ObjectPool<ParticleSystem> _hitParticlePool;
    private static ParticleSystem _hitParticlePrefab;
    private static GameObject _poolContainer;
    private static MonoBehaviour _caller;

    public static void Initialize(ParticleSystem prefab, MonoBehaviour caller)
    {
        if (_hitParticlePool != null) { return; }

        _caller = caller;
        _hitParticlePrefab = prefab;
        _poolContainer = new GameObject("HitParticlePoolContainer");

        _hitParticlePool = new ObjectPool<ParticleSystem>(
            createFunc: () =>
            {
                var particle = Object.Instantiate(_hitParticlePrefab);
                particle.gameObject.SetActive(false);
                particle.transform.parent = _poolContainer.transform;
                return particle;
            },
            actionOnGet: (particle) =>
            {
                particle.gameObject.SetActive(true);
                particle.Play();
            },
            actionOnRelease: (particle) =>
            {
                particle.Stop();
                particle.gameObject.SetActive(false);
            },
            actionOnDestroy: (particle) => Object.Destroy(particle.gameObject),
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 35
            );
    }

    public static void SpawnHitParticle(Vector3 position, Quaternion rotation)
    {
        if(_hitParticlePool == null)
        {
            Debug.LogError("HitParticlePool is not initialized: Call Initialize(prefab) first.");
            return;
        }

        ParticleSystem particle = _hitParticlePool.Get();
        particle.transform.position = position;
        particle.transform.rotation = rotation;

        _caller.StartCoroutine(ReturnToPool(particle, particle.main.duration));
        
    }

    private static IEnumerator ReturnToPool(ParticleSystem particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        _hitParticlePool.Release(particle);
    }
}
