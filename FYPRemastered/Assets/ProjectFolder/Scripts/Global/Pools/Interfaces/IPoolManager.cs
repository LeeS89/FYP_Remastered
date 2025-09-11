using System;
using UnityEngine;

public interface IPoolManager
{
    Type ItemType { get; }
    UnityEngine.Object GetFromPool(Vector3 position, Quaternion rotation);
    void Release(UnityEngine.Object obj);
}
