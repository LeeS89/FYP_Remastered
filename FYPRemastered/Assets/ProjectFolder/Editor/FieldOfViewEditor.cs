using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyFSMController))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
       /* CombatComponent fov = (CombatComponent)target;

        if (fov._fovLocation == null) return;

        Vector3 origin = fov._fovLocation.position;
        float viewRadius = fov._proximityRadius;
        float viewAngle = fov._fovViewangle;

        // Draw vision radius
        Handles.color = Color.white;
        Handles.DrawWireArc(origin, Vector3.up, Vector3.forward, 360, viewRadius);

        // Calculate direction vectors
        Vector3 viewAngle01 = DirectionFromAngle(fov._fovLocation.eulerAngles.y, -viewAngle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov._fovLocation.eulerAngles.y, viewAngle / 2);

        // Draw FOV cone boundaries
        Handles.color = Color.red;
        Handles.DrawLine(origin, origin + viewAngle01 * viewRadius);
        Handles.DrawLine(origin, origin + viewAngle02 * viewRadius);*/

        // Optional: show line to player if visible
        /*if (fov._canSeePlayer && fov.playerRef != null)
        {
            Handles.color = Color.green;
            Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);
        }*/
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }
}
