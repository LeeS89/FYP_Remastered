using UnityEngine;

public interface IPoolable
{
    void SetParentPool(IPoolManager manager);

    void LaunchPoolable(GameObject owner = null);

}
