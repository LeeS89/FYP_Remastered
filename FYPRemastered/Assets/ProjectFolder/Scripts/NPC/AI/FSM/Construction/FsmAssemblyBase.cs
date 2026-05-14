using Npc.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public abstract class FsmAssemblyBase<T> where T : FsmFeatureBase
{
    protected readonly T _metaData;
    protected readonly IPathService _pathService;
    protected IDistanceMonitoringService _distService;

    protected Dictionary<int, Dictionary<StateId, IFsmState>> _registry = new();
    /* protected IAddressableService _fsmSpeedService;
     protected Task<bool> _fsmSpeedServiceInitTask;*/

    public FsmAssemblyBase(T metaData, IPathService pathService)
    {
        if (metaData == null) DebugLogs.RequireNotNull(metaData, "metaData", this);
        _metaData = metaData;
        _pathService = pathService;
    }


    public async Task<(IFsmController manager, INpcBrain brain)> Build(IInstanceIdentifiable callerId, INpcBody body, TryGetCombatTarget targetRetrieverFunc, ITickableRunner tickHost, ICoroutineHost coroutineHost,
        IPathNotifications pathNotifyChannel, IAnimationRequestNotifications animNotifyChannel = null)
    {
        if (callerId is null || body is null || tickHost is null || coroutineHost is null) return (null, null);
        DebugLogs.Log("Calling Build Fsm");
        int id = callerId.EntityId;

        if (_registry.ContainsKey(id)) return (null, null);
       
        var manager = await CreateManager(callerId, body, targetRetrieverFunc, tickHost, coroutineHost, pathNotifyChannel, animNotifyChannel);

        if(manager is null) return (null, null);

        var states = await CreateStates(manager, coroutineHost);
        if (states is null) { DebugLogs.Err("States failed to create"); return (null, null); }

        manager.InjectStates(states);

        _registry.Add(id, states);

        return (manager, GetBrain());
       
    }

    protected async Task<FsmManager> CreateManager(IInstanceIdentifiable id, INpcBody body, TryGetCombatTarget targetRetrieverFunc, ITickableRunner tickHost, 
        ICoroutineHost coroutineHost, IPathNotifications pathNotifySender, IAnimationRequestNotifications animNotifySender = null)
    {
        FsmContext ctx = new FsmContext(body, targetRetrieverFunc, id.EntityId);
        FsmServices svs = new FsmServices(tickHost, coroutineHost);
        FsmOutputChannels cnls = new FsmOutputChannels(pathNotifySender, animNotifySender);
        /*FsmConfig config*/IFsmSpeedControl sc = await CreateSpeedcontroller();


        return sc is not null ? new FsmManager(ctx, svs, cnls, sc) : null;
      /*  if(config is null) return null;

        return new FsmManager(ctx, svs, config);*/
    }

    protected abstract Task<IFsmSpeedControl> CreateSpeedcontroller();

    protected abstract INpcBrain GetBrain();
    
    protected abstract Task<Dictionary<StateId, IFsmState>> CreateStates(FsmManager manager, ICoroutineHost coroutineHost);


    /// <summary>
    /// Attempts to load and initialize a state service instance of the specified type. Returns the initialized instance
    /// if successful; otherwise, returns null.
    /// Contains safety to prevent multiple initialization attempts on the same instance, and ensures proper disposal of the service if initialization fails.
    /// </summary>
    /// <remarks>If initialization fails, the service instance is disposed before returning null. This method
    /// is typically used to ensure that a service is both created and properly initialized before use.</remarks>
    /// <typeparam name="TConcrete">The concrete service type to initialize. Must implement IAddressableService and have a parameterless
    /// constructor.</typeparam>
    /// <param name="instance">An existing instance of the service to initialize, or null to create a new instance.</param>
    /// <param name="initTask">A task representing the asynchronous initialization operation. If null, the method will invoke
    /// TryInitialiseAsync on the instance.</param>
    /// <param name="data">The feature metadata used to initialize the service.</param>
    /// <returns>The initialized service instance if initialization succeeds; otherwise, null.</returns>
    protected Task<TConcrete> TryLoadStateServiceAndInitialize<TConcrete>(
        ref TConcrete instance,
        ref Task<bool> initTask,
        FeatureMeta data)
        where TConcrete : class, IAddressableService, new()
    {

        if (instance is null)
            instance = new TConcrete();

        if (initTask is null)
            initTask = instance.TryInitialiseAsync(data);

        return AwaitAndCheck(instance, initTask);

        static async Task<TConcrete> AwaitAndCheck(TConcrete svc, Task<bool> task)
        {
            if (!await task)
            {
                svc.Dispose();
                return null;
            }
            return svc;
        }
       
    }


    protected void AddstateIfValid<TState>(
        ref Dictionary<StateId, IFsmState> states,
        Func<TState> factory)
        where TState : IFsmState
    {
        var state = factory();
        if (state is null) return;

        states ??= new();
        states.TryAdd(state.GetId(), state);
    }

}
