using System.Collections.Generic;
using UnityEngine;
using System;

[Obsolete]
public interface IPoolOwner
{
    List<PoolIdSO> GetPoolIds();
}
