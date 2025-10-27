using UnityEngine;

public readonly struct FSMPolicy
{
    public readonly MovementIntent MoveIntent;
    public readonly bool UseRandomStoppingdistance;
    public readonly float MinStoppingdistance;
    public readonly float MaxStoppingdistance;
    public readonly uint Version;

    public FSMPolicy(uint version, MovementIntent intent, bool useRandomStoppingdistance = false, float minStopdist = 5f, float maxStopdist = 12f)
    {
        Version = version;
        MoveIntent = intent;
        UseRandomStoppingdistance = useRandomStoppingdistance;
        MinStoppingdistance = minStopdist;
        MaxStoppingdistance = maxStopdist;
    }
}

