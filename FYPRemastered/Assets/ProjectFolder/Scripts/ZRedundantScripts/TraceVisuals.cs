
using UnityEngine;

public class TraceVisuals : MonoBehaviour
{
    public Vector3 waistPos;// = transform.position + Vector3.up * waistHeight;
    public Vector3 eyePos;// = transform.position + Vector3.up * eyeHeight;

    private void Update()
    {
        float waistHeight = 1.1f;
        float eyeHeight = 1.8f;

        Vector3 waistPos = transform.position + Vector3.up * waistHeight;
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
       // Debug.DrawLine(waistPos, waistPos + direction * maxDistance, Color.yellow);
        DebugExtension.DebugWireSphere(waistPos, Color.yellow, 0.1f);
        DebugExtension.DebugWireSphere(eyePos, Color.black, 0.1f);
        DebugExtension.DebugCapsule(waistPos, eyePos, Color.cyan, 0.4f);
    }
}
