using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanoidFactory : FsmAssemblyBase<HumanoidFsmFeature>
{

    private WaypointResources _wpService; 
    private Task<bool> _wpServiceInitTask;

    private FsmSpeedResources _speedService;
    private Task<bool> _speedServiceInitTask;

    private IAddressableService _flankService;
    private ChaseResources _chaseService;
    private Task<bool> _chaseServiceInitTask;

    public HumanoidFactory(HumanoidFsmFeature meta, IPathService pathService) : base(meta, pathService) { }
   

    protected override async Task<FsmConfig> CreateConfig()
    {
        _speedService = await TryLoadStateServiceAndInitialize(ref _speedService, ref _speedServiceInitTask, _metaData.SpeedData);

        if (_speedService is null) return null;
        FsmSpeedControlBridge bridge = new FsmSpeedControlBridge(_speedService);

        return new FsmConfig(bridge, null); // Remember to remove the null param
    }

    protected override async Task<Dictionary<StateId, IFsmState>> CreateStates(FsmManager manager, ICoroutineHost coroutineHost)
    {
        Dictionary<StateId, IFsmState> states = null;
        DestinationProcessor destP = null;

        _wpService = await TryLoadStateServiceAndInitialize(ref _wpService, ref _wpServiceInitTask, _metaData.Waypoints);

       
        AddstateIfValid(ref states, () =>
        {
            if (_wpService is null) return null;

            var pb = new PatrolServiceBridge(_wpService);
            destP ??= new DestinationProcessor(_pathService, coroutineHost);

            return new FsmPatrolState(manager, pb, destP, coroutineHost);
        });

        if (_metaData.ChaseData.enabled)
        {
            _chaseService = await TryLoadStateServiceAndInitialize(ref _chaseService, ref _chaseServiceInitTask, _metaData.ChaseData);

            AddstateIfValid(ref states, () =>
            {
                if (_chaseService is null) return null;
                var distService = GlobalServices.Acquire(() => new DistanceManagerJob());

                var cb = new ChaseServiceBridge(_chaseService, distService, manager);
                destP ??= new DestinationProcessor(_pathService, coroutineHost);
                return new FsmChaseState(manager, cb, destP, coroutineHost);
            });
        }

        /*  PatrolServiceBridge pb = new PatrolServiceBridge(_wpService);
          states ??= new();
          destP ??= new DestinationProcessor(_pathService, coroutineHost);

          var patrolState = new FsmPatrolState(pb, destP, coroutineHost);
          StateId patrolId = patrolState.GetId();
          states.TryAdd(patrolId, patrolState);*/
        // }
        
        return states;
    }
}

