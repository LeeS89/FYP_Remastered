using UnityEngine;

public interface IPoolable
{
    void SetParentPool(IPoolManager manager);
    
}
