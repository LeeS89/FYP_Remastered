using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoonSceneManager : BaseSceneManager
{
    [SerializeField] private ParticleSystem _normalHitPrefab;
    [SerializeField] private AudioSource _deflectAudioPrefab;
    [SerializeField] private GameObject _normalBulletPrefab;
    private PoolManager _bulletPool;
    private PoolManager _deflectAudioPool;
    private PoolManager _hitParticlePool;
    public Dictionary<GameObject, float> stats = new Dictionary<GameObject, float>();

    public EnemyFSMController _enemy;
    public bool _testspawn = false;
    private ZoneAgentRegistry _zoneAgentRegistry;


    private void Start()
    {
        SetupScene();
    }

    private void Update()
    {
        if(_enemy == null) { return; }

        if (_testspawn)
        {
            _enemy.ResetFSM();
            _testspawn = false;
        }
    }

    public override void SetupScene()
    {
        LoadSceneResources();
       

        InitializePools();
        LoadActiveSceneEventManagers();
        //StartCoroutine(InitializeDelay());
        AssignPools();

        SceneStarted();
        
    }

    IEnumerator InitializeDelay()
    {
        yield return new WaitForSeconds(1f);
        LoadActiveSceneEventManagers();
    }

    protected override void LoadSceneResources()
    {
        _normalBulletPrefab = Resources.Load<GameObject>("Bullets/BasicBullet");
        _normalHitPrefab = Resources.Load<ParticleSystem>("ParticlePoolPrefabs/BasicHit");
        _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");
        _zoneAgentRegistry = new ZoneAgentRegistry();
        LoadWaypoints();
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

    /*protected override void LoadActiveSceneEventManagers()
    {
        _eventManagers = new List<EventManager>();

        _eventManagers.AddRange(FindObjectsByType<EventManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        foreach(var eventManager in _eventManagers)
        {
            if (eventManager is not BulletEventManager)
            {
                eventManager.BindComponentsToEvents();
            }
        }

    }*/


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



    protected override void LoadWaypoints()
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


    #region Agent Registry
   
    public override void RegisterAgentAndZone(EnemyFSMController agent, int zone) => _zoneAgentRegistry.Register(agent, zone);
    public override void UnregisterAgentAndZone(EnemyFSMController agent, int zone) => _zoneAgentRegistry.Unregister(agent, zone);
    public override void AlertZoneAgents(int zone, EnemyFSMController source) => _zoneAgentRegistry.AlertZone(zone, source);
    #endregion

}
