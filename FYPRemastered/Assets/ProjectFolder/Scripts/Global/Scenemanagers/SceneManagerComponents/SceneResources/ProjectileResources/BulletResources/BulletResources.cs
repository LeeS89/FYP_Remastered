using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BulletResources : SceneResources
{
    private GameObject _normalBulletPrefab;
    private GameObject _normalHitPrefab;
    private GameObject _deflectAudioPrefab;

    private PoolManager _bulletPool;
    private PoolManager _deflectAudioPool;
    private PoolManager _hitParticlePool;
    

    public override async Task LoadResources()
    {
        try
        {
            var normalBulletHandle = Addressables.LoadAssetAsync<GameObject>("BasicBullet");

            await normalBulletHandle.Task;

            if (normalBulletHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _normalBulletPrefab = normalBulletHandle.Result;
                if (_normalBulletPrefab == null)
                {
                    Debug.LogError("Loaded normal bullet prefab is null.");
                }

                await LoadResourceAudio();
                await LoadResourceParticles();

                InitializePools();
            }
            else
            {
                Debug.LogError("Failed to load the normal bullet prefab from Addressables.");
            }

            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
            SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        }
        catch(Exception e)
        {
            Debug.LogError($"Error loading bullet resources: {e.Message}");
        }
       
    }

    protected override async Task LoadResourceAudio()
    {
        try
        {
            var audioHandle = Addressables.LoadAssetAsync<GameObject>("DeflectAudio");

            await audioHandle.Task;

            if(audioHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _deflectAudioPrefab = audioHandle.Result;
            }

        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading bullet audio resources: {e.Message}");
        }

    }

    protected override async Task LoadResourceParticles()
    {
        try
        {
            var particleHandle = Addressables.LoadAssetAsync<GameObject>("BasicHit");
            
            await particleHandle.Task;

            if (particleHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _normalHitPrefab = particleHandle.Result;
                if (_normalHitPrefab == null)
                {
                    Debug.LogError("Loaded normal hit particle prefab is null.");
                }
            }
            else
            {
                Debug.LogError("Failed to load the normal hit particle prefab from Addressables.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading bullet particle resources: {e.Message}");
        }
    }

    protected override void InitializePools()
    {
        
        if (_normalHitPrefab != null)
        {
            _hitParticlePool = new PoolManager(_normalHitPrefab, 40, 80);
            //_hitParticlePool.PrewarmPool(PoolContents.Particle, 15);
        }

        if (_deflectAudioPrefab != null)
        {
            _deflectAudioPool = new PoolManager(_deflectAudioPrefab, 5, 10);
            _deflectAudioPool.PrewarmPool(5);
        }

        if (_normalBulletPrefab != null)
        {
            _bulletPool = new PoolManager(_normalBulletPrefab, 40, 80);
            //_bulletPool.PrewarmPool(PoolContents.Object, 40);
        }
    }

    protected override void ResourceRequested(ResourceRequest request)
    {
        if(request.ResourceType == PoolResourceType.None) { return; }

        switch (request.ResourceType)
        {
            case PoolResourceType.NormalBulletPool:
                request.poolRequestCallback?.Invoke(_bulletPool);
                break;
            case PoolResourceType.BasicHitParticlePool:
                request.poolRequestCallback?.Invoke(_hitParticlePool);
                break;
            case PoolResourceType.DeflectAudioPool:
                request.poolRequestCallback?.Invoke(_deflectAudioPool);
                break;
            default:
                Debug.LogWarning("Unknown resource type requested.");
                break;
        }
    }


}
