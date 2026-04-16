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
    private IAddressableService _chaseService;

    public HumanoidFactory(HumanoidFsmFeature meta, IPathService pathService) : base(meta, pathService) { }
   

    protected override async Task<FsmConfig> CreateConfig(IReadOnlyDictionary<StateId, IFsmState> states)
    {
        _speedService = await TryLoadStateServiceAndInitialize(ref _speedService, ref _speedServiceInitTask, _metaData.SpeedData);

        if (_speedService is null) return null;
        FsmSpeedControlBridge bridge = new FsmSpeedControlBridge(_speedService);

        return new FsmConfig(bridge, states);
    }

    protected override async Task<Dictionary<StateId, IFsmState>> CreateStates(ICoroutineHost coroutineHost)
    {
        Dictionary<StateId, IFsmState> states = null;
        DestinationProcessor destP = null;

        _wpService = await TryLoadStateServiceAndInitialize(ref _wpService, ref _wpServiceInitTask, _metaData.Waypoints);

        // if(_wpService is not null)
        // {

        AddstateIfValid(ref states, () =>
        {
            if (_wpService is null) return null;

            var pb = new PatrolServiceBridge(_wpService);
            destP ??= new DestinationProcessor(_pathService, coroutineHost);

            return new FsmPatrolState(pb, destP, coroutineHost);
        });

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
