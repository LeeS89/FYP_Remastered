using System.Collections.Generic;
using UnityEngine;

public interface IPoolOwner
{
    List<PoolIdSO> GetPoolIds();
}
