using UnityEngine;

namespace Npc.API
{

    [System.Serializable]
    public sealed class MovementConfig
    {
        [Header("Agent Base Speeds")]
        public float walkSpeed = 0.9f;
        public float sprintSpeed = 3.6f;

        [Header("Sprint/ Walk thresholds")]
        public float sprintEnterDistance = 15;
        public float sprintExitDistance = 12;

        [Header("Stopping - When remaining distance is <= stopping distance + threshold")]
        public float stopDistancethreshold = 0.25f;

        [Header("Path status check interval")]
        public float pathStatusInterval = 0.1f;

        [Header("Lerp Settings")]
        public float idleLerp = 10f;
        public float moveLerp = 2f;
    }

    [System.Serializable]
    public sealed class FlankStateConfig
    {
        [Range(5, 15)]
        public int maxFlankSteps = 12;
        [Min(4)]
        public int minFlankSteps;
    }

    [System.Serializable]
    public sealed class ChaseStateConfig
    {
        [Min(4)]
        public float minStoppingdistance = 4;
        [Range(5, 15)]
        public float maxStoppingdistance = 12;
    }

    [System.Serializable]
    public sealed class PatrolStateConfig
    {
        [Range(0.5f, 15f)]
        [SerializeField] public float maxTimeAtWaypoint;
        [Min(0.5f)]
        [SerializeField] public float minTimeAtWaypoint;
    }
}

