
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

public class PoolResources : SceneResources, IUpdateableResource
{
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
              
                PoolManagerBase pool = config.Kind switch
                {
                    PoolKind.ParticleSystem => new PoolManager<ParticleSystem>(this, h.Result.GetComponent<ParticleSystem>()),
                    PoolKind.Audio => new PoolManager<AudioSource>(this, h.Result.GetComponent<AudioSource>()),
                    _ => new PoolManager<GameObject>(this, h.Result)
                };
              
                _pools[config.Id.Id] = pool;
                _handles[config.Id] = h;
                
                pool.PreWarmPool(config.PrewarmSize);
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


    protected override void ResourceRequested(in ResourceRequests request)
    {
        if (string.IsNullOrEmpty(request.PoolId)) return;
        var id = request.PoolId;    

        if (!_pools.TryGetValue(id, out var pool))
        {
            request.PoolRequesterCallback?.Invoke(id, null);
            return;
        }
        request.PoolRequesterCallback?.Invoke(id, pool);
      
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



    #region obsolete
    /* List<AsyncOperationHandle> _cached = new();
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
    public override async Task LoadResources(*//*string sceneName*//*)
    {
        try
        {
            if (!_addressablesready)
            {
                await Addressables.InitializeAsync().Task;
                _addressablesready = true;
            }

            await EnsureCatalogAsync();

            //var hn = new List<AsyncOperationHandle>();
            var configsToLoads = new List<PoolAddressSO>();
            var objectsToLoad = new List<GameObject>();
            foreach (var locs in _addrToSO)
            {
                
                var h = Addressables.LoadAssetAsync<GameObject>(locs.Key);
                _cached.Add(h);
                await h.Task;
                if(h.Status == AsyncOperationStatus.Succeeded && h.Result != null)
                {
                    objectsToLoad.Add(h.Result);
                    configsToLoads.Add(locs.Value);
                    //_addrToSOTemp[locs.Key] = h.Result;
                }
            }
            for(int i = 0; i < _cached.Count; i++)
            {
                var cnfg = configsToLoads[i];
                GameObject go = objectsToLoad[i];

                PoolManagerBase pool = cnfg.Kind switch
                {
                    PoolKind.ParticleSystem => new PoolManager<ParticleSystem>(this, go.GetComponent<ParticleSystem>()),
                    PoolKind.Audio => new PoolManager<AudioSource>(this, go.GetComponent<AudioSource>()),
                    _ => new PoolManager<GameObject>(this, go)
                };
                _pools[cnfg.Id.Id] = pool;
                pool.PreWarmPool(cnfg.PrewarmSize);
            }

            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;

            SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
            _jobs.EnsureCapacity(_maxTrackedPoolObjects);

            return;

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
                // await h.Task;

            }
            await Task.WhenAll(handles.Select(h => h.Task));

            await Awaitable.NextFrameAsync();

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

                _pools[config.Id.Id] = pool;
                _handles[config.Id] = h;

                pool.PreWarmPool(config.PrewarmSize);
            }

            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;

            SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
            _jobs.EnsureCapacity(_maxTrackedPoolObjects);

        }
        catch (NullReferenceException e)
        {
            Debug.LogError($"Error loading resources: {e.Message}");
        }
    }*/
    #endregion
}
