using UnityEngine;

[CreateAssetMenu(fileName = "AgentSpeedData", menuName = "Scriptable Objects/AgentSpeedData")]
public class AgentSpeedData : ScriptableObject
{
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _sprintSpeed;

    public float WalkSpeed => _walkSpeed;
    public float SprintSpeed => _sprintSpeed;
}
