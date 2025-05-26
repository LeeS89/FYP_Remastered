using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseSceneManager : MonoBehaviour, ISceneManager
{
    public static BaseSceneManager _instance { get; private set; }

    [SerializeField] protected List<EventManager> _eventManagers;
    [SerializeField] protected ParticleSystem _normalHitPrefab;
    [SerializeField] protected AudioSource _deflectAudioPrefab;
    [SerializeField] protected GameObject _normalBulletPrefab;
    protected PoolManager _bulletPool;
    protected PoolManager _deflectAudioPool;
    protected PoolManager _hitParticlePool;
    protected PathRequestManager _pathRequestManager;

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


    /// <summary>
    /// Events used with Enemy AI system to notify when the closest flanking point to player has changed.
    /// When there is an active Alert status - OnClosestPointToPlayerChanged will be invoked by the player when ever they stop moving
    /// This in turn runs the ClosestPointToPlayerJob to find the closest point to player. Once job completes,
    /// OnClosestPointToPlayerJobComplete notifies all interested parties with the index of the closest point to player.
    /// </summary>
    public event Action OnClosestPointToPlayerChanged;
    public event Action<int> OnClosestPointToPlayerJobComplete;

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    public void ClosestPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestPointToPlayerJobComplete?.Invoke(pointIndex);
    }


    #endregion

    #region Abstract Functions
    public abstract void SetupScene();
    protected abstract void LoadSceneResources();

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

    #region Waypoint and enemy alert phase Functions  
    protected virtual void LoadWaypoints() { }


    public virtual BlockData RequestWaypointBlock() { return null; }

    public virtual void ReturnWaypointBlock(BlockData bd) { }

    public virtual void RegisterAgentAndZone(EnemyFSMController agent, int zone) { }
    public virtual void UnregisterAgentAndZone(EnemyFSMController agent, int zone) { }
    public virtual void AlertZoneAgents(int zone, EnemyFSMController source) { }

    public virtual void EnqueuePathRequest(PathRequest request) { }
    #endregion



    #region Object Pooling Region
    public virtual void GetImpactParticlePool(ref PoolManager manager) { }

    public virtual void GetBulletPool(ref PoolManager manager) { }

    #endregion


}
