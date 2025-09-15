using UnityEngine;

[CreateAssetMenu(fileName = "PoolIdSO", menuName = "Pools/PoolIdSO")]
public class PoolIdSO : ScriptableObject
{
    [SerializeField] private string _id;
    public string Id => _id;
}
