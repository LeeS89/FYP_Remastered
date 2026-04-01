using UnityEngine;

public enum TargetingMode { Self, Raycast, Cone, Sphere}

[System.Serializable]
public class TargetingDef
{
    public TargetingMode Mode;
    public float Range;
    public float AngleDeg;
    public float Radius;
    public LayerMask hitMask;
    public int Maxhits = 32;
}
