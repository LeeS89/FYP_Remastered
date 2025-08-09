using System;
using UnityEngine;

public interface IPoolManager
{
    Type ItemType { get; }
    UnityEngine.Object Get(Vector3 position, Quaternion rotation);
    void Release(UnityEngine.Object obj);
}
