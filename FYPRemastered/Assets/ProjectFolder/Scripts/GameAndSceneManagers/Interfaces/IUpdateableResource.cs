using System;
using UnityEngine;

[Obsolete("Use ITickable instead")]
public interface IUpdateableResource
{
    void UpdateResource();
}
