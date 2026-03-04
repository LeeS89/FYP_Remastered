using UnityEngine;

[CreateAssetMenu(fileName = "AgentChaseData", menuName = "Scriptable Objects/AgentChaseData")]
public class AgentChaseData : ScriptableObject
{
    [SerializeField] private float _minStoppingDistance;
    [SerializeField] private float _maxStoppingDistance;

    public float MinStoppingDistance => _minStoppingDistance;
    public float MaxStoppingDistance => _maxStoppingDistance;
}
