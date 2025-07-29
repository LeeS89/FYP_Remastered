using UnityEngine;

[System.Serializable]
public class BlockData
{
    public GameObject _block = null;
    public Vector3 _blockPosition;          // Store position of the block
    public Vector3[] _waypointPositions;   // Store positions of all waypoints
    public Vector3[] _waypointForwards;
    public int _blockZone;                 // The zone this block belongs to
    public bool _inUse = false;            // Whether the block is in use or not
}