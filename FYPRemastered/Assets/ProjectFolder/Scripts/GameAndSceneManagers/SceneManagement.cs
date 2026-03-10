using System;
using System.Threading.Tasks;
using UnityEngine;

public class SceneManagement : SceneManagementBase, ISceneService
{
    [SerializeField] private WaypointBlockData _waypointBlockData;
    [SerializeField] private WaypointManager _waypointManager;
   // private SceneResourceManager _resources;

    private SceneServiceBus _sceneServiceBus;


    // public Dictionary<GameObject, float> stats = new Dictionary<GameObject, float>();

    // Test Variables
   // public EnemyFSMController _enemy;
    public bool _testspawn = false;

    public event Action OnSceneBegin;
    public event Action OnSceneEnd;

    /// End Test

    private async void Start()
    {
        await SetupScene(); // Will be called by Game Manager once fully tested
    }

    // Update used for Testing only - Delete Later
    private void Update()
    {


        if (_testspawn)
        {
        //    _enemy.ResetFSM();
            _testspawn = false;
        }

        _sceneServiceBus?.Tick(Time.deltaTime);
        //_resources?.UpdateResources();

    }

    public void OnTargetableDied(ITargetable targetable)
    {
        throw new NotImplementedException();
    }

    public void OnTargetableRespawned(ITargetable targetable)
    {
        throw new NotImplementedException();
    }


    public override async Task SetupScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        _sceneServiceBus = new SceneServiceBus(sceneName, this);

        //await _sceneServiceBus.InitialiseServicesAsync();
        await _sceneServiceBus.NewInit();
        // await _resources.LoadResourcesAsync();
        //  await _resources.LoadDependancies();

        RegisterGettableComponents();

        LoadActiveSceneEventManagers(_sceneServiceBus);

        OnSceneBegin?.Invoke();
        //SceneStarted();

    }






    protected override void UnloadSceneResources()
    {
        // Implement Later
        // Clear Damageables
        // ensure all resource class' fields are cleared
    }







    #region Obsolete Code
    [Obsolete]
    private void AssignPools() // => Sort next time
    {
        /*
                var impactAudioSubscribers = InterfaceRegistry.GetAll<IImpactAudio>();

                foreach (var audioSubscriber in impactAudioSubscribers)
                {
                    audioSubscriber.SetDeflectAudioPool(_deflectAudioPool);
                }*/

    }



    /*public override void GetBulletPool(ref PoolManager manager)
    {
        manager = _bulletPool;
    }*/

    /* private void InitializePools()
     {
         if (_normalBulletPrefab == null || _deflectAudioPrefab == null || _normalHitPrefab == null)
         {
             Debug.LogError("Failed to load Scene Resources");
             return;
         }

         _hitParticlePool = new PoolManager(_normalHitPrefab, 40, 80);
         //_hitParticlePool.PrewarmPool(PoolContents.Particle, 15);

         _deflectAudioPool = new PoolManager(_deflectAudioPrefab, 5, 10);
         _deflectAudioPool.PrewarmPool(PoolContents.Audio, 5);

         _bulletPool = new PoolManager(_normalBulletPrefab, 40, 80);
         //_bulletPool.PrewarmPool(PoolContents.Object, 40);



     }*/
   

    //public override void UnregisterAgentAndZone(EnemyFSMController agent, int zone) => _zoneAgentRegistry.Unregister(agent, zone);
    //public override void RegisterAgentAndZone(EnemyFSMController agent, int zone) => _zoneAgentRegistry.Register(agent, zone);

    //public override void AlertZoneAgents(int zone, EnemyFSMController source) => _zoneAgentRegistry.AlertZone(zone, source);
    #endregion

}
