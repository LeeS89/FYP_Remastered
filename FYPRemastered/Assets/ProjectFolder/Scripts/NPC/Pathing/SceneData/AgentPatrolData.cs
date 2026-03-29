using UnityEngine;

[CreateAssetMenu(fileName = "AgentPatrolData", menuName = "Scriptable Objects/AgentPatrolData")]
public class AgentPatrolData : ScriptableObject
{
    [SerializeField, Range(0.5f, 15f)] private float _maxTimeAtPatrolPoint;
    [SerializeField, Min(0.5f)] private float _minTimeAtPatrolPoint;

    public float MaxTimeAtPatrolPoint => _maxTimeAtPatrolPoint;
    public float MinTimeAtPatrolPoint => _minTimeAtPatrolPoint;
}
