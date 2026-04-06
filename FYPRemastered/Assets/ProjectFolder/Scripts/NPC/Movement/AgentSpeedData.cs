using UnityEngine;

[CreateAssetMenu(fileName = "AgentSpeedData", menuName = "Scriptable Objects/AgentSpeedData")]
public class AgentSpeedData : ScriptableObject
{
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _sprintEnterdistance;
    [SerializeField] private float _sprintExitdistance;

    public float WalkSpeed => _walkSpeed;
    public float SprintSpeed => _sprintSpeed;
    public float SprintEnterDistance => _sprintEnterdistance;
    public float SprintExitDistance => _sprintExitdistance;
}
