using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseSceneManager : MonoBehaviour, ISceneManager
{
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;

    [SerializeField] protected WaypointBlockData _waypointBlockData;
    [SerializeField] protected WaypointManager _waypointManager;
    [SerializeField] protected List<EventManager> _eventManagers;

    public static BaseSceneManager _instance {  get; private set; }

    protected virtual void Awake()
    {
        _instance = this;
    }

    public void SceneStarted()
    {
        //GameManager.Instance.SetPlayer();
        OnSceneStarted?.Invoke();
    }

    public void SceneEnded()
    {
        OnSceneEnded?.Invoke();
    }

    public virtual void SetupScene() { }

    

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





    /// Scene Specific overridden Functions
    protected virtual void LoadSceneResources() { }

    protected virtual void LoadWaypoints() { }

    public virtual BlockData RequestWaypointBlock() { return null; }

    public virtual void ReturnWaypointBlock(BlockData bd) { }

    public virtual void GetImpactParticlePool(ref PoolManager manager) { }

    public virtual void GetBulletPool(ref PoolManager manager) { }

}
