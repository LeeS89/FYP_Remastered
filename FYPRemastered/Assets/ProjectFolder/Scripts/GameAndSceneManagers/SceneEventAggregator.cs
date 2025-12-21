using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SceneEventAggregator : MonoBehaviour
{
    public static SceneEventAggregator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    #region Global Scene Events
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;
   // public event Action<ResourceRequest> OnResourceRequested;
   // public event Action<AIDestinationRequestData> OnAIResourceRequested;
    public event Action<ResourceRequest> OnResourceReleased;
    //public event Action<List<Type>> OnDependanciesAdded;
    public event Action<SceneResources> OnDependancyAdded;
    public event Func<Type, bool> OnCheckDependencyExists;

    // new way
    public delegate void ResourcesRequestedHandler(in ResourceRequests request);
    public event ResourcesRequestedHandler OnResourceRequested;
   // public event Action<ResourceRequests> OnResourcesRequested;

    public void ResourceRequested(in ResourceRequests request) => OnResourceRequested?.Invoke(request);

    //NEW AI
    //public event Action<T> OnAIResourceRequest;
    // END NEW AI

    public void SceneStarted() => OnSceneStarted?.Invoke();


    public void SceneEnded() => OnSceneEnded?.Invoke();


   /* [Obsolete("Use RequestingResource instead")]
    public void RequestResource(ResourceRequest request)
    {
        if (request is AIDestinationRequestData aiRequest)
        {
            OnAIResourceRequested?.Invoke(aiRequest);
        }
        else
        {
            OnResourceRequested?.Invoke(request);
        }
           
    }*/

    public void ReleaseResource(ResourceRequest request) => OnResourceReleased?.Invoke(request);


    /*public void AddDependancies(List<Type> resourceDependancies)
    {
        
        OnDependanciesAdded?.Invoke(resourceDependancies);
    }*/

    public void AddDependancy(SceneResources resource)
    {
        // Invoke the event with the resource
        OnDependancyAdded?.Invoke(resource);
    }

    public bool CheckDependancyExists(Type dependency) => OnCheckDependencyExists?.Invoke(dependency) ?? false;

    #endregion


    #region AI Agent Events

    #region Player Flanking Point Events
    /// <summary>
    /// Events used with Enemy AI system to notify when the closest flanking point to player has changed.
    /// When there is an active Alert status - OnClosestPointToPlayerChanged will be invoked by the player when ever they stop moving
    /// This in turn runs the ClosestPointToPlayerJob to find the closest point to player. Once job completes,
    /// OnClosestPointToPlayerJobComplete notifies all interested parties with the index of the closest point to player.
    /// </summary>
    public event Action OnClosestPointToPlayerChanged;
    public event Action<int> OnClosestFlankPointToPlayerJobComplete;
    public event Action OnRunClosestPointToPlayerJob; 
    //public event Action<ResourceRequest> OnFlankPointsRequested;

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    public void ClosestFlankPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestFlankPointToPlayerJobComplete?.Invoke(pointIndex);
    }

    public void RunClosestPointToPlayerJob()
    {
        OnRunClosestPointToPlayerJob?.Invoke();
    }

    /* public void FlankPointsRequested(ResourceRequest request) // Change
     {
         OnFlankPointsRequested?.Invoke(request);
     }*/
    #endregion Scene Events

    #region Agent Zone Registry Events
    public event Action<FSMControllerBase, int> OnAgentZoneRegistered;
    
    public event Action<FSMControllerBase, int> OnAgentZoneUnRegistered;
    public event Action<int, FSMControllerBase> OnAlertZoneAgents;

    public void RegisterAgentAndZone(FSMControllerBase agent, int zone)
    {
        OnAgentZoneRegistered?.Invoke(agent, zone);
    }

    public void UnRegisterAgentAndZone(FSMControllerBase agent, int zone)
    {
        OnAgentZoneUnRegistered?.Invoke(agent, zone);
    }

    public void AlertZoneAgents(int zone, FSMControllerBase source)
    {
        OnAlertZoneAgents?.Invoke(zone, source);
    }

    // NEW

    public Action<INotificationListener, ZoneId> OnRegisterAgentAndZone;
    public void RegisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnRegisterAgentAndZone?.Invoke(agent, zone);

    public Action<INotificationListener, ZoneId> OnUnRegisterAgentAndZone;
    public void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnUnRegisterAgentAndZone?.Invoke(agent, zone);

    public Func<ZoneId, INotificationListener, bool> OnAlertAgentsInZone;
    public bool AlertAgentsInZone(ZoneId zone, INotificationListener listener)
        => OnAlertAgentsInZone?.Invoke(zone, listener) ?? true; // if no subscribers, default to true
    
    // END NEW
    #endregion



    #endregion
}

public class SceneServiceBus : ISceneServiceProvider
{
    #region Global Scene Events
    
    public event Action<ResourceRequest> OnResourceReleased;
    //public event Action<List<Type>> OnDependanciesAdded;
    public event Action<SceneResources> OnDependancyAdded;
    public event Func<Type, bool> OnCheckDependencyExists;

    // new way
    public delegate void ResourcesRequestedHandler(in ResourceRequests request);
    public event ResourcesRequestedHandler OnResourceRequested;
    // public event Action<ResourceRequests> OnResourcesRequested;

    public void ResourceRequested(in ResourceRequests request) => OnResourceRequested?.Invoke(request);


    public void ReleaseResource(ResourceRequest request) => OnResourceReleased?.Invoke(request);


    public void AddDependancy(SceneResources resource)
    {
        // Invoke the event with the resource
        OnDependancyAdded?.Invoke(resource);
    }

    public bool CheckDependancyExists(Type dependency) => OnCheckDependencyExists?.Invoke(dependency) ?? false;

    #endregion


    #region AI Agent Events

    #region Player Flanking Point Events
    /// <summary>
    /// Events used with Enemy AI system to notify when the closest flanking point to player has changed.
    /// When there is an active Alert status - OnClosestPointToPlayerChanged will be invoked by the player when ever they stop moving
    /// This in turn runs the ClosestPointToPlayerJob to find the closest point to player. Once job completes,
    /// OnClosestPointToPlayerJobComplete notifies all interested parties with the index of the closest point to player.
    /// </summary>
    public event Action OnClosestPointToPlayerChanged;
    public event Action<int> OnClosestFlankPointToPlayerJobComplete;
    public event Action OnRunClosestPointToPlayerJob;
    //public event Action<ResourceRequest> OnFlankPointsRequested;

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    public void ClosestFlankPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestFlankPointToPlayerJobComplete?.Invoke(pointIndex);
    }

    public void RunClosestPointToPlayerJob()
    {
        OnRunClosestPointToPlayerJob?.Invoke();
    }

 
    #endregion Scene Events

    #region Agent Zone Registry Events
    public event Action<FSMControllerBase, int> OnAgentZoneRegistered;

    public event Action<FSMControllerBase, int> OnAgentZoneUnRegistered;
    public event Action<int, FSMControllerBase> OnAlertZoneAgents;
    public event Action OnSceneBegin;
    public event Action OnSceneEnd;

    public void RegisterAgentAndZone(FSMControllerBase agent, int zone)
    {
        OnAgentZoneRegistered?.Invoke(agent, zone);
    }

    public void UnRegisterAgentAndZone(FSMControllerBase agent, int zone)
    {
        OnAgentZoneUnRegistered?.Invoke(agent, zone);
    }

    public void AlertZoneAgents(int zone, FSMControllerBase source)
    {
        OnAlertZoneAgents?.Invoke(zone, source);
    }

    // NEW

    public Action<INotificationListener, ZoneId> OnRegisterAgentAndZone;
    public void RegisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnRegisterAgentAndZone?.Invoke(agent, zone);

    public Action<INotificationListener, ZoneId> OnUnRegisterAgentAndZone;
    public void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnUnRegisterAgentAndZone?.Invoke(agent, zone);

    public Func<ZoneId, INotificationListener, bool> OnAlertAgentsInZone;

    public IWaypointService WaypointService => throw new NotImplementedException();

    public INpcService NpcService => throw new NotImplementedException();

    public IFlankService FlankService => throw new NotImplementedException();

    public IPoolService PoolService => throw new NotImplementedException();

    public IPathService PathService => throw new NotImplementedException();

    public IPlayerRefService PlayerRefService => throw new NotImplementedException();

    public bool AlertAgentsInZone(ZoneId zone, INotificationListener listener)
        => OnAlertAgentsInZone?.Invoke(zone, listener) ?? true; // if no subscribers, default to true

    public void OnTargetableDied(ITargetable targetable)
    {
        throw new NotImplementedException();
    }

    // END NEW
    #endregion



    #endregion
}

public interface ISceneServiceProvider : ISceneAIServices, IScenePoolServices, ISceneService
{
    // IPoolService PoolService { get; }
}

public interface IGlobalServices
{
    ISceneService SceneService { get; }
}

public interface ISceneService
{
    event Action OnSceneBegin;
    event Action OnSceneEnd;
    void OnTargetableDied(ITargetable targetable);
}

public interface IGameManager
{
    void PlayerDied();
    void PlayerRespawned();
}


public interface IPlayerRefService
{
    event Action OnPlayerDied;
    event Action OnPlayerRespawned;
    ITargetable GetPlayer();
}

public interface ISceneAIServices : ISceneService
{
    IPlayerRefService PlayerRefService { get; }
    IPathService PathService { get; }
    IWaypointService WaypointService { get; }
    INpcService NpcService { get; }
    IFlankService FlankService { get; }
}

public interface IPathService
{
    void RequestPath(Vector3 from, Vector3 to, NavMeshPath path, Action<PathResult> onRequestComplete);
}

public interface IWaypointService //: IDestinationService
{
    bool TryGetWaypoints(object requester, List<Vector3> buffer);

    bool TryReleaseWaypoints(object requester, List<Vector3> buffer);
}

public interface IFlankService //: IDestinationService
{
    void TryGetFlankCandidates(Vector3 flankTargetPos, int numSteps, List<Vector3> buffer, Action<bool> OnRequestComplete);
}

public interface IClosestFlankPointService
{
    void RequestClosestIndex(int id, Vector3 targetPosition, Action<int, int, bool> OnRequestComplete);
}

public interface IScenePoolServices
{
    IPoolService PoolService { get; }
}

public interface INpcService
{
    void RegisterAgentAndZone(INotificationListener agent, ZoneId zone);
    void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone);
    bool TryAlertAgentsInZone(ZoneId zone, INotificationListener listener);
}

public interface  IPoolService
{
    
}


public interface IServicable<TServices, TManager>
{
    void Load(TServices services, TManager manager);
    void Unload();
}


[Obsolete]
public interface IDestinationService
{
    DestinationServiceId ServiceId { get; }
}

[Obsolete]
public enum DestinationServiceId
{
    WaypointService,
    FlankPointService,
    TargetPointService
}

public interface Test1
{
    void Helping();
}
public interface Test2 : Test1 { }


public class TestClass1
{
    public virtual void TestMethod<T>(T one) { }
}

public class TestClass2 : TestClass1
{
    public override void TestMethod<Test3>(Test3 one)
    {
       // one.Help();
        base.TestMethod(one);
    }
   
}


public interface IBase { }
public interface IChild : IBase { }

public abstract class Handler<T> where T : IBase
{
    public abstract void Handle(T obj);
}

public class ChildHandler : Handler<IChild>
{
    public override void Handle(IChild obj)
    {
        // Can assume IChild here
    }
}

