/*using System;
using UnityEngine;
using UnityEngine.AI;

[Obsolete("Moving to Resource Request")]
public class DestinationRequestData
{
    public DestinationType destinationType;
    public Vector3 start;
    public Vector3 end;
    public NavMeshPath path;
    public NavMeshObstacle obstacle; 

    public Action<bool> internalCallback; // Callback for internal use, e.g., within the same system or component
    public Action<bool, Vector3> externalCallback; // Callback for external use, e.g., to notify original caller
    public Action carvingCallback; // Callback for carving operations, if needed
    public Action agentActiveCallback;
}*/