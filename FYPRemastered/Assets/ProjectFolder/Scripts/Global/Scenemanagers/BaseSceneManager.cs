using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BaseSceneManager : MonoBehaviour, ISceneManager
{
    public static BaseSceneManager _instance { get; private set; }

    [SerializeField] protected List<EventManager> _eventManagers;
  
    protected PoolManager _deflectAudioPool;
  
    protected virtual void Awake()
    {
        _instance = this;
    }

    #region Scene Events
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;

    public void SceneStarted()
    {
        //GameManager.Instance.SetPlayer();
        OnSceneStarted?.Invoke();
    }

    public void SceneEnded()
    {
        OnSceneEnded?.Invoke();
    }


   


    #endregion

    #region Abstract Functions
    public abstract Task SetupScene();
    protected abstract Task LoadSceneResources();

    protected abstract void UnloadSceneResources();
    #endregion

    #region Shared Scene Functions

    protected virtual void LoadActiveSceneEventManagers()
    {
        _eventManagers = new List<EventManager>();

        _eventManagers.AddRange(FindObjectsByType<EventManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        foreach (var eventManager in _eventManagers)
        {
            if (eventManager is not BulletEventManager)
            {
                eventManager.BindComponentsToEvents();
            }
        }
    }
    #endregion



    #region Obsolete Code
    //protected virtual void LoadWaypoints() { }
    // public virtual void RegisterAgentAndZone(EnemyFSMController agent, int zone) { }
    public virtual void UnregisterAgentAndZone(EnemyFSMController agent, int zone) { }
    //public virtual void AlertZoneAgents(int zone, EnemyFSMController source) { }

    // public virtual void EnqueuePathRequest(DestinationRequestData request) { }

    //public virtual UniformZoneGridManager GetGridManager() => null;

    /// <summary>
    /// Events used with Enemy AI system to notify when the closest flanking point to player has changed.
    /// When there is an active Alert status - OnClosestPointToPlayerChanged will be invoked by the player when ever they stop moving
    /// This in turn runs the ClosestPointToPlayerJob to find the closest point to player. Once job completes,
    /// OnClosestPointToPlayerJobComplete notifies all interested parties with the index of the closest point to player.
    /// </summary>
    public event Action OnClosestPointToPlayerChanged; // => Moving to Scene event aggregator
    public event Action<int> OnClosestPointToPlayerJobComplete; // => Moving to Scene event aggregator

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    [Obsolete("Use Scene Event Aggregator instead")]
    public void ClosestPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestPointToPlayerJobComplete?.Invoke(pointIndex);
    }

    public virtual void GetImpactParticlePool(ref PoolManager manager) { }

    //public virtual void GetBulletPool(ref PoolManager manager) { }
    #endregion
}
