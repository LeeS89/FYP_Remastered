using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanoidFactory : FsmAssemblyBase<HumanoidFsmFeature>
{

    private IAddressableService _wpService; 
    private IAddressableService _flankService;
    private IAddressableService _chaseService;

    public HumanoidFactory(HumanoidFsmFeature meta) : base(meta) { }
   

    protected override Task<FsmConfig> CreateConfig(IReadOnlyDictionary<StateId, IFsmState> states)
    {
        throw new System.NotImplementedException();
    }

    protected override Task<Dictionary<StateId, IFsmState>> CreateStates()
    {
        throw new System.NotImplementedException();
    }
}
