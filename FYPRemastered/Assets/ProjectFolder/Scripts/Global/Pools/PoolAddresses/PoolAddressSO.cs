using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum PoolKind { GameObject, Audio, ParticleSystem}

[CreateAssetMenu(fileName = "PoolIdSO", menuName = "Pools/PoolAddressSO")]
public class PoolAddressSO : ScriptableObject
{
    public AssetReferenceGameObject AddressableReference;
   
    [SerializeField] private string _addressableKey;
    [SerializeField] private PoolIdSO _id;
    [SerializeField] private PoolKind _kind;
    [SerializeField] private int _prewarmCount = 0;
    public string Key => _addressableKey;
    public PoolKind Kind => _kind;

    public PoolIdSO Id => _id;

    public int PrewarmSize => _prewarmCount;
}
