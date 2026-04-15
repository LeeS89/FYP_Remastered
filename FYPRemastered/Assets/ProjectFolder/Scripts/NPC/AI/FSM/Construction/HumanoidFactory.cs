using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanoidFactory : FsmAssemblyBase<HumanoidFsmFeature>
{

    private WaypointResources _wpService; 
    private Task<bool> _wpServiceInitTask;

    private IAddressableService _flankService;
    private IAddressableService _chaseService;

    public HumanoidFactory(HumanoidFsmFeature meta, IPathService pathService) : base(meta, pathService) { }
   

    protected override Task<FsmConfig> CreateConfig(IReadOnlyDictionary<StateId, IFsmState> states)
    {
        throw new System.NotImplementedException();
    }

    protected override async Task<Dictionary<StateId, IFsmState>> CreateStates()
    {
        Dictionary<StateId, IFsmState> states;

        _wpService = await TryLoadStateServiceAndInitialize(ref _wpService, ref _wpServiceInitTask, _metaData.Waypoints);

        if(_wpService is not null)
        {
            PatrolServiceBridge pb = new PatrolServiceBridge(_wpService);
         //   var patrolState = new FsmPatrolState(_metaData.Patrol, _pathService, _wpService);
        }

        return null;
    }
}
