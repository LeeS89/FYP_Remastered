using UnityEngine;

public class AutoReturnToPool : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _ps;
    public IPoolManager PoolManager { get; private set; }

    public void LaunchPoolable(GameObject owner) { }


    public void SetParentPool(IPoolManager manager)
    {
        if (manager == null) return;
        PoolManager = manager;
    }

    
    // Update is called once per frame
    void Update()
    {
        if (!_ps.IsAlive())
        {
            PoolManager?.Release(gameObject);
        }
    }
}
