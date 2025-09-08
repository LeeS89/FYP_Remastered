using UnityEngine;


public class TestAddColl : MonoBehaviour, IFreezeAndDeflectable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnsureCollider();
        //AddColliderIfNoneExists<SphereCollider>();
    }


    private void EnsureCollider()
    {

        var existingCol = GetComponentInChildren<Collider>(true);

        if (existingCol != null)
        {
            CreateDefaultColliderIfNoneExists(existingCol.gameObject, exists: true);
            return;
        }

        var mr = GetComponentInChildren<MeshRenderer>(true);
        var mf = GetComponentInChildren<MeshFilter>(true);

        var meshObj = mr != null ? mr.gameObject
                    : mf != null ? mf.gameObject
                    : gameObject;


        CreateDefaultColliderIfNoneExists(meshObj, exists: false);
    }

    protected virtual void CreateDefaultColliderIfNoneExists(GameObject target, bool exists)
    {
        if (exists) return;
        var col = target.AddComponent<SphereCollider>();
        col.isTrigger = true;
    }

















   

    public void Deflect(ProjectileKickType type)
    {
        throw new System.NotImplementedException();
    }

    public void Freeze()
    {
        throw new System.NotImplementedException();
    }
}
