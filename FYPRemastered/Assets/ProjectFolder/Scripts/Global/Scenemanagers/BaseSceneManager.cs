using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseSceneManager : MonoBehaviour, ISceneManager
{
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;

    [SerializeField] protected List<EventManager> _eventManagers;

    public static BaseSceneManager _instance {  get; private set; }

    protected virtual void Awake()
    {
        _instance = this;
    }

    public void SceneStarted()
    {
        OnSceneStarted?.Invoke();
    }

    public void SceneEnded()
    {
        OnSceneEnded?.Invoke();
    }

    public virtual void SetupScene() { }

    protected virtual void LoadSceneResources() { }

    protected virtual void LoadActiveSceneEventManagers() { }


    public virtual void GetImpactParticlePool(ref PoolManager manager) { }

    public virtual void GetBulletPool(ref PoolManager manager) { }


}
