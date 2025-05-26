using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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

    private void Start()
    {
        SetupScene(); // Will be called by Game Manager once fully tested
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

    public override void SetupScene()
    {
        LoadSceneResources();
       

        InitializePools();
        LoadActiveSceneEventManagers();
        
        AssignPools();

        SceneStarted();
        
    }


    protected override void LoadSceneResources() // Create Resources Component Later
    {
        _normalBulletPrefab = Resources.Load<GameObject>("Bullets/BasicBullet");
        _normalHitPrefab = Resources.Load<ParticleSystem>("ParticlePoolPrefabs/BasicHit");
        _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");
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
        }
        if (_waypointManager != null)
        {
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
    public override void EnqueuePathRequest(PathRequest request) => _pathRequestManager.AddRequest(request);
    #endregion

}
