using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BulletResources : SceneResources, IUpdateableResource
{
    private int _maxTrackedPoolObjects = 256;
    private List<PoolObjectTracker> _jobs;

    private GameObject _normalBulletPrefab;
    private GameObject _normalHitPrefab;
    public GameObject DeflectAudioSourcePrefabGO { get; private set; }

  

    private PoolManager<GameObject> _bulletPool;
    private PoolManager<AudioSource> _deflectAudioPool;
    private PoolManager<ParticleSystem> _hitParticlePool;


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

           // SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
            SceneEventAggregator.Instance.OnResourceRequested += ResourcesRequested;

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
                DeflectAudioSourcePrefabGO = audioHandle.Result;
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
        _jobs = new List<PoolObjectTracker>();
        _jobs.EnsureCapacity(_maxTrackedPoolObjects);

        if (_normalHitPrefab != null)
        {
            //_hitParticlePool = this.CreatePool<ParticleSystem>(_normalHitPrefab.GetComponent<ParticleSystem>());
            _hitParticlePool = new PoolManager<ParticleSystem>(this, _normalHitPrefab.GetComponent<ParticleSystem>());
            _hitParticlePool.PreWarmPool(40);
            /*_hitParticlePool = new PoolManager(_normalHitPrefab, 40, 80);
            _hitParticlePool.PrewarmPool(5);*/
        }

        if (DeflectAudioSourcePrefabGO != null)
        {
            //_deflectAudioPool = this.CreatePool<AudioSource>(DeflectAudioSourcePrefabGO.GetComponent<AudioSource>());
            _deflectAudioPool = new PoolManager<AudioSource>(this, DeflectAudioSourcePrefabGO.GetComponent<AudioSource>());
            _deflectAudioPool.PreWarmPool(20);
            /*_deflectAudioPool = new PoolManager(DeflectAudioSourcePrefabGO, 5, 10);
            _deflectAudioPool.PrewarmPool(5);*/
        }

        if (_normalBulletPrefab != null)
        {
            //_bulletPool = this.CreatePool<GameObject>(_normalBulletPrefab);
            _bulletPool = new PoolManager<GameObject>(this, _normalBulletPrefab);
            //_bulletPool = new PoolManager(_normalBulletPrefab, 40, 80);
            _bulletPool.PreWarmPool(30);
        }

       
    }

    protected override void ResourcesRequested(in ResourceRequests request)
    {

        if (request.PoolType == PoolResourceType.None) return;

        IPoolManager pool = request.PoolType switch
        {
            PoolResourceType.NormalBulletPool => _bulletPool,
            PoolResourceType.BasicHitParticlePool => _hitParticlePool,
            PoolResourceType.DeflectAudioPool => _deflectAudioPool,
#if UNITY_EDITOR
            _ => throw new ArgumentOutOfRangeException(nameof(request.PoolType), request.PoolType, null)
#else
            _ => null
#endif
        };

        request.PoolCallback?.Invoke(request.PoolType, pool);

       /* switch (request.PoolType)
        {
            case PoolResourceType.NormalBulletPool:
                pool = _bulletPool;
                break;
            case PoolResourceType.BasicHitParticlePool:
                pool = _hitParticlePool;
                break;
            case PoolResourceType.DeflectAudioPool:
                pool = _deflectAudioPool;
                break;
            default:
#if UNITY_EDITOR
                Debug.LogWarning("Invalid resource type requested.");
#endif
                break;
        }*/
        
    }

    [Obsolete]
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

    public bool SchedulePoolObjectRelease(IPoolManager pool, UnityEngine.Object item, float seconds)
    {
        if(_jobs.Count == _maxTrackedPoolObjects) { return false; }
        _jobs.Add(new PoolObjectTracker(pool, item, seconds));
        return true;
    }

    public void UpdateResource()
    {
        if(_jobs == null || _jobs.Count == 0) { return; }

        float dt = Time.deltaTime;

        for (int i = _jobs.Count - 1; i >= 0; i--)
        {
            var job = _jobs[i];
            job.TimeRemaining -= dt;
            _jobs[i] = job;
           
            if (job.TimeRemaining <= 0f)
            {
                job.Pool.Release(job.Item);

                int last = _jobs.Count - 1;
                if (i != last)
                {
                    _jobs[i] = _jobs[last];
                }
                _jobs.RemoveAt(last);
            }

        }
    }
}
