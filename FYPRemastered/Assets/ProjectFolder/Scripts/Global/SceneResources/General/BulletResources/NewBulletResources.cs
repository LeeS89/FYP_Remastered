using Oculus.Interaction.GrabAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class NewBulletResources : SceneResources, IUpdateableResource
{
    private int _maxTrackedPoolObjects = 256;
    private List<PoolObjectTracker> _jobs;

    private GameObject _normalBulletPrefab;
    private GameObject _normalHitPrefab;
    private GameObject _fireParticleGO;
    public GameObject DeflectAudioSourcePrefabGO { get; private set; }



    private PoolManager<GameObject> _bulletPool;
    private PoolManager<AudioSource> _deflectAudioPool;
    private PoolManager<ParticleSystem> _hitParticlePool;
    private PoolManager<ParticleSystem> _fireParticlePool;

    //NEW
    static bool _catalogReady = false;
   // static readonly List<PoolAddressSO> _poolAddresses = new();
    static readonly Dictionary<string, PoolAddressSO> _addrToSO = new();
    AsyncOperationHandle<IList<PoolAddressSO>> _poolAddressHandle = new();
    readonly Dictionary<PoolIdSO, PoolManagerBase> _pools = new();
    readonly Dictionary<PoolIdSO, AsyncOperationHandle<GameObject>> _handles = new();
    //END NEW

    public async Task EnsureCatalogAsync()
    {
        if (_catalogReady) return;

        _addrToSO.Clear();

        _poolAddressHandle = Addressables.LoadAssetsAsync<PoolAddressSO>(
            "MoonScene", 
            so =>
            {
                if (!string.IsNullOrWhiteSpace(so.Key))
                    _addrToSO[so.Key] = so;
            }
        );
        await _poolAddressHandle.Task;

        _catalogReady = true;
    }

    public override async Task LoadResources(/*string sceneName*/)
    {
        try
        {

            await EnsureCatalogAsync();

            //string sceneLabel = $"scene:{sceneName}";

            // Get all resource locations with the specified label
            var locationHandle = Addressables.LoadResourceLocationsAsync("MoonScene", typeof(GameObject));
            var locations = await locationHandle.Task;

            if (locations == null || locations.Count == 0) return;

            // Filter locations to only those that have a corresponding PoolAddressSO
            var locationsToLoad = new List<IResourceLocation>(locations.Count);
            var configsToLoad = new List<PoolAddressSO>(locations.Count);

            foreach (var loc in locations)
            {
                if (_addrToSO.TryGetValue(loc.PrimaryKey, out var config))
                {
                    locationsToLoad.Add(loc);
                    configsToLoad.Add(config);
                }
            }

            if (locationsToLoad.Count == 0) return;
            // Load all assets in parallel
            var handles = new List<AsyncOperationHandle<GameObject>>(locationsToLoad.Count);
            for (int i = 0; i < locationsToLoad.Count; i++)
            {
                var h = Addressables.LoadAssetAsync<GameObject>(locationsToLoad[i]);
                handles.Add(h);
                await h.Task;

            }
         //   await Task.WhenAll(handles.Select(h => h.Task));
           
            for (int i = 0; i < handles.Count; i++)
            {
                var h = handles[i];
                var config = configsToLoad[i];

                if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null) continue;
              
                PoolManagerBase pool = config.Kind switch
                {
                    PoolKind.ParticleSystem => new PoolManager<ParticleSystem>(this, h.Result.GetComponent<ParticleSystem>()),
                    PoolKind.Audio => new PoolManager<AudioSource>(this, h.Result.GetComponent<AudioSource>()),
                    _ => new PoolManager<GameObject>(this, h.Result)
                };
              
                _pools[config.Id] = pool;
                _handles[config.Id] = h;
                
               // pool.PreWarmPool(config.PrewarmSize);
            }

            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;

            SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
            CoroutineRunner.Instance.StartCoroutine(PoolLoad());
        }
        catch (NullReferenceException e)
        {
            Debug.LogError($"Error loading resources: {e.Message}");
        }
    }

    private IEnumerator PoolLoad()
    {
        yield return new WaitForSeconds(3f);
        foreach(var pool in _pools)
        {
            pool.Value.PreWarmPool(10);
        }
    }



    protected override void ResourceRequested(in ResourceRequests request)
    {
        if (request.PoolId == null) return;
        var id = request.PoolId;    

        if (!_pools.TryGetValue(id, out var pool))
        {
#if UNITY_EDITOR
            throw new KeyNotFoundException($"Pool with ID {id?.name} not found.");
#endif
        }
        request.PoolCallback?.Invoke(id, pool);
        /*IPoolManager pool = request.PoolType switch
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
*/


    }




   


    public bool SchedulePoolObjectRelease(IPoolManager pool, UnityEngine.Object item, float seconds)
    {
        if (_jobs.Count == _maxTrackedPoolObjects) { return false; }
        _jobs.Add(new PoolObjectTracker(pool, item, seconds));
        return true;
    }

    public void UpdateResource()
    {
        if (_jobs == null || _jobs.Count == 0) { return; }

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
