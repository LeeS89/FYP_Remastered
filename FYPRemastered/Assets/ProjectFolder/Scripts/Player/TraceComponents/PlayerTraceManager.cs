using UnityEngine;

public class PlayerTraceManager: TraceComponent
{
    private Collider[] _overlapResults;

    public PlayerTraceManager(int maxOverlapResults)
    {
        _overlapResults = new Collider[maxOverlapResults];
    }


    public void CheckTargetProximity<T>(Transform traceLocation, T[] buffer, ref int count, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false) where T : class
    {

        bool foundObject = IsTargetWithinRange(traceLocation.position, sphereRadius, traceLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (debug)
        {

            DebugExtension.DebugWireSphere(traceLocation.position, debugColor, sphereRadius);

        }
       

        if (foundObject)
        {

            int hitCount = Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, _overlapResults, traceLayer);

            for (int i = 0; i < hitCount && count < buffer.Length; i++)
            {
                var go = _overlapResults[i].gameObject;
                if (go == null || !ComponentRegistry.TryGet<T>(go, out var comp)) continue;
                bool duplicate = false;
                for (int j = 0; j < count; j++) if (ReferenceEquals(buffer[j], comp)) { duplicate = true; break; }
                if (!duplicate)
                {
                    buffer[count++] = comp;
                }
            }
        }

    }

    public override void OnInstanceDestroyed()
    {
        _overlapResults = null;
    }
}
