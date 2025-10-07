using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoolIdSO", menuName = "Pools/PoolIdSO")]
public class PoolIdSO : ScriptableObject
{
    [SerializeField] private string _id;
    public string Id => _id;

    [SerializeField] private PoolKind _kind;
    public int _prewarmCount;

    public PoolKind Kind => _kind;

    public List<PoolIdSO> _fxPools;
    public PoolIdSO _audioPoolId;
    public PoolIdSO _particlePoolId;
}
