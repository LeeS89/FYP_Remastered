using System.Collections.Generic;
using UnityEngine;

public class EnemyStatHandler : StatHandler, IEnemyDamageable
{
    public SkinnedMeshRenderer skinnedRenderer;
    public Dictionary<string, Transform> limbBoneMap;

    public void HandleHit(Collider hitCollider, Vector3 rawHitPoint)
    {
        string tag = hitCollider.tag;

        if (!limbBoneMap.TryGetValue(tag, out Transform bone))
        {
            Debug.LogWarning("Bone not found for tag: " + tag);
            return;
        }

        Vector3 refinedPoint = FindClosestVertexNearBone(skinnedRenderer, bone, rawHitPoint);
        //SpawnHitEffect(refinedPoint);
        //ApplyDamage(10f); // Use limb-based damage if you like
    }

    private Vector3 FindClosestVertexNearBone(SkinnedMeshRenderer smr, Transform bone, Vector3 hitPoint)
    {
        Mesh baked = new Mesh();
        smr.BakeMesh(baked);
        Vector3[] vertices = baked.vertices;

        Vector3 closest = Vector3.zero;
        float closestDist = float.MaxValue;

        for (int i = 0; i < vertices.Length; i += 3)
        {
            Vector3 worldPoint = smr.transform.TransformPoint(vertices[i]);

            if ((worldPoint - bone.position).sqrMagnitude > 0.3f * 0.3f) continue;

            float dist = (worldPoint - hitPoint).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = worldPoint;
            }
        }

        return closest;
    }
}
