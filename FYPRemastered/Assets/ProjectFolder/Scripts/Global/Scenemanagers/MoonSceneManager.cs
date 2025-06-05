using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;


public class MoonSceneManager : BaseSceneManager
{
    [SerializeField] private WaypointBlockData _waypointBlockData;
    [SerializeField] private WaypointManager _waypointManager;
    
    public Dictionary<GameObject, float> stats = new Dictionary<GameObject, float>();

    // Test Variables
    public EnemyFSMController _enemy;
    public bool _testspawn = false;
    public bool _testJobRun = false;
    /// End Test

    private ZoneAgentRegistry _zoneAgentRegistry;
    private ClosestPointToPlayerJob _closestPointjob;
    [SerializeField] private UniformZoneGridManager _gridManager;

    private async void Start()
    {
        await SetupScene(); // Will be called by Game Manager once fully tested
    }

    
    public override void TestRun()
    {
        _closestPointjob.RunClosestPointJob();
    }

    // Update used for Testing only - Delete Later
    private void Update()
    {
        if (_testJobRun)
        {
            _closestPointjob.RunClosestPointJob();
            _testJobRun = false;
        }

        //if(_enemy == null) { return; }

        if (_testspawn)
        {
            _enemy.ResetFSM();
            _testspawn = false;
        }

        if (_pathRequestManager != null && _pathRequestManager.GetPendingRequestCount() > 0)
        {
            _pathRequestManager.ExecutePathRequests();
        }
        
    }

    public override async Task SetupScene()
    {
        await LoadSceneResources();

        
        InitializePools();
        LoadActiveSceneEventManagers();
        
        AssignPools();
        // await Task.Delay(3000);
        // await Task.CompletedTask;
        await WaitUntilNavMeshReady();
        Debug.LogError("Nav mesh ready");
       // await Task.Delay(10000); // Wait for NavMesh to be ready before proceeding
        //SceneStarted();
        FireSceneStarted();

    }


    private void FireSceneStarted()
    {
        SceneStarted();
    }

    private async Task WaitUntilNavMeshReady()
    {
        await Task.Yield(); // Wait 1 frame (optional but good first step)

        // Wait until at least 1 frame passes and NavMesh is ready
        while (!NavMeshReady())
        {
            await Task.Yield(); // Check again on next frame
        }
    }

    private bool NavMeshReady()
    {
        Vector3 Playerpos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
       
        // Option 1: Check if the sample position returns valid result
        NavMeshHit hit;
        return NavMesh.SamplePosition(Playerpos, out hit, 2f, NavMesh.AllAreas);
    }



    protected override async Task LoadSceneResources() // Create Resources Component Later
    {
        try
        {
            var bulletHandle = Addressables.LoadAssetAsync<GameObject>("BasicBullet");
            _normalBulletPrefab = await bulletHandle.Task;

            var hitHandle = Addressables.LoadAssetAsync<GameObject>("BasicHit");
            _normalHitPrefab = (await hitHandle.Task).GetComponent<ParticleSystem>();

            /*Addressables.LoadAssetAsync<GameObject>("BasicBullet").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _normalBulletPrefab = handle.Result;
                    Debug.LogError("Successfully loaded BasicBullet prefab from Addressables");
                }
                else
                {
                    Debug.LogError("Failed to load BasicBullet prefab from Addressables");
                }
            };*/
            //_normalBulletPrefab = Resources.Load<GameObject>("Bullets/BasicBullet");

            /*Addressables.LoadAssetAsync<GameObject>("BasicHit").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _normalHitPrefab = handle.Result.GetComponent<ParticleSystem>();
                    Debug.LogError("Successfully loaded BasicHit prefab from Addressables");
                }
                else
                {
                    Debug.LogError("Failed to load BasicHit prefab from Addressables");
                }
            };*/
            //_normalHitPrefab = Resources.Load<ParticleSystem>("ParticlePoolPrefabs/BasicHit");
            _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");
            
            //await Task.Delay(1000); // Wait for resources to load

            _zoneAgentRegistry = new ZoneAgentRegistry();
            LoadWaypoints();

            _gridManager = FindFirstObjectByType<UniformZoneGridManager>();

            if (!_gridManager)
            {
                Debug.LogError("No Grid Manager found in scene");

                return;
            }

            _closestPointjob = new ClosestPointToPlayerJob();
            _closestPointjob.AddSamplePointData(_gridManager.InitializeGrid());

            _pathRequestManager = new PathRequestManager();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading scene resources: {e.Message}");
        }
       
    }

    

    public override UniformZoneGridManager GetGridManager()
    {
        return _gridManager;
    }

    protected override void UnloadSceneResources()
    {
       // Implement Later
    }
   

    private void InitializePools()
    {
        if(_normalBulletPrefab == null || _deflectAudioPrefab == null || _normalHitPrefab == null)
        {
            Debug.LogError("Failed to load Scene Resources");
            return;
        }

        _hitParticlePool = new PoolManager(_normalHitPrefab, this, 40, 80);
        //_hitParticlePool.PrewarmPool(PoolContents.Particle, 15);

        _deflectAudioPool = new PoolManager(_deflectAudioPrefab, this, 5, 10);
        _deflectAudioPool.PrewarmPool(PoolContents.Audio, 5);

        _bulletPool = new PoolManager(_normalBulletPrefab, 40, 80);
        //_bulletPool.PrewarmPool(PoolContents.Object, 40);

        
       
    }

    public override void GetImpactParticlePool(ref PoolManager manager)
    {
        manager = _hitParticlePool;  
    }

    public override void GetBulletPool(ref PoolManager manager)
    {
        manager = _bulletPool;
    }

    private void AssignPools()
    {
        
        var impactAudioSubscribers = InterfaceRegistry.GetAll<IImpactAudio>();

        foreach(var audioSubscriber in impactAudioSubscribers)
        {
            audioSubscriber.SetDeflectAudioPool(_deflectAudioPool);
        }

    }



    protected override void LoadWaypoints() // Create WaypointManager Component Later
    {
        if (_waypointManager == null)
        {
            _waypointManager = FindFirstObjectByType<WaypointManager>();
            Debug.LogError("Finding Waypoint manager");
        }
        if (_waypointManager != null)
        {
            Debug.LogError("Waypoint manager found");
            _waypointBlockData = _waypointManager.RetreiveWaypointData();
        }

        if (_waypointBlockData != null)
        {
            foreach (var blockData in _waypointBlockData.blockDataArray)
            {
                blockData._inUse = false;
            }
        }
    }

    public override BlockData RequestWaypointBlock()
    {
        if (_waypointBlockData == null)
        {
            Debug.LogWarning("No Waypoints exist in the scene, please update waypoint manager");
            return null;
        }

        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                return blockData;
            }
        }
        return null;
    }

    public override void ReturnWaypointBlock(BlockData bd)
    {
        if (!_waypointBlockData.blockDataArray.Contains(bd)) { return; }

        int index = Array.FindIndex(_waypointBlockData.blockDataArray, block => block == bd);

        if (index >= 0)
        {
            _waypointBlockData.blockDataArray[index]._inUse = false;
        }
       
    }


    #region Agent Zone Registry
   
    public override void RegisterAgentAndZone(EnemyFSMController agent, int zone) => _zoneAgentRegistry.Register(agent, zone);
    public override void UnregisterAgentAndZone(EnemyFSMController agent, int zone) => _zoneAgentRegistry.Unregister(agent, zone);
    public override void AlertZoneAgents(int zone, EnemyFSMController source) => _zoneAgentRegistry.AlertZone(zone, source);
    #endregion

    #region Agent Path Finding
    public override void EnqueuePathRequest(DestinationRequestData request) => _pathRequestManager.AddRequest(request);
    #endregion

}
