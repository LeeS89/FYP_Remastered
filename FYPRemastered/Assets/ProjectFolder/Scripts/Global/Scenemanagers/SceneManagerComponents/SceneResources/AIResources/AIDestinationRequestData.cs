using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIDestinationRequestData : ProjectileResourceRequest
{
    public AIResourceType resourceType { get; set; }

    public BlockData blockData { get; set; }
    public Action<BlockData> waypointCallback { get; set; }


    public int numSteps { get; set; }
    public Action<List<Vector3>> FlankPointCandidatesCallback { get; set; }

    public AIDestinationType destinationType;
    public Vector3 start;
    public Vector3 end;
    public NavMeshPath path;
    public NavMeshObstacle obstacle;

    public Action<bool> internalCallback; // Callback for internal use, e.g., within the same system or component
    public Action<bool, Vector3> externalCallback; // Callback for external use, e.g., to notify original caller
    public Action carvingCallback; // Callback for carving operations, if needed
    public Action agentActiveCallback;
}
