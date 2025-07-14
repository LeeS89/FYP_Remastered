using UnityEngine;

public static class TargetingUtility
{
    public static Vector3 GetDirectionToTarget(GameObject target, GameObject from, bool getRandomOffset = false)
    {
        if(target == null || from == null)
        {
            return Vector3.zero;
        }

        Vector3 targetLocation = GetObjectLocation(target, getRandomOffset);
        Vector3 fromLocation = GetObjectLocation(from);

        return (targetLocation - fromLocation).normalized;
    }

    private static Vector3 GetObjectLocation(GameObject obj, bool getRandomOffset = false)
    {
        if(obj == null)
        {
            return Vector3.zero;
        }

        Vector3 location = obj.transform.position;

        if (getRandomOffset)
        {
            ApplyRandomOffset(obj, ref location);
        }

        return location;
    }

    private static void ApplyRandomOffset(GameObject target, ref Vector3 locationToOffset)
    {
        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider != null)
        {
            Vector3 boxSize = Vector3.Scale(collider.size, collider.transform.lossyScale) * 0.5f; 
            Vector3 randomOffset = new Vector3(
                Random.Range(-boxSize.x, boxSize.x),
                Random.Range(-boxSize.y, boxSize.y),
                Random.Range(-boxSize.z, boxSize.z)
            );

            Vector3 randomPoint = collider.center + randomOffset;

            locationToOffset += randomPoint;
        }
    }

    //Transfom Direction
    public static Vector3 GetDirectionToTarget(Transform target, Transform from, bool getRandomOffset = false)
    {
        if (target == null || from == null)
        {
            return Vector3.zero;
        }

        Vector3 targetLocation = GetObjectLocation(target, getRandomOffset);
        Vector3 fromLocation = GetObjectLocation(from);

        return (targetLocation - fromLocation).normalized;
    }

    private static Vector3 GetObjectLocation(Transform obj, bool getRandomOffset = false)
    {
        if (obj == null)
        {
            return Vector3.zero;
        }

        Vector3 location = obj.position;

        if (getRandomOffset)
        {
            ApplyRandomOffset(obj, ref location);
        }

        return location;
    }

    private static void ApplyRandomOffset(Transform target, ref Vector3 locationToOffset)
    {
        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider != null)
        {
            Vector3 boxSize = Vector3.Scale(collider.size, collider.transform.lossyScale) * 0.5f; // Half extent like UE
            Vector3 randomOffset = new Vector3(
                Random.Range(-boxSize.x, boxSize.x),
                Random.Range(-boxSize.y, boxSize.y),
                Random.Range(-boxSize.z, boxSize.z)
            );

            Vector3 randomPoint = collider.center + randomOffset;

            locationToOffset += randomPoint;
        }
    }


    public static Vector3 GetDirectionToTarget(Vector3 target, Vector3 from, bool getRandomOffset = false)
    {
        if (target == null || from == null)
        {
            return Vector3.zero;
        }

        //Vector3 targetLocation = GetObjectLocation(target, getRandomOffset);
        //Vector3 fromLocation = GetObjectLocation(from);

        return (target - from).normalized;
    }





}
