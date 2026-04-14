using Npc.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class FsmAssemblyBase<T> where T : FsmFeatureBase
{
    protected readonly T _metaData;
    protected IAddressableService _fsmSpeedService;

    public FsmAssemblyBase(T metaData)
    {
        if (metaData == null) DebugLogs.RequireNotNull(metaData, "metaData", this);
        _metaData = metaData;
    }


    public async Task<IFsmController> Build(IInstanceIdentifiable callerId, INpcBody body, TryGetTarget targetRetrieverFunc, ITickableRunner tickHost, ICoroutineHost coroutineHost, 
        IPathNotifications pathNotifySender, IAnimationRequestNotifications animNotifySender = null)
    {
        if (callerId is null || body is null || tickHost is null || coroutineHost is null) return null;


        var states = await CreateStates();
        if (states is null) return null;

        return await CreateManager(callerId, body, states, targetRetrieverFunc, tickHost, coroutineHost, pathNotifySender, animNotifySender);
    }

    protected async Task<IFsmController> CreateManager(IInstanceIdentifiable id, INpcBody body, IReadOnlyDictionary<StateId, IFsmState> states, TryGetTarget targetRetrieverFunc, ITickableRunner tickHost, 
        ICoroutineHost coroutineHost, IPathNotifications pathNotifySender, IAnimationRequestNotifications animNotifySender = null)
    {
        FsmContext ctx = new FsmContext(body, targetRetrieverFunc, id.EntityId);
        FsmServices svs = new FsmServices(tickHost, coroutineHost, pathNotifySender, animNotifySender);
        FsmConfig config = await CreateConfig(states);

        return new FsmManager(ctx, svs, config);
    }

    protected abstract Task<FsmConfig> CreateConfig(IReadOnlyDictionary<StateId, IFsmState> states);
    
    protected abstract Task<Dictionary<StateId, IFsmState>> CreateStates();


    protected async Task<TConcrete> TryLoadStateServiceAndInitialize<TConcrete>(FeatureMeta data/*, Func<TConcrete> createFunc*/) where TConcrete : class, IAddressableService, new()
    {
        //    if (string.IsNullOrWhiteSpace(data.addressKey)) { DebugLogs.RequireNotNull(data.addressKey, "addressKey", this); return null; }

        var svc = new TConcrete();
        if (svc == null)
        {
            DebugLogs.RequireNotNull(svc, $"{typeof(TConcrete).Name}", this);
            return null;
        }

        bool serviceInitSuccess = await svc.TryInitialiseAsync(data);

        if (!serviceInitSuccess)
        {
            DebugLogs.LoadFail(svc, $"(The Service of {typeof(TConcrete).Name})", this);
            svc.Dispose();
            return null;

        }

        return svc;
    }

}
