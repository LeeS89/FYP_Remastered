using UnityEngine;

public readonly struct MovementPolicy
{
    public readonly MovementIntent MoveIntent;
    public readonly float Stoppingdistance;
    public readonly bool UseRandomStoppingdistance;
    public readonly float MinStoppingdistance;
    public readonly float MaxStoppingdistance;

    public MovementPolicy(MovementIntent intent, float stoppingdistance, bool useRandomStoppingdistance = false, float minStopdist = 0f, float maxStopdist = 12f)
    {
        MoveIntent = intent;
        Stoppingdistance = stoppingdistance;
        UseRandomStoppingdistance = useRandomStoppingdistance;
        MinStoppingdistance = minStopdist;
        MaxStoppingdistance = maxStopdist;
    }
}

public readonly struct EngagementPolicy
{
    public readonly Transform Target;
}
