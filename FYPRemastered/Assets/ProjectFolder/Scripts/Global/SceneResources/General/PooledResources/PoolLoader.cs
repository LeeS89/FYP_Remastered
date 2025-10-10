using Meta.XR.MRUtilityKit.SceneDecorator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PoolLoader : SceneResources, IUpdateableResource
{
    /// NEW LOADER
    private readonly Dictionary<string, PoolManagerBase> _cache = new();
    private readonly HashSet<string> _loading = new();
    private readonly Dictionary<string, List<ResourceRequests>> _waiters = new();
    private readonly Queue<ResourceRequests> _queue = new();
    private bool _isProcessing;
    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _assetHandles = new();
    private readonly HashSet<string> _poolsToLoad = new(10);

    /*public PoolLoader(List<PoolIdSO> poolIds)
    {
        if (poolIds == null || poolIds.Count == 0) return;

        foreach(var tag in poolIds)
        {
            _poolsToLoad.Add(tag.Id);
            if (tag._fxPools == null || tag._fxPools.Count == 0) continue;

            foreach (var fxp in tag._fxPools)
            {
                if (!string.IsNullOrEmpty(fxp.Id)) _poolsToLoad.Add(fxp.Id);
            }

           *//* string ap = tag._audioPoolId.Id;
            string pp = tag._particlePoolId.Id;
            if (!string.IsNullOrEmpty(ap)) _poolsToLoad.Add(ap);
            if(!string.IsNullOrEmpty(pp)) _poolsToLoad.Add(pp);*//*
        }
    }*/
    //ConcurrentDictionary
    //////// END NEW LOADER

    /// <summary>
    // CancellationToken
    /// </summary>
    // NEW FUNCTIONS
    public void RequestPool(in ResourceRequests req)
    {
        if(_cache.TryGetValue(req.PoolId.Id, out var pool))
        {
            req.PoolRequesterCallback?.Invoke(req.PoolId.Id, pool);
            return;
        }
        // If Requested pool has not already been loaded, queue request for loading
        _queue.Enqueue(req);
        if(!_isProcessing) _ = ProcessQueue();
    }

    private async Task ProcessQueue()
    {
        _isProcessing = true;
       
        while(_queue.Count > 0)
        {
            var req = _queue.Dequeue();

            // Checking if the requested pool has any dependent pools that also need to be loaded
            PoolIdSO pid = req.PoolId;

            if(pid._fxPools != null)
            {
                foreach(var poolId in pid._fxPools)
                {
                    if(!_cache.ContainsKey(poolId.Id))
                        await LoadPoolAsync(poolId, notifyWaiters: false);
                }
            }


            // Another request earlier in the queue may have loaded it already
            if (_cache.TryGetValue(req.PoolId.Id, out var pool))
            {
                req.PoolRequesterCallback?.Invoke(req.PoolId.Id, pool);
                continue;
            }

            // If it's already loading, add this requester to the waiters
            if (_loading.Contains(req.PoolId.Id))
            {
                _waiters[req.PoolId.Id].Add(req);
                continue;
            }

            _loading.Add(req.PoolId.Id);
            _waiters[req.PoolId.Id] = new List<ResourceRequests> { req };

            _= LoadPoolAsync(req.PoolId, notifyWaiters: true);

           
        }
        _isProcessing = false;
    }



    private async Task LoadPoolAsync(PoolIdSO poolId, bool notifyWaiters)
    {
        try
        {
            string address = poolId.Id;
            if (string.IsNullOrEmpty(address)) throw new Exception("Invalid address passed for pool");
            address = address.Trim();

            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var prefab = await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || prefab == null)
                throw new Exception($"Addressables load failed for '{poolId}' ({handle.Status}).");



            _assetHandles[address] = handle;
            PoolKind kind = poolId.Kind;

            PoolManagerBase pool = kind switch
            {
                PoolKind.ParticleSystem => new PoolManager<ParticleSystem>(this, prefab.GetComponent<ParticleSystem>()),
                PoolKind.Audio => new PoolManager<AudioSource>(this, prefab.GetComponent<AudioSource>()),
                _ => new PoolManager<GameObject>(this, prefab)
            };

            _cache[address] = pool;

            pool.PreWarmPool(poolId._prewarmCount);

            if (!notifyWaiters) return;

            if(_waiters.TryGetValue(poolId.Id, out var waiters))
            {
                _waiters.Remove(poolId.Id);
                _loading.Remove(poolId.Id);

                foreach (var waiter in waiters)
                    waiter.PoolRequesterCallback?.Invoke(poolId.Id, pool);
            }
           
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading resources: {e.Message}");
        }
    }
    // END NEW FUNCTIONS

    [Header("Handles auto return to pools for Audio & SFX pools")]
    private int _maxTrackedPoolObjects = 256;
    private List<PoolObjectTracker> _jobs = new();


    private static bool _addressablesready;

    private static bool _catalogReady = false;

    private static readonly Dictionary<string, PoolAddressSO> _addrToSO = new();
    private static readonly Dictionary<string, GameObject> _addrToSOTemp = new();
    AsyncOperationHandle<IList<PoolAddressSO>> _poolAddressHandle = new();
    private readonly Dictionary<string, PoolManagerBase> _pools = new();
    private readonly Dictionary<PoolIdSO, AsyncOperationHandle<GameObject>> _handles = new();


    /// <summary>
    /// Loads the addreses of all PoolAddressSO in the project into a dictionary for quick lookup.
    /// Finds the addresses via addressable label "Pools".
    /// </summary>
    /// <returns></returns>
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

    public void Testr()
    {
        // Check if pool exists
        // If it does exist => Invoke callback
        // If it doesnt exist => add the struct to queue
    }


    /// <summary>
    /// Asynchronously loads and initializes resources required for the scene, including creating and pre-warming object
    /// pools.
    /// </summary>
    /// <remarks>This method retrieves resource locations with a label matching the current scene and loads the corresponding
    /// assets.  It filters the resources to include only those with a matching configuration in the internal
    /// address-to-configuration mapping. For each loaded asset, an appropriate object pool is created based on the
    /// resource type, and the pool is pre-warmed to the specified size.  Event handlers for resource requests and
    /// releases are registered with the <see cref="SceneEventAggregator"/> to manage resource usage
    /// dynamically.</remarks>
    /// <returns></returns>
    public override async Task LoadResources(/*string sceneName*/)
    {
        SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;

        SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        _jobs.EnsureCapacity(_maxTrackedPoolObjects);
        return;
        try
        {
            if (!_addressablesready)
            {
                await Addressables.InitializeAsync().Task;
                _addressablesready = true;
            }

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
            //await Task.WhenAll(handles.Select(h => h.Task));

            await Awaitable.NextFrameAsync();

            for (int i = 0; i < handles.Count; i++)
            {
                var h = handles[i];
                var config = configsToLoad[i];

                if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null) continue;

               /* PoolManagerBase pool = config.Kind switch
                {
                    PoolKind.ParticleSystem => new PoolManager<ParticleSystem>(this, h.Result.GetComponent<ParticleSystem>()),
                    PoolKind.Audio => new PoolManager<AudioSource>(this, h.Result.GetComponent<AudioSource>()),
                    _ => new PoolManager<GameObject>(this, h.Result)
                };

                _pools[config.Id.Id] = pool;
                _handles[config.Id] = h;

                pool.PreWarmPool(config.PrewarmSize);*/
            }

            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;

            SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
            _jobs.EnsureCapacity(_maxTrackedPoolObjects);

        }
        catch (NullReferenceException e)
        {
            Debug.LogError($"Error loading resources: {e.Message}");
        }
    }

    protected override void InitializePools()
    {
        /*foreach(var pool in _pools)
        {

        }
        foreach (var pool in _pools.Values)
        {
            pool.Initialize();
        }*/
    }


    protected override void ResourceRequested(in ResourceRequests req)
    {
        if (req.PoolId == null || string.IsNullOrEmpty(req.PoolId.Id)) return;
        
        if (_cache.TryGetValue(req.PoolId.Id, out var pool))
        {
            req.PoolRequesterCallback?.Invoke(req.PoolId.Id, pool);
            return;
        }

        _queue.Enqueue(req);
        if (!_isProcessing) ProcessQueue();




/*
        if (req.PoolId == null || string.IsNullOrEmpty(req.PoolId.Id)) return;

        foreach (var tag in poolIds)
        {
            _poolsToLoad.Add(tag.Id);
            if (tag._fxPools == null || tag._fxPools.Count == 0) continue;

            foreach (var fxp in tag._fxPools)
            {
                if (!string.IsNullOrEmpty(fxp.Id)) _poolsToLoad.Add(fxp.Id);
            }

           
        }*/
        /* if (string.IsNullOrEmpty(request.PoolId.Id)) return;
         var id = request.PoolId.Id;

         if (!_pools.TryGetValue(id, out var pool))
         {
             request.PoolRequesterCallback?.Invoke(id, null);
             return;
         }
         request.PoolRequesterCallback?.Invoke(id, pool);*/

    }







    public override bool SchedulePoolObjectRelease(IPoolManager pool, UnityEngine.Object item, float seconds)
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
